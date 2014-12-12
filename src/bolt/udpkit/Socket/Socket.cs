﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace UdpKit {
  public enum UdpSocketState : int {
    Created = 0,
    Running = 1,
    Shutdown = 2
  }

  public enum UdpSocketMode : int {
    None = 0,
    Host = 1,
    Client = 2
  }

  public partial class UdpSocket {
    public static int MaxTokenSize {
      get { return 768; }
    }

    readonly internal UdpConfig Config;
    readonly internal UdpPipeConfig PacketPipeConfig;
    readonly internal UdpPipeConfig StreamPipeConfig;

    volatile int frame;
    volatile int channelIdCounter;
    volatile uint connectionIdCounter;
    volatile UdpSocketMode mode;
    volatile UdpSocketState state;

    readonly byte[] sendBuffer;
    readonly byte[] recvBuffer;

    readonly Thread thread;
    readonly UdpPlatform platform;
    readonly Protocol.Peer platformSocketPeer;
    readonly UdpPlatformSocket platformSocket;

    readonly UdpPacketPool packetPool;
    readonly AutoResetEvent availableEvent;
    readonly Queue<UdpEvent> eventQueueIn;
    readonly Queue<UdpEvent> eventQueueOut;
    readonly SessionHandler sessionHandler;
    readonly BroadcastHandler broadcastHandler;
    readonly List<UdpConnection> connectionList = new List<UdpConnection>();
    readonly UdpSet<UdpEndPoint> pendingConnections = new UdpSet<UdpEndPoint>(new UdpEndPoint.Comparer());
    readonly Dictionary<UdpEndPoint, UdpConnection> connectionLookup = new Dictionary<UdpEndPoint, UdpConnection>(new UdpEndPoint.Comparer());
    readonly Dictionary<UdpChannelName, UdpStreamChannel> streamChannels = new Dictionary<UdpChannelName, UdpStreamChannel>(UdpChannelName.EqualityComparer.Instance);

    /// <summary>
    /// Local endpoint of this socket
    /// </summary>
    public UdpEndPoint LocalEndPoint {
      get {
        return platformSocket.EndPoint;
      }
    }

    /// <summary>
    /// The current state of the socket
    /// </summary>
    public UdpSocketState State {
      get { return state; }
    }

    /// <summary>
    /// The current mode of the socket
    /// </summary>
    public UdpSocketMode Mode {
      get { return mode; }
    }

    /// <summary>
    /// The precision time (in ms) of the underlying socket platform
    /// </summary>
    public uint PrecisionTime {
      get { return GetCurrentTime(); }
    }

    public UdpPacketPool PacketPool {
      get { return packetPool; }
    }

    /// <summary>
    /// A thread can wait on this event before calling Poll to make sure at least one event is available
    /// </summary>
    public AutoResetEvent EventsAvailable {
      get { return availableEvent; }
    }

    /// <summary>
    /// A user-assignable object
    /// </summary>
    public object UserToken {
      get;
      set;
    }

    public UdpSocket(UdpPlatform platform)
      : this(platform, new UdpConfig()) {
    }

    public UdpSocket(UdpPlatform platform, UdpConfig config) {
      this.Config = config.Duplicate();

      this.platform = platform;
      this.platformSocket = platform.CreateSocket();

      state = UdpSocketState.Created;
      availableEvent = new AutoResetEvent(false);
      sessionHandler = new SessionHandler();
      broadcastHandler = new BroadcastHandler(this);
      connectionIdCounter = 1;

#if DEBUG
      if (this.Config.NoiseFunction == null) {
        Random random = new Random();
        this.Config.NoiseFunction = delegate() { return (float)random.NextDouble(); };
      }
#endif

      platformSocketPeer = new Protocol.Peer();
      platformSocketPeer.Ack_AddHandler<Protocol.NatPunch_PeerRegister>(Socket_NatPunch_PeerRegister_Ack);
      platformSocketPeer.Ack_AddHandler<Protocol.Socket_Punch>(Socket_Punch_Ack);

      platformSocketPeer.Message_AddHandler<Protocol.Socket_Ping>(Socket_Ping);
      platformSocketPeer.Message_AddHandler<Protocol.Socket_Punch>(Socket_Punch);
      platformSocketPeer.Message_AddHandler<Protocol.NatPunch_PunchInfo>(NatPunch_PunchInfo);

      sendBuffer = new byte[Math.Max(config.StreamDatagramSize, config.PacketDatagramSize) * 2];
      recvBuffer = new byte[Math.Max(config.StreamDatagramSize, config.PacketDatagramSize) * 2];

      packetPool = new UdpPacketPool(this);
      eventQueueIn = new Queue<UdpEvent>(config.InitialEventQueueSize);
      eventQueueOut = new Queue<UdpEvent>(config.InitialEventQueueSize);

      thread = new Thread(NetworkLoop);
      thread.Name = "UdpKit Background Thread";
      thread.IsBackground = true;
      thread.Start();

      // setup packet pipe configuration
      PacketPipeConfig = new UdpPipeConfig {
        PipeId = UdpPipe.PIPE_PACKET,
        Timeout = 0, // don't use timeout
        AckBytes = 8,
        SequenceBytes = 2,
        UpdatePing = true,
        WindowSize = Config.PacketWindow,
        DatagramSize = Config.PacketDatagramSize,
      };

      // setup stream pipe config
      StreamPipeConfig = new UdpPipeConfig {
        PipeId = UdpPipe.PIPE_STREAM,
        Timeout = 500,
        AckBytes = 32,
        SequenceBytes = 3,
        UpdatePing = false,
        WindowSize = Config.StreamWindow,
        DatagramSize = config.StreamDatagramSize
      };
    }


    /// <summary>
    /// Start this socket
    /// </summary>
    /// <param name="endpoint">The endpoint to bind to</param>
    public void Start(UdpEndPoint endpoint, UdpSocketMode mode) {
      UdpEvent ev = new UdpEvent();
      ev.Type = UdpEvent.INTERNAL_START;
      ev.EndPoint = endpoint;
      ev.SocketMode = mode;

      Raise(ev);
    }

    /// <summary>
    /// Close this socket
    /// </summary>
    public void Close() {
      Raise(UdpEvent.INTERNAL_CLOSE);
    }

    /// <summary>
    /// Connect to remote endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint to connect to</param>
    public void Connect(UdpEndPoint endpoint) {
      Connect(endpoint, null);
    }

    public void Connect(UdpSession session) {
      Connect(session, null);
    }

    public void Connect(UdpSession session, byte[] connectToken) {
      UdpEvent ev = new UdpEvent();
      ev.Type = UdpEvent.INTERNAL_SESSION_CONNECT;
      ev.Session = session;
      ev.ConnectToken = connectToken;

      Raise(ev);
    }

    /// <summary>
    /// Connect to remote endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint to connect to</param>
    public void Connect(UdpEndPoint endpoint, byte[] connectToken) {
      UdpEvent ev = new UdpEvent();
      ev.Type = UdpEvent.INTERNAL_CONNECT;
      ev.EndPoint = endpoint;
      ev.ConnectToken = connectToken;
      Raise(ev);
    }

    /// <summary>
    /// Cancel ongoing attempt to connect to endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint to cancel connect attempt to</param>
    public void CancelConnect(UdpEndPoint endpoint) {
      Raise(UdpEvent.INTERNAL_CONNECT_CANCEL, endpoint);
    }

    /// <summary>
    /// Accept a connection
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="userObject"></param>
    /// <param name="acceptToken"></param>
    public void Accept(UdpEndPoint endpoint, object userObject, byte[] acceptToken, byte[] connectToken) {
      UdpEvent ev = new UdpEvent();
      ev.Type = UdpEvent.INTERNAL_ACCEPT;
      ev.EndPoint = endpoint;
      ev.ConnectToken = connectToken;
      ev.AcceptArgs = new UdpEventAcceptArgs { AcceptToken = acceptToken, UserObject = userObject };
      Raise(ev);
    }

    /// <summary>
    /// Refuse a connection request from a remote endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint to refuse</param>
    public void Refuse(UdpEndPoint endpoint, byte[] refuseToken) {
      UdpEvent ev = new UdpEvent();
      ev.Type = UdpEvent.INTERNAL_REFUSE;
      ev.EndPoint = endpoint;
      ev.RefusedToken = refuseToken;
      Raise(ev);
    }

    /// <summary>
    /// A list of all currently available sessions
    /// </summary>
    public UdpSession[] GetSessions() {
      return sessionHandler.Sessions.ToArray();
    }

    /// <summary>
    /// Poll socket for any events
    /// </summary>
    /// <param name="ev">The current event on this socket</param>
    /// <returns>True if a new event is available, False otherwise</returns>
    public bool Poll(out UdpEvent ev) {
      lock (eventQueueOut) {
        if (eventQueueOut.Count > 0) {
          ev = eventQueueOut.Dequeue();
          return true;
        }
      }

      ev = default(UdpEvent);
      return false;
    }

    public void MasterServerSet(UdpEndPoint endpoint) {
      UdpEvent ev = new UdpEvent();
      ev.Type = UdpEvent.INTERNAL_MASTERSERVER_SET;
      ev.EndPoint = endpoint;
      Raise(ev);
    }

    public void MasterServerRequestSessionList() {
      Raise(UdpEvent.INTERNAL_MASTERSERVER_SESSION_LISTREQUEST);
    }

    public void LanBroadcastEnable(UdpEndPoint endpoint) {
      UdpEvent ev = new UdpEvent();
      ev.Type = UdpEvent.INTERNAL_LANBROADCAST_ENABLE;
      ev.EndPoint = endpoint;
      Raise(ev);
    }

    public void LanBroadcastDisable() {
      Raise(UdpEvent.INTERNAL_LANBROADCAST_DISABLE);
    }

    public void ForgetLanSessions() {
      Raise(UdpEvent.INTERNAL_LANBROADCAST_FORGETSESSIONS);
    }

    public void SetHostInfo(string serverName, byte[] userData) {
      UdpEvent ev = new UdpEvent();
      ev.Type = UdpEvent.INTERNAL_SESSION_HOST_SETINFO;
      ev.HostName = serverName;
      ev.HostData = userData;
      Raise(ev);
    }

    internal void Raise(int eventType) {
      UdpEvent ev = new UdpEvent();
      ev.Type = eventType;
      Raise(ev);
    }

    internal void Raise(int eventType, UdpEndPoint endpoint) {
      UdpEvent ev = new UdpEvent();
      ev.Type = eventType;
      ev.EndPoint = endpoint;
      Raise(ev);
    }

    internal void Raise(int eventType, UdpConnection connection) {
      UdpEvent ev = new UdpEvent();
      ev.Type = eventType;
      ev.Connection = connection;
      Raise(ev);
    }

    internal void Raise(int eventType, UdpConnection connection, UdpPacket packet) {
      UdpEvent ev = new UdpEvent();
      ev.Type = eventType;
      ev.Connection = connection;
      ev.Packet = packet;
      Raise(ev);
    }

    internal void Raise(UdpConnection connection, UdpPacket packet, UdpSendFailReason reason) {
      UdpEvent ev = new UdpEvent();
      ev.Type = UdpEvent.PUBLIC_PACKET_LOST;
      ev.Connection = connection;
      ev.Packet = packet;
      ev.FailedReason = reason;
      Raise(ev);
    }

    internal bool FindChannel(int id, out UdpStreamChannel channel) {
      return streamChannels.TryGetValue(new UdpChannelName(id), out channel);
    }

    internal byte[] GetSendBuffer() {
      Array.Clear(sendBuffer, 0, sendBuffer.Length);
      return sendBuffer;
    }

    internal byte[] GetRecvBuffer() {
      Array.Clear(recvBuffer, 0, recvBuffer.Length);
      return recvBuffer;
    }

    internal uint GetCurrentTime() {
      return platform.GetPrecisionTime();
    }

    internal void Raise(UdpEvent ev) {
      if (ev.IsInternal) {
        lock (eventQueueIn) {
          eventQueueIn.Enqueue(ev);
        }
      }
      else {
        lock (eventQueueOut) {
          eventQueueOut.Enqueue(ev);
        }

        if (Config.UseAvailableEventEvent) {
          availableEvent.Set();
        }
      }
    }

    internal bool Send(UdpEndPoint endpoint, byte[] buffer, int length) {
      if (state == UdpSocketState.Running || state == UdpSocketState.Created) {
        return platformSocket.SendTo(buffer, length, endpoint) == length;
      }

      return false;
    }

    internal void SendCommand(UdpEndPoint endpoint, byte cmd) {
      SendCommand(endpoint, cmd, null);
    }

    internal void SendCommand(UdpEndPoint endpoint, byte cmd, byte[] data) {
      int size = 2;

      byte[] buffer = GetSendBuffer();
      buffer[0] = UdpPipe.PIPE_COMMAND;
      buffer[1] = cmd;

      if (data != null) {
        // copy into buffer
        Array.Copy(data, 0, buffer, 2, data.Length);

        // add size
        size += data.Length;
      }

      Send(endpoint, buffer, size);
    }

    bool ChangeState(UdpSocketState from, UdpSocketState to) {
      if (CheckState(from)) {
        state = to;
        return true;
      }

      return false;
    }

    bool CheckState(UdpSocketState s) {
      if (state != s) {
        return false;
      }

      return true;
    }

    void NetworkLoop() {
      bool created = false;
      bool started = false;

      while (state == UdpSocketState.Created || state == UdpSocketState.Running) {
        try {
          if (created == false) {
            UdpLog.Info("socket created");
            created = true;
          }

          while (state == UdpSocketState.Created) {
            ProcessStartEvent();
            Thread.Sleep(1);
          }

          if (started == false) {
            UdpLog.Info("physical socket started");
            started = true;
          }

          while (state == UdpSocketState.Running) {
            uint now = GetCurrentTime();

            RecvDelayedPackets();
            RecvNetworkData();
            ProcessTimeouts();
            ProcessLanBroadcastDiscovery();
            ProcessInternalEvents();

            MasterServer_Update(now);
            NatProbe_Update(now);
            Session_Update(now);
            Socket_Update(now);


            frame += 1;
          }

          UdpLog.Info("socket closed");
          return;
        }
        catch (Exception exn) {
          UdpLog.Error(exn.ToString());
        }
      }
    }

    bool CreatePhysicalSocket(UdpEndPoint ep, UdpSocketState s) {
      UdpLog.Info("Binding physical socket using platform '{0}'", platform.GetType());

      if (ChangeState(UdpSocketState.Created, s)) {
        platformSocket.Bind(ep);

        if (platformSocket.IsBound) {
          UdpLog.Info("Physical socket bound to {0}", platformSocket.EndPoint.ToString());
          return true;
        }
        else {
          ChangeState(s, UdpSocketState.Shutdown);
          UdpLog.Error("Could not bind physical socket, platform error: {0}", platformSocket.Error);
        }
      }
      else {
        UdpLog.Error("Socket has incorrect state: {0}", state);
      }

      return false;
    }

    void AcceptConnection(UdpEndPoint ep, object userToken, byte[] acceptToken, byte[] connectToken) {
      UdpConnection cn = CreateConnection(ep, UdpConnectionMode.Server, connectToken);
      cn.UserToken = userToken;
      cn.AcceptToken = acceptToken;
      cn.ConnectionId = ++connectionIdCounter;

      if (cn.AcceptToken == null) {
        cn.AcceptTokenWithPrefix = BitConverter.GetBytes(cn.ConnectionId);
      }
      else {
        cn.AcceptTokenWithPrefix = new byte[cn.AcceptToken.Length + 4];

        // copy connection id into first 4 bytes of accept token (with prefix)
        Buffer.BlockCopy(BitConverter.GetBytes(cn.ConnectionId), 0, cn.AcceptTokenWithPrefix, 0, 4);

        // copy full accept token into bytes after connection id
        Buffer.BlockCopy(cn.AcceptToken, 0, cn.AcceptTokenWithPrefix, 4, cn.AcceptToken.Length);
      }

      cn.ChangeState(UdpConnectionState.Connected);
    }

    void ProcessTimeouts() {
      if ((frame & 3) == 3) {
        uint now = GetCurrentTime();

        for (int i = 0; i < connectionList.Count; ++i) {
          UdpConnection cn = connectionList[i];

          switch (cn.State) {
            case UdpConnectionState.Connecting:
              cn.ProcessConnectingTimeouts(now);
              break;

            case UdpConnectionState.Connected:
              cn.ProcessConnectedTimeouts(now);
              break;

            case UdpConnectionState.Disconnected:
              cn.ChangeState(UdpConnectionState.Destroy);
              break;

            case UdpConnectionState.Destroy:
              if (DestroyConnection(cn)) {
                --i;
              }
              break;
          }
        }
      }
    }

    void RecvNetworkData() {
      if (platformSocket.RecvPoll(1)) {
        var endpoint = UdpEndPoint.Any;
        var buffer = GetRecvBuffer();
        var bytes = platformSocket.RecvFrom(buffer, ref endpoint);

        if (bytes > 0) {
#if DEBUG
          if (ShouldDropPacket) {
            return;
          }

          if (ShouldDelayPacket) {
            DelayPacket(endpoint, buffer, bytes);
            return;
          }
#endif
          RecvNetworkPacket(endpoint, buffer, bytes);
        }
      }
    }

    void RecvNetworkPacket(UdpEndPoint ep, byte[] buffer, int bytes) {
      switch (buffer[0]) {
        case UdpPipe.PIPE_COMMAND:
          RecvCommand(ep, buffer, bytes);
          break;

        case UdpPipe.PIPE_PACKET:
          RecvPacket(ep, buffer, bytes);
          break;

        case UdpPipe.PIPE_STREAM:
          RecvStream(ep, buffer, bytes);
          break;

        case Protocol.Message.MESSAGE_HEADER:
          RecvProtocol(ep, buffer, bytes);
          break;
      }
    }

    void RecvProtocol(UdpEndPoint endpoint, byte[] buffer, int bytes) {
      var off = 0;
      var msg = platform.ParseMessage(buffer, ref off);

      if (msg != null) {
        msg.Sender = endpoint;

        if ((MasterServer != null) && MasterServer.Peer.HasHandler(msg)) {
          MasterServer.Peer.Message_Recv(msg);
          return;
        }

        if ((NatProbe != null) && NatProbe.Peer.HasHandler(msg)) {
          NatProbe.Peer.Message_Recv(msg);
          return;
        }

        if ((platformSocketPeer != null) && platformSocketPeer.HasHandler(msg)) {
          platformSocketPeer.Message_Recv(msg);
          return;
        }
      }
    }

    void RecvCommand(UdpEndPoint ep, byte[] buffer, int size) {
      UdpConnection cn;

      if (connectionLookup.TryGetValue(ep, out cn)) {
        cn.OnCommandReceived(buffer, size);
      }
      else {
        RecvConnectionCommand_Unconnected(ep, buffer, size);
      }
    }

    void RecvStream(UdpEndPoint ep, byte[] buffer, int bytes) {
      UdpConnection cn;

      if (connectionLookup.TryGetValue(ep, out cn)) {
        cn.OnStreamReceived(buffer, bytes);
      }
    }

    void RecvPacket(UdpEndPoint ep, byte[] buffer, int size) {
      UdpConnection cn;

      if (connectionLookup.TryGetValue(ep, out cn)) {
        cn.OnPacketReceived(buffer, size);
      }
    }

    void AddPendingConnection(UdpEndPoint endpoint, byte[] connectToken) {
      if (pendingConnections.Add(endpoint)) {
        UdpEvent ev = new UdpEvent();
        ev.Type = UdpEvent.PUBLIC_CONNECT_REQUEST;
        ev.EndPoint = endpoint;
        ev.ConnectToken = connectToken;

        Raise(ev);
      }
    }

    UdpConnection CreateConnection(UdpEndPoint endpoint, UdpConnectionMode mode, byte[] connectToken) {
      if (connectionLookup.ContainsKey(endpoint)) {
        UdpLog.Warn("connection for {0} already exists", endpoint);
        return default(UdpConnection);
      }

      UdpConnection cn;

      cn = new UdpConnection(this, mode, endpoint);
      cn.ConnectToken = connectToken;

      connectionLookup.Add(endpoint, cn);
      connectionList.Add(cn);

      return cn;
    }

    bool DestroyConnection(UdpConnection cn) {
      for (int i = 0; i < connectionList.Count; ++i) {
        if (connectionList[i] == cn) {
          connectionList.RemoveAt(i);
          connectionLookup.Remove(cn.RemoteEndPoint);

          cn.Destroy();

          return true;
        }
      }

      return false;
    }

    void RecvConnectionCommand_Unconnected(UdpEndPoint endpoint, byte[] buffer, int size) {
      if (buffer[1] == UdpConnection.COMMAND_CONNECT) {
        byte[] connectToken = UdpUtils.ReadToken(buffer, size, 2);

        if (Config.AllowIncommingConnections && ((connectionLookup.Count + pendingConnections.Count) < Config.ConnectionLimit || Config.ConnectionLimit == -1)) {
          if (Config.AutoAcceptIncommingConnections) {
            AcceptConnection(endpoint, null, null, connectToken);
          }
          else {
            AddPendingConnection(endpoint, connectToken);
          }
        }
        else {
          SendCommand(endpoint, UdpConnection.COMMAND_REFUSED);
        }
      }
    }
  }
}