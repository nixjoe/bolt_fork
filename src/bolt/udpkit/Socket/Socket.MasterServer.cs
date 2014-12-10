﻿using System;
using System.Collections.Generic;

using System.Text;

namespace UdpKit {
  partial class UdpSocket {
    const uint MASTERSERVER_UPDATERATE = 50;
    const uint MASTERSERVER_SESSION_LISTREQUEST_TIMEOUT = 500;

    const uint MASTERSERVER_HOST_REGISTER_TIMEOUT = 5000;
    const uint MASTERSERVER_HOST_KEEPALIVE_TIMEOUT = 20000;

    class MasterServerInfo {
      public Protocol.Peer Peer;
      public UdpEndPoint EndPoint;
      public uint LastUpdate;
      public UdpSession IntroduceHost;
    }

    MasterServerInfo MasterServer = new MasterServerInfo();

    void MasterServer_Set(UdpEndPoint endpoint) {
      MasterServer = new MasterServerInfo();
      MasterServer.EndPoint = endpoint;

      MasterServer.Peer = new Protocol.Peer(platformSocket);
      MasterServer.Peer.Message_AddHandler<Protocol.MasterServer_Session_Info>(MasterServer_Session_Info);
      MasterServer.Peer.Message_AddHandler<Protocol.MasterServer_IntroduceInfo>(MasterServer_IntroduceInfo_Ack);

      MasterServer.Peer.Ack_AddHandler<Protocol.MasterServer_Introduce>(MasterServer_Introduce);
      MasterServer.Peer.Ack_AddHandler<Protocol.MasterServer_NatProbeInfo>(MasterServer_NatProbeInfo_Ack);
      MasterServer.Peer.Ack_AddHandler<Protocol.MasterServer_HostRegister>(MasterServer_HostRegister_Ack);

      // reset nat probe client
      NatProbe_Reset();

      // request nat probe info from new master server
      MasterServer.Peer.Message_Send<Protocol.MasterServer_NatProbeInfo>(MasterServer.EndPoint);
    }


    void MasterServer_Update(uint now) {
      if (MasterServer.EndPoint == UdpEndPoint.Any) {
        return;
      }

      if ((MasterServer.LastUpdate + MASTERSERVER_UPDATERATE) < now) {
        // update protocol peer
        MasterServer.Peer.Query_Update(now);

        //
        switch (mode) {
          case UdpSocketMode.Host: MasterServer_UpdateHost(now); break;
          case UdpSocketMode.Client: MasterServer_UpdateClient(now); break;
        }

        //
        MasterServer.LastUpdate = now;
      }
    }

    void MasterServer_UpdateHost(uint now) {
      if ((MasterServer.Peer.LastSend + MASTERSERVER_HOST_KEEPALIVE_TIMEOUT) < now) {
        MasterServer.Peer.Message_Send(platform.CreateMessage<Protocol.MasterServer_HostKeepAlive>(), MasterServer.EndPoint);
      }

      if (NAT_UPnP_Result != Session.Local.UPnP_Result) {
        UdpLog.Debug("NAT_UPnP_Result = {0}", NAT_UPnP_Result);
        Session.Local.UPnP_Result = NAT_UPnP_Result;
        MasterServer_HostRegister();
      }

    }

    void MasterServer_UpdateClient(uint now) {

    }

    void MasterServer_Connect(UdpSession session, byte[] token) {
      // only allow wan sessions
      UdpAssert.Assert(session.IsWan);

      if (MasterServer.EndPoint == UdpEndPoint.Any) {
        return;
      }

      // simple case where the host allows direct connections
      if (session.ConnectivityStatus == UdpConnectivityStatus.DirectConnection) {
        ConnectToEndPoint(session.WanEndPoint, token);
        return;
      }

      Protocol.MasterServer_Introduce query;

      query = MasterServer.Peer.Message_Create<Protocol.MasterServer_Introduce>();
      query.Client = Session.Local;
      query.Host = session;

      MasterServer.Peer.Message_Send(query, MasterServer.EndPoint);
    }

    void MasterServer_HostRegister() {
      if (MasterServer.EndPoint == UdpEndPoint.Any) {
        return;
      }

      Protocol.MasterServer_HostRegister query;

      query = MasterServer.Peer.Message_Create<Protocol.MasterServer_HostRegister>();
      query.Host = Session.Local;

      MasterServer.Peer.Message_Send(query, MasterServer.EndPoint);
    }

    void MasterServer_Session_ListRequest() {
      if (MasterServer.EndPoint == UdpEndPoint.Any) {
        return;
      }

      MasterServer.Peer.Message_Send(MasterServer.Peer.Message_Create<Protocol.MasterServer_Session_ListRequest>(), MasterServer.EndPoint);
    }

    void MasterServer_NatProbeInfo_Ack(Protocol.MasterServer_NatProbeInfo query) {
      if (NatProbe_IsRunning() == false) {
        if (query.Result == null) {

        }
        else {
          NatProbe_Start(query.Result.Probe0, query.Result.Probe1, query.Result.Probe2);
        }
      }
    }

    void MasterServer_Session_Info(Protocol.MasterServer_Session_Info msg) {
      Session_Add(msg.Host);
    }

    void MasterServer_HostRegister_Ack(Protocol.MasterServer_HostRegister msg) {
      if (msg.Result == null) {
        MasterServer_HostRegister();
      }
    }

    void MasterServer_Introduce(Protocol.MasterServer_Introduce msg) {

    }

    void MasterServer_IntroduceInfo_Ack(Protocol.MasterServer_IntroduceInfo msg) {
      MasterServer.Peer.Message_Ack(msg);

      Protocol.NatPunch_PeerRegister register; 
      
      register = MasterServer.Peer.Message_Create<Protocol.NatPunch_PeerRegister>();
      register.Remote = msg.Remote;

      platformSocketPeer.Message_Send(register, msg.PunchServer);
    }
  }
}
