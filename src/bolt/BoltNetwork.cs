﻿using Bolt;
using System;
using System.Collections.Generic;
using BoltInternal;
using UdpKit;
using UnityEngine;
using System.Threading;
using System.Diagnostics;

namespace BoltInternal {
  public interface IDebugDrawer {
    void IsEditor(bool isEditor);
    void Indent(int adjust);
    void Label(string text);
    void LabelBold(string text);
    void LabelField(string text, object value);
    void Separator();
    void SelectGameObject(GameObject gameObject);
  }
}

[Documentation(Ignore = true)]
public static class BoltNetworkInternal {
  public static bool UsingUnityPro;

  public static Action EnvironmentSetup;
  public static Action EnvironmentReset;
  public static IDebugDrawer DebugDrawer;
  public static NatCommunicator NatCommunicator;

  public static Func<int, string> GetSceneName;
  public static Func<string, int> GetSceneIndex;
  public static Func<List<STuple<BoltGlobalBehaviourAttribute, Type>>> GetGlobalBehaviourTypes;

  public static void __Initialize(BoltNetworkModes mode, UdpEndPoint endpoint, BoltConfig config, UdpPlatform platform, string autoloadScene) {
    BoltCore.Initialize(mode, endpoint, config, platform, autoloadScene);
  }

  public static void __Shutdown() {
    BoltCore.Shutdown();
  }
}

/// <summary>
/// Holds global methods and properties for starting and
/// stopping bolt, instantiating prefabs and other utils
/// </summary>
/// <example>
/// *Example:* How to load a map on the server and instantiate a server controlled player.
/// 
/// ```csharp
/// void LoadMap(string mapName) {
///   BoltNetwork.LoadScene(mapName);
///   var serverPlayer = BoltNetwork.Instantiate(BoltPrefabs.Player) as GameObject;
///   Configure(serverPlayer);
/// }
/// ```
/// 
/// *Example:* How to connect to a server with known IP and port number.
/// 
/// ```csharp
/// void ConnectToServer(string host, int port) {
///   UdpEndPoint serverAddr = UdpEndPoint(UdpIPv4Address.Parse(host), (ushort)port);
///   BoltNetwork.Connect(server);
/// }
/// ```
/// 
/// *Example:* How to use the BoltNetwork ```frame``` property in an Update loop. Here we recalculate the player path 3 times per second.
/// 
/// ```csharp
/// void Update() {
///   if(BoltNetwork.frame % 20) == 0) {
///     playerMotor.Repath(targetPos);
///   }
/// }
/// ```
/// </example>
[Documentation]
public static class BoltNetwork {




  /// <summary>
  /// Async load Bolt Entity prefabs
  /// </summary>
  /// <example>
  /// *Example:* Preloading assets in the background before Bolt starts.
  /// 
  /// ```csharp
  /// void loadInBackground() {
  ///   BoltNetwork.PreLoadPrefabDatabase();
  ///   Resources.LoadAsync<myTerrain>("Terrain");
  /// }
  /// ```
  /// </example>
  public static void PreLoadPrefabDatabase() {
    Resources.LoadAsync<PrefabDatabase>("BoltPrefabDatabase");
  }

  //public static Action<bool> OnPoll;
  //public static Action<bool> OnSend;

  [Conditional("DEBUG")]
  internal static void VerifyIsRunning() {
    if (BoltNetwork.isRunning == false) {
      throw new InvalidOperationException("You can't do this if Bolt is not running");
    }
  }

  /// <summary>
  /// Whether the local simulation can receive entities instantiated from other connections
  /// </summary>
  /// <param name="canReceiveEntities">WTrue/False</param>
  /// <example>
  /// *Example:* Configuring the server to allow receiving entities and open a LAN game.
  /// 
  /// ```csharp
  /// void ConfigureServer() {
  ///   BoltNetwork.SetCanReceiveEntities(true);
  ///   BoltNetwork.EnableLanBroadcast();
  /// }
  /// ```
  /// </example>

  public static void SetCanReceiveEntities(bool canReceiveEntities) {
    BoltNetwork.VerifyIsRunning();
    BoltCore._canReceiveEntities = canReceiveEntities;
  }

  /// <summary>
  /// A list of all BoltEntities in the server simulation
  /// </summary>
  /// <example>
  /// *Example:* Destroying all player entities using a foreach loop over ```BoltNetwork.SceneObjects``` and finding the players with ```StateIs()```.
  /// 
  /// ```csharp
  /// void DestroyAllPlayers()
  /// {
  ///   foreach(var entity in BoltNetwork.SceneObjects)
  ///   {
  ///     if(entity.StateIs&ltIPlayerState&gt())
  ///     {
  ///       BoltNetwork.Destroy(entity.gameObject);
  ///     }
  ///   }
  /// }
  /// ```
  /// </example>
  public static IEnumerable<BoltEntity> SceneObjects {
    get { return BoltCore._sceneObjects.Values; }
  }

  public static void RegisterShutdownDoneCallback(Action action) {

  }

  /// <summary>
  /// The current local simulation frame number
  /// </summary>
  /// <example>
  /// *Example:* Using the BoltNetwork frame in a loop to recalculate the player path once every 20 frames.
  /// 
  /// ```csharp
  /// void FixedUpdate() {
  ///   if(BoltNetwork.frame % 20) == 0) {
  ///     playerMotor.Repath(targetPos);
  ///   }
  /// }
  /// ```
  /// </example>
  public static int frame {
    get { return BoltCore.frame; }
  }

  /// <summary>
  /// The max number of client connections to the server
  /// </summary>
  /// <example>
  /// *Example:* Using the max connections value to enforce server connection limits on an incoming client connection.
  /// 
  /// ```csharp
  /// public override void ConnectRequest(UdpEndPoint clientAddr)
  /// {
  ///   if(BoltNetwork.connections.Count == BoltNetwork.maxConnections)
  ///   {
  ///     BoltNetwork.Refuse(clientAddr);
  ///     return;
  ///   }
  /// }
  /// ```
  /// </example>
  public static int maxConnections {
    get {
      if (isRunning) {
        if (IsSinglePlayer) {
          return 0;
        }

        return isClient ? 1 : BoltCore._config.serverConnectionLimit;
      }

      return -1;
    }
  }

  public static UdpSocket UdpSocket {
    get {
      return BoltCore._udpSocket;
    }
  }

  /// <summary>
  /// The current server simulation frame number
  /// </summary>
  /// <example>
  /// *Example:* A post-game method to destroy all minions/npcs in the server simulation.
  /// 
  /// ```csharp
  /// void PostGameCleanup() {
  ///   foreach(var entity in BoltNetwork.entities) {
  ///     if(entity.isOwner && entity.StateIs&ltMinionState&gt) {
  ///       BoltNetwork.Destroy(entity);
  ///     }
  ///   }
  /// }
  /// ```
  /// </example>
  public static IEnumerable<BoltEntity> entities {
    get { return BoltCore.entities; }
  }

  /// <summary>
  /// On the server this returns the local frame, on a client this returns
  /// the currently estimated frame of all server objects we have received
  /// </summary>
  /// <example>
  /// *Example:* Predicting the next possible fire frame on the client using the estimated ```serverFrame```.
  /// 
  /// ```csharp
  /// void ClientFireWeapon(PlayerCommand cmd) {
  ///   if(weapon.nextFireFrame <= BoltNEtwork.serverFrame) {
  ///     state.Modify().FireTrigger();
  ///     weapon.nextFireFrame = BoltNetwork.serverFrame + weapon.refireRate;
  ///   }
  /// }
  /// ```
  /// </example>
  public static int serverFrame {
    get { return BoltCore.serverFrame; }
  }

  /// <summary>
  /// The current server simulation time
  /// </summary>
  /// <example>
  /// *Example:* Using the ```serverTime``` property to display a message when the max game time
  /// has expired.
  /// 
  /// ```csharp
  /// void Update() {
  ///   if((BoltNetwork.serverTime - gameStartTime >= MAX_GAME_TIME) {
  ///     Message.Show("Game Over", "Time Has Expired!");
  ///   }
  /// }
  /// ``` 
  /// </example>
  public static float serverTime {
    get { return BoltCore.serverTime; }
  }

  /// <summary>
  /// The local time, same as Time.time
  /// </summary>
  /// <example>
  /// *Example:* Using the ```time``` property to periodically play footstep sounds on the client.
  /// 
  /// ```csharp
  /// void Footsteps() {
  ///   if(BoltNetwork.time >= lastFootstep + footstepInterval) {
  ///     audio.PlayOneShot(footstepSound);
  ///     lastFootstep = BoltNetwork.time;
  ///   }
  /// }
  /// ```
  /// </example>
  public static float time {
    get { return BoltCore.time; }
  }

  /// <summary>
  /// The fixed frame delta, same as Time.fixedDeltaTime
  /// </summary>
  /// <example>
  /// *Example:* How to use ```frameDeltaTime``` to translate a player's per-second health regeneration rate into a per-frame
  /// value.
  /// 
  /// ```csharp
  /// protected override void SimulateOwner() {
  ///   float hpRegen = BoltNetwork.frameDeltaTime * state.HealthRegen;
  ///   state.Modify().HP = Mathf.Clamp(state.HP + hpRegen, 0, 100);
  /// }
  /// ```
  /// </example>
  public static float frameDeltaTime {
    get { return BoltCore.frameDeltaTime; }
  }

  /// <summary>
  /// The time the last fixed update begain, same as Time.fixedTime
  /// </summary>
  [System.Obsolete("This property will be removed in a future update")]
  public static float frameBeginTime {
    get { return BoltCore.frameBeginTime; }
  }

  /// <summary>
  /// Normalized value of how much time have passed since the last FixedUpdate
  /// </summary>
  [System.Obsolete("This property will be removed in a future update")]
  public static float frameAlpha {
    get { return BoltCore.frameAlpha; }
  }

  /// <summary>
  /// All the connections connected to this host
  /// </summary>
  /// <example>
  /// *Example:* Terminating all connections.
  /// 
  /// ```csharp
  /// void DisconnectAll() {
  ///   foreach(var connection in BoltNetwork.connections) {
  ///     connection.Disconnect();
  ///   }
  /// }
  /// ```
  /// </example>
  public static IEnumerable<BoltConnection> connections {
    get { return BoltCore.connections; }
  }

  /// <summary>
  /// All clients connected to this host
  /// </summary>
  /// <example>
  /// *Example:* Disconnecting all current clients.
  /// 
  /// ```csharp
  /// void DisconnectAllClients() {
  ///   foreach(var client in BoltNetwork.clients) {
  ///     client.Disconnect();
  ///   }
  /// }
  /// ```
  /// </example>
  public static IEnumerable<BoltConnection> clients {
    get { return BoltCore.clients; }
  }

  /// <summary>
  /// The server connection
  /// </summary>
  /// <example>
  /// *Example:* Displaying the current server IP address and port to the client.
  /// 
  /// ```csharp
  /// void ShowServerEndpoint() {
  ///   UdpEndPoint serverEndPoint = BoltNetwork.server.remoteEndPoint;
  ///   Message.Show("Current Server", string.Format("({0}:{1}", serverEndPoint.Address, serverEndPoint.Port);
  /// }
  /// ```
  /// </example>
  public static BoltConnection server {
    get { return BoltCore.server; }
  }

  /// <summary>
  /// How many FixedUpdate frames per second bolt is configured to run
  /// </summary>
  /// <example>
  /// *Example:* Calculating the number of frames between footsteps from a time interval.
  /// 
  /// ```csharp
  /// int FootstepFrameInterval(float stepTimeInterval) {
  ///   return Mathf.Round(stepTimeInterval / BoltNetwork.framesPerSecond);
  /// }
  /// ```
  /// </example>
  public static int framesPerSecond {
    get { return BoltCore.framesPerSecond; }
  }

  /// <summary>
  /// Returns true if this host is a server
  /// </summary>
  /// <example>
  /// *Example:* Using the ```isServer``` property to implement server specific logic such as spawning NPCs after a new map has been generated. 
  /// 
  /// ```csharp
  /// public override void SceneLoadLocalDone(string map) {
  ///   GenerateMap();
  ///   
  ///   if(BoltNetwork.isServer) {
  ///     SpawnNPCs();
  ///   }
  /// }
  /// ```
  /// </example>
  public static bool isServer {
    get { return BoltCore.isServer; }
  }

  [System.Obsolete("This property will be removed in a future update")]
  public static bool isServerOrNotRunning {
    get { return isServer || (isClient == false); }
  }

  /// <summary>
  /// Returns true if this instance is a server or a client with at least one valid connection.
  /// </summary>
  /// <example>
  /// *Example:* Using the ```isConnected``` property to do an automatic reconnect loop.
  /// 
  /// ```csharp
  /// void Update() {
  ///   if(!BoltNetwork.connected && BoltNetwork.time - lastReconnectTime > 30f) {
  ///     BoltNetwork.Connect(serverAddr);
  ///     lastReconnectTime = BoltNetwork.time;
  ///   }
  /// }
  /// ```
  /// </example>
  public static bool isConnected {
    get { return isServer || (isClient && BoltCore._connections.count > 0); }
  }

  /// <summary>
  /// Creating a new binary streaming channel
  /// </summary>
  /// <param name="name">Channel name</param>
  /// <param name="mode">Reliability mode</param>
  /// <param name="priority">Bandwidth priority</param>
  /// <returns>Channel name struct</returns>
  /// <example>
  /// *Example:* Creating an unreliable stream channel for voice and a reliable stream channel for sending custom player icons.
  /// 
  /// ```csharp
  /// public static UdpKit.UdpChannelName Voice;
  /// public static UdpKit.UdpChannelName CustomPlayerIcon;
  /// 
  /// public override void BoltStartBegin() {
  ///   Voice = BoltNetwork.CreateStreamChannel("Voice", UdpKit.UdpChannelMode.Unreliable, 1});
  ///   CustomPlayerIcon = BoltNetwork.CreateStreamChannel("CustomPlayerIcon", UdpKit.UdpChannelMode.Reliable, 4});
  /// }
  /// ```
  /// </example>
  public static UdpChannelName CreateStreamChannel(string name, UdpChannelMode mode, int priority) {
    BoltNetwork.VerifyIsRunning();
    return BoltCore.CreateStreamChannel(name, mode, priority);
  }

  public static void UpdateSceneObjectsLookup() {
    BoltNetwork.VerifyIsRunning();
    BoltCore.UpdateSceneObjectsLookup();
  }

  //public static UdpStreamData CreateStreamData(byte[] data) {
  //  return BoltCore.CreateStreamData(data);
  //}

  /// <summary>
  /// Returns true if this host is a client
  /// </summary>
  /// <example>
  /// *Example:* Implementing a client-side score sheet display to show the scores at the end of a game.
  /// 
  /// ```csharp
  /// void GameOver(string winTeam) {
  ///   if(BoltNetwork.isClient) {
  ///     DisplayScoreSheet("Game Over", winTeam + " Team Wins!";
  ///   }
  /// }
  /// ```
  /// </example>
  public static bool isClient {
    get { return BoltCore.isClient; }
  }


  /// <summary>
  /// If bolt is running
  /// </summary>
  /// <example>
  /// *Example:* How to use the ```isRunning``` property to detect a downtime and restart the server.
  /// 
  /// ```csharp
  /// void Update() {
  ///   if(!BoltNetwork.isRunning && BoltNetwork.time > lastRestart + 30f) {
  ///     RestartServer();
  ///     lastRestart = BoltNetwork.time;
  ///   }
  /// }
  /// ```
  /// </example>
  public static bool isRunning {
    get { return isServer || isClient || (BoltCore._mode == BoltNetworkModes.Shutdown); }
  }

  /// <summary>
  /// Returns true if Bolt was compiled in debug mode
  /// </summary>
  /// <example>
  /// *Example:* Showing an FPS and ping counter when in debug mode.
  /// 
  /// ```csharp
  /// public override void BoltStartDone() {
  ///   if(BoltNetwork.isDebugMode) {
  ///     PingView.instance.Show();
  ///     FPSCounter.instance.Show();
  ///   }   
  /// }
  /// ```
  /// </example>
  public static bool isDebugMode {
    get { return BoltCore.isDebugMode; }
  }

  /// <summary>
  /// The scoping mode active
  /// </summary>
  public static ScopeMode scopeMode {
    get {
      BoltNetwork.VerifyIsRunning();
      return BoltCore._config.scopeMode;
    }
  }

  /// <summary>
  /// The global object that all global behaviours will be attached to
  /// </summary>
  /// <example>
  /// *Example:* The ```globalObject``` can be used as a root for attaching new scripts such as GlobalEventListener callbacks.
  /// 
  /// ```csharp
  /// protected override void SceneLoadLocalDone(string map) {
  ///   if(map.Equals("GameScene")) {
  ///     BoltNetwork.globalObject.AddComponent&ltClientGameCallbacks&gt();
  ///   }
  /// }
  /// ```
  /// </example>
  public static GameObject globalObject {
    get {
      BoltNetwork.VerifyIsRunning();
      return BoltCore.globalObject;
    }
  }

  /// <summary>
  /// Find an entity based on unique id
  /// </summary>
  /// <param name="id">The id to look up</param>
  /// <returns>The entity if one was found, otherwise null</returns>
  /// <example>
  /// *Example:* Locating an entity within the scene using an id provided by the input command.
  /// 
  /// ```csharp
  /// public override void ExecuteCommand(Bolt.Command cmd, bool resetState) {
  ///   AttackCommand atkCmd = (AttackCommand)cmd;
  ///   vNetworkId targetId = atkCmd.Input.targetId;
  ///   
  ///   BoltEntity target = BoltNetwork.FindEntity(targetId);
  ///   activeWeapon.Fire(entity, target);
  /// }
  /// ```
  /// </example>
  public static BoltEntity FindEntity(NetworkId id) {
    BoltNetwork.VerifyIsRunning();

    if (id.Packed == 0) {
      return null;
    }


    foreach (var itval in BoltCore._entities) {
      if (itval.IsAttached && itval.UnityObject && itval.NetworkId.Packed == id.Packed) {
        return itval.UnityObject;
      }
    }

    BoltLog.Warn("Could not find entity with {0}", id);
    return null;
  }

  /// <summary>
  /// Registers a type as a potential protocol token
  /// </summary>
  /// <typeparam name="T">The type to register</typeparam>
  /// <example>
  /// *Example* Registering two token types on startup.
  /// 
  /// ```csharp
  /// public override void BoltStartDone() {
  ///   BoltNetwork.RegisterTokenClass&ltUserToken&gt();
  ///   BoltNetwork.RegisterTokenClass&ltServerMessage&gt();
  /// }
  /// ```
  /// </example>
  public static void RegisterTokenClass<T>() where T : class, IProtocolToken, new() {
    BoltNetwork.VerifyIsRunning();
    Factory.RegisterTokenClass(typeof(T));
  }


  /// <summary>
  /// Enables UPnP support on this instance
  /// </summary>
  public static void EnableUPnP() {
    BoltNetwork.VerifyIsRunning();
    UPnP.Enable();
  }

  /// <summary>
  /// Disable UPnP
  /// </summary>
  public static void DisableUPnP() {
    BoltNetwork.VerifyIsRunning();
    UPnP.Disable(true);
  }

  /// <summary>
  /// Opens a port to UPnP
  /// </summary>
  /// <param name="port">The port number</param>
  public static void OpenPortUPnP(int port) {
    BoltNetwork.VerifyIsRunning();
    UPnP.OpenPort(port);
  }

  /// <summary>
  /// Closes a port to UPnP
  /// </summary>
  /// <param name="port">The port number</param>
  public static void ClosePortUPnP(int port) {
    BoltNetwork.VerifyIsRunning();
    UPnP.ClosePort(port);
  }

  /// <summary>
  /// A list of available devices that provide UPnP support
  /// </summary>
  public static IEnumerable<INatDevice> NatDevicesUPnP {
    get { return UPnP.NatDevices; }
  }

  /// <summary>
  /// Sets bolt to use a filter to accept or reject certain events based on custom filtering
  /// </summary>
  /// <param name="filter">Your custom implementation of the IEventFilter interface</param>
  /// <example>
  /// *Example:* A custom event filter implementation which does nothing.
  /// 
  /// ```csharp
  /// public class NullEventFilter : IEventFilter {
  ///   public bool EventReceived(Event ev) {
  ///     return true;
  ///   }
  /// }
  /// ```
  /// 
  /// *Example:* Setting the ```NullEventFilter``` on startup.
  /// 
  /// ```csharp
  /// public override void BoltStartDone() {
  ///   SetEventFilter(new NullEventFilter());
  /// }
  /// ```
  /// 
  /// </example>
  public static void SetEventFilter(IEventFilter filter) {
    if (filter == null) {
      throw new ArgumentNullException("filter");
    }

    BoltCore.EventFilter = filter;
  }

  /// <summary>
  /// Sets a custom implementation for pooling prefabs
  /// </summary>
  /// <param name="pool">The custom pooling implementation</param>
  /// <example>
  /// *Example:* Setting bolt to use a custom prefab pooling implementation.
  /// 
  /// ```csharp
  /// public override void BoltStartDone() {
  ///   SetPrefabPool(new YourPrefabPool());
  /// }
  /// ```
  /// </example>
  public static void SetPrefabPool(IPrefabPool pool) {
    if (pool == null) {
      throw new ArgumentNullException("pool");
    }

    BoltCore.PrefabPool = pool;
  }

  /*
   * Instantiate
   * 
   * */

  /// <summary>
  /// Create a new entity in the simuation from a prefab
  /// </summary>
  /// <param name="prefab">The prefab to clone into the simulation</param>
  /// <returns>A reference to the new bolt entity</returns>
  /// <example>
  /// *Example:* How to instantiate and configure a player entity inside a ```Bolt.GlobalEventListener``` on the server using
  /// a public editor variable ```playerPrefab``` as the player prefab object.
  /// 
  /// ```csharp
  /// public GameObject playerPrefab;
  /// 
  /// public override void SceneLoadRemoteDone(BoltConnection connection) {
  ///   var player = BoltNetwork.Instantiate(playerPrefab);
  ///   player.transform.position = spawnPoint.transform.position;
  ///   
  ///   var initData = prototype.GetNewPlayer(GameLogic.PlayableClass.Mercenary);
  ///   Configure(player, initData);
  ///   
  ///   player.AssignControl(connection);
  /// }
  /// ```
  /// </example>
  public static BoltEntity Instantiate(GameObject prefab) {
    BoltNetwork.VerifyIsRunning();
    return Instantiate(prefab, null, Vector3.zero, Quaternion.identity);
  }

  /// <summary>
  /// Create a new entity in the simuation from a prefab
  /// </summary>
  /// <param name="prefab">The prefab to clone into the simulation</param>
  /// <param name="token">A data token of max size 512 bytes</param>
  /// <returns>A reference to the new bolt entity</returns>
  /// <example>
  /// *Example:* How to instantiate a player entity and allow to to configure itself with some initial data.
  /// 
  /// ```csharp
  /// public GameObject playerPrefab;
  /// 
  /// public override void SceneLoadRemoteDone(BoltConnection connection) {
  ///   var initData = prototype.GetNewPlayer(GameLogic.PlayableClass.Mercenary);
  ///   var player = BoltNetwork.Instantiate(playerPrefab, initData);
  ///   player.AssignControl(connection);
  /// }
  /// ```
  /// </example>
  public static BoltEntity Instantiate(GameObject prefab, IProtocolToken token) {
    BoltNetwork.VerifyIsRunning();
    return Instantiate(prefab, token, Vector3.zero, Quaternion.identity);
  }

  /// <summary>
  /// Create a new entity in the simuation from a prefab
  /// </summary>
  /// <param name="prefab">The prefab to clone into the simulation</param>
  /// <param name="position">A position vector</param>
  /// <param name="rotation">A rotation quaternion</param>
  /// <returns>A reference to the new bolt entity</returns>
  /// <example>
  /// *Example:* How to instantiate and configure a player entity with the position and rotation set to match
  /// that of the ```spawnPoint``` transform reference.
  /// 
  /// ```csharp
  /// public GameObject playerPrefab;
  /// 
  /// public override void SceneLoadRemoteDone(BoltConnection connection) {
  ///   var player = BoltNetwork.Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
  ///   
  ///   var initData = prototype.GetNewPlayer(GameLogic.PlayableClass.Mercenary);
  ///   Configure(player, initData);
  ///   
  ///   player.AssignControl(connection);
  /// }
  /// ```
  /// </example>
  public static BoltEntity Instantiate(GameObject prefab, Vector3 position, Quaternion rotation) {
    BoltNetwork.VerifyIsRunning();
    return Instantiate(prefab, null, position, rotation);
  }

  /// <summary>
  /// Create a new entity in the simuation from a prefab
  /// </summary>
  /// <param name="prefab">The prefab to clone into the simulation</param>
  /// <param name="token">A data token of max size 512 bytes</param>
  /// <param name="position">A position vector</param>
  /// <param name="rotation">A rotation quaternion</param>
  /// <returns>A reference to the new bolt entity</returns>
  /// <example>
  /// *Example:* How to instantiate and configure a player entity with both an initial data token and a given position / rotation.
  /// 
  /// ```csharp
  /// public GameObject playerPrefab;
  /// 
  /// public override void SceneLoadRemoteDone(BoltConnection connection) {
  ///   var initData = prototype.GetNewPlayer(GameLogic.PlayableClass.Mercenary);
  ///   var player = BoltNetwork.Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
  ///   
  ///   player.AssignControl(connection);
  /// }
  /// ```
  /// </example>
  public static BoltEntity Instantiate(GameObject prefab, IProtocolToken token, Vector3 position, Quaternion rotation) {
    BoltNetwork.VerifyIsRunning();
    BoltEntity be = prefab.GetComponent<BoltEntity>();

    if (!be) {
      BoltLog.Error("Prefab '{0}' does not have a Bolt Entity component attached", prefab.name);
      return null;
    }

    if (be.serializerGuid == UniqueId.None) {
      BoltLog.Error("Prefab '{0}' does not have a serializer assigned", prefab.name);
      return null;
    }

    return BoltCore.Instantiate(new PrefabId(be._prefabId), Factory.GetFactory(be.serializerGuid).TypeId, position, rotation, InstantiateFlags.ZERO, null, token);
  }

  /// <summary>
  /// Create a new entity in the simuation from a prefab
  /// </summary>
  /// <param name="prefabId">The prefab to clone into the simulation</param>
  /// <returns>A reference to the new bolt entity</returns>
  /// <example>
  /// *Example:* How to instantiate a player entity using the static ```BoltPrefabs``` class as a shortcut to the 
  /// player prefab object.
  /// 
  /// ```csharp
  /// public override void SceneLoadRemoteDone(BoltConnection connection) {
  ///   var initData = prototype.GetNewPlayer(GameLogic.PlayableClass.Mercenary);
  ///   var player = BoltNetwork.Instantiate(BoltPrefabs.Player, initData);
  ///   player.AssignControl(connection);
  /// }
  /// ```
  /// </example>
  public static BoltEntity Instantiate(PrefabId prefabId) {
    BoltNetwork.VerifyIsRunning();
    return Instantiate(prefabId, null, Vector3.zero, Quaternion.identity);
  }

  /// <summary>
  /// Create a new entity in the simuation from a prefab
  /// </summary>
  /// <param name="prefabId">The prefab to clone into the simulation</param>
  /// <param name="token">A data token of max size 512 bytes</param>
  /// <returns>A reference to the new bolt entity</returns>
  /// <example>
  /// *Example:* How to instantiate a player entity and allow to to configure itself with some initial data using
  /// static ```BoltPrefabs``` class as a shortcut to the player prefab object.
  /// 
  /// ```csharp
  /// public override void SceneLoadRemoteDone(BoltConnection connection) {
  ///   var initData = prototype.GetNewPlayer(GameLogic.PlayableClass.Mercenary);
  ///   var player = BoltNetwork.Instantiate(BoltPrefabs.Player, initData);
  ///   player.AssignControl(connection);
  /// }
  /// ```
  /// </example>
  public static BoltEntity Instantiate(PrefabId prefabId, IProtocolToken token) {
    BoltNetwork.VerifyIsRunning();
    return Instantiate(prefabId, token, Vector3.zero, Quaternion.identity);
  }

  /// <summary>
  /// Create a new entity in the simuation from a prefab
  /// </summary>
  /// <param name="prefabId">The prefab to clone into the simulation</param>
  /// <param name="position">A position vector</param>
  /// <param name="rotation">A rotation quaternion</param>
  /// <returns>A reference to the new bolt entity</returns>
  /// <example>
  /// *Example:* How to instantiate a player entity from the static ```BoltPrefabs``` class with a given position and rotation.
  /// 
  /// ```csharp
  /// public override void SceneLoadRemoteDone(BoltConnection connection) {
  ///   var player = BoltNetwork.Instantiate(BoltPrefabs.Player, spawnPoint.position, spawnPoint.rotation);
  ///   
  ///   var initData = prototype.GetNewPlayer(GameLogic.PlayableClass.Mercenary);
  ///   Configure(player, initData);
  ///   
  ///   player.AssignControl(connection);
  /// }
  /// ```
  /// </example>
  public static BoltEntity Instantiate(PrefabId prefabId, Vector3 position, Quaternion rotation) {
    BoltNetwork.VerifyIsRunning();
    return Instantiate(prefabId, null, position, rotation);
  }

  /// <summary>
  /// Create a new entity in the simuation from a prefab
  /// </summary>
  /// <param name="prefabId">The prefab to clone into the simulation</param>
  /// <param name="token">A data token of max size 512 bytes</param>
  /// <param name="position">A position vector</param>
  /// <param name="rotation">A rotation quaternion</param>
  /// <returns>A reference to the new bolt entity</returns>
  /// <example>
  /// *Example:* How to instantiate and configure a player entity inside a ```Bolt.GlobalEventListener``` on the server using
  /// initial data and the static ```BoltPrefabs``` class as a shortcut to the player prefab id.
  /// 
  /// ```csharp
  /// public override void SceneLoadRemoteDone(BoltConnection connection) {
  ///   var initData = prototype.GetNewPlayer(GameLogic.PlayableClass.Mercenary);
  ///   var player = BoltNetwork.Instantiate(BoltPrefabs.Player, spawnPoint.position, spawnPoint.rotation);
  ///   
  ///   player.AssignControl(connection);
  /// }
  /// ```
  /// </example>
  public static BoltEntity Instantiate(PrefabId prefabId, IProtocolToken token, Vector3 position, Quaternion rotation) {
    BoltNetwork.VerifyIsRunning();
    return Instantiate(BoltCore.PrefabPool.LoadPrefab(prefabId), token, position, rotation);
  }

  /*
   * Destroy
   * 
   * */

  /// <summary>
  /// Remove a gameObject from the bolt simulation.
  /// </summary>
  /// <param name="gameObject">The gameObject to remove</param>
  /// <example>
  /// *Example:* Destroying all player entities using a foreach loop over ```BoltNetwork.SceneObjects```.
  /// 
  /// ```csharp
  /// void DestroyAllPlayers() {
  ///   foreach(var entity in BoltNetwork.SceneObjects) {
  ///     if(entity.StateIs&ltIPlayerState&gt()) {
  ///       BoltNetwork.Destroy(entity.gameObject);
  ///     }
  ///   }
  /// }
  /// ```
  /// </example>
  public static void Destroy(GameObject gameObject) {
    Destroy(gameObject, null);
  }

  /// <summary>
  /// Remove a gameObject from the bolt simulation.
  /// </summary>
  /// <param name="gameObject">The gameObject to remove</param>
  /// <example>
  /// *Example:* Destroying all player entities using a foreach loop over ```BoltNetwork.SceneObjects``` and sending a death recap message as a protocol token.
  /// 
  /// ```csharp
  /// void DestroyAllPlayers() {
  ///   foreach(var entity in BoltNetwork.SceneObjects) {
  ///     if(entity.StateIs&ltIPlayerState&gt()) {
  ///       DeathRecap recap = new DeathRecap("Destroyed By Server");
  ///       BoltNetwork.Destroy(entity.gameObject, recap);
  ///     }
  ///   }
  /// }
  /// ```
  /// </example>
  public static void Destroy(GameObject gameObject, IProtocolToken token) {
    if (isRunning) {
      BoltEntity entity = gameObject.GetComponent<BoltEntity>();

      if (entity) {
        BoltCore.Destroy(entity, token);
      }
      else {
        if (token != null) {
          UnityEngine.Debug.LogWarning("Passing protocol token to destroy call for gameobject without bolt entity, token will be ignored");
        }

        UnityEngine.Object.Destroy(gameObject);
      }
    }
    else {
      if (token != null) {
        UnityEngine.Debug.LogWarning("Passing protocol token to destroy call for gameobject when bolt is not running, token will be ignored");
      }

      UnityEngine.Object.Destroy(gameObject);
    }
  }

  /*
   * Attach
   * 
   * */

  public static GameObject Attach(GameObject gameObject) {
    BoltNetwork.VerifyIsRunning();
    return Attach(gameObject, null);
  }

  public static GameObject Attach(GameObject gameObject, IProtocolToken token) {
    BoltNetwork.VerifyIsRunning();
    return BoltCore.Attach(gameObject, EntityFlags.ZERO, token);
  }

  /*
   * Detach
   * 
   * */

  public static void Detach(GameObject gameObject) {
    BoltNetwork.VerifyIsRunning();
    Detach(gameObject, null);
  }

  public static void Detach(GameObject gameObject, IProtocolToken token) {
    BoltNetwork.VerifyIsRunning();
    BoltCore.Detach(gameObject.GetComponent<BoltEntity>(), token);
  }

  public static Vector3 PositionAtFrame(BoltHitboxBody hitbox, int frame) {
    BoltNetwork.VerifyIsRunning();
    return BoltPhysics.PositionAtFrame(hitbox, frame);
  }

  /// <summary>
  /// Perform a raycast against Bolt hitboxes
  /// </summary>
  /// <param name="ray"><The ray to/param>
  /// <returns>The hitboxes that intersected the ray</returns>
  /// <example>
  /// *Example:* Using RaycastAll to detect a hit event and apply damage in a player weapon firing method.
  /// 
  /// ```csharp
  /// void FireWeaponOwner(PlayerCommand cmd, BoltEntity entity) {
  ///   if(entity.isOwner) {
  ///     using(var hits = BoltNetwork.RaycastAll(new Ray(entity.transform.position, cmd.Input.targetPos)) {
  ///       var hit = hits.GetHit(0);
  ///       var targetEntity = hit.body.GetComponent&ltBoltEntity&gt();
  ///       
  ///       if(targetEntity.StateIs&ltILivingEntity&gt()) {
  ///         targetEntity.GetState&ltILivingEntity&gt().Modify().HP -= activeWeapon.damage; 
  ///       }
  ///     }
  ///   }
  /// }
  /// ```
  /// </example> 
  public static BoltPhysicsHits RaycastAll(Ray ray) {
    BoltNetwork.VerifyIsRunning();
    return BoltPhysics.Raycast(ray);
  }

  /// <summary>
  /// Perform a raycast against Bolt hitboxes
  /// </summary>
  /// <param name="ray"><The ray to/param>
  /// <param name="frame">The frame to roll back to when performing this raycast</param>
  /// <returns>The hitboxes that intersected the ray</returns>
  /// <example>
  /// *Example:* Using RaycastAll to detect a hit event on a specific previous frame and then apply damage in a player weapon firing method.
  /// 
  /// ```csharp
  /// void FireWeaponOwner(PlayerCommand cmd, BoltEntity entity) {
  ///   if(entity.isOwner) {
  ///     using(var hits = BoltNetwork.RaycastAll(new Ray(entity.transform.position, cmd.Input.targetPos),
  ///       cmd.ServerFrame)) {
  ///       var hit = hits.GetHit(0);
  ///       var targetEntity = hit.body.GetComponent&ltBoltEntity&gt();
  ///       
  ///       if(targetEntity.StateIs&ltILivingEntity&gt()) {
  ///         targetEntity.GetState&ltILivingEntity&gt().Modify().HP -= activeWeapon.damage; 
  ///       }
  ///     }
  ///   }
  /// }
  /// ```
  /// </example>
  public static BoltPhysicsHits RaycastAll(Ray ray, int frame) {
    BoltNetwork.VerifyIsRunning();
    return BoltPhysics.Raycast(ray, frame);
  }

  /// <summary>
  /// Perform a sphere overlap against Bolt hiboxes
  /// </summary>
  /// <param name="origin">The origin of the sphere</param>
  /// <param name="radius">The radius of the sphere</param>
  /// <returns>The hitboxes that overlapped with the sphere</returns>
  /// <example>
  /// *Example:* Calculating the blast radius of a grenade.
  /// 
  /// ```csharp
  /// void GrenadeOwner(PlayerCommand cmd, BoltEntity entity, IThrownWeapon grenade) {
  ///   if(entity.isOwner) {
  ///     using(var hits = BoltNetwork.OverlapSphereAll(cmd.targetPos, grenade.explosionRadius)) {
  ///       for(int i = 0; i < hits.count; i++) {
  ///         var hit = hits.GetHit(i);
  ///         var targetEntity = hit.body.GetComponent&ltBoltEntity&gt();
  ///         
  ///         if(targetEntity != entity && targetEntity.StateIs&ltILivingEntity&gt()) {
  ///             targetEntity.GetState&ltILivingEntity&gt().Modify().HP -= grenade.damage;   
  ///         } 
  ///       }
  ///     }
  ///   }
  /// }
  /// ```
  /// </example>
  public static BoltPhysicsHits OverlapSphereAll(Vector3 origin, float radius) {
    BoltNetwork.VerifyIsRunning();
    return BoltPhysics.OverlapSphere(origin, radius);
  }

  /// <summary>
  /// Perform a sphere overlap against Bolt hiboxes
  /// </summary>
  /// <param name="origin">The origin of the sphere</param>
  /// <param name="radius">The radius of the sphere</param>
  /// <param name="frame">The frame to rollback to for calculation</param>
  /// <returns>The hitboxes that overlapped with the sphere</returns>
  /// <example>
  /// *Example:* Calculating the blast radius of a grenade.
  /// 
  /// ```csharp
  /// void GrenadeOwner(PlayerCommand cmd, BoltEntity entity, IThrownWeapon grenade) {
  ///   if(entity.isOwner) {
  ///     using(var hits = BoltNetwork.OverlapSphereAll(cmd.targetPos, grenade.explosionRadius, cmd.ServerFrame)) {
  ///       for(int i = 0; i < hits.count; i++) {
  ///         var hit = hits.GetHit(i);
  ///         var targetEntity = hit.body.GetComponent&ltBoltEntity&gt();
  ///         
  ///         if(targetEntity != entity && targetEntity.StateIs&ltILivingEntity&gt()) {
  ///             targetEntity.GetState&ltILivingEntity&gt().Modify().HP -= grenade.damage;   
  ///         } 
  ///       }
  ///     }
  ///   }
  /// }
  /// ```
  /// </example>
  public static BoltPhysicsHits OverlapSphereAll(Vector3 origin, float radius, int frame) {
    BoltNetwork.VerifyIsRunning();
    return BoltPhysics.OverlapSphere(origin, radius, frame);
  }

  /*
   * Accept
   * 
   * */

  /// <summary>
  /// Signal bolt to accept an incoming client connection request
  /// </summary>
  /// <param name="endpoint">The UDP address of incoming client connection</param>
  /// <example>
  /// *Example:* Accepting an incoming connection.
  /// 
  /// ```csharp
  /// public override void ConnectRequest(UdpEndPoint endpoint) {
  ///   BoltNetwork.Accept(endPoint);
  /// }
  /// ```
  /// </example>
  public static void Accept(UdpEndPoint endpoint) {
    BoltNetwork.VerifyIsRunning();
    BoltCore.AcceptConnection(endpoint, null, null);
  }

  /// <summary>
  /// Signal bolt to accept an incoming client connection request
  /// </summary>
  /// <param name="endpoint">The UDP address of incoming client connection</param>
  /// <param name="acceptToken">A data token from the server</param> 
  /// <example>
  /// *Example:* Accepting an incoming connection and passing a data token to tell the client the preferred reconnect timeout.
  /// 
  /// ```csharp
  /// public override void ConnectRequest(BoltConnection connection) {
  ///   ConnectionToken token = new ConnectionToken();
  ///   connectionToken.retryTimeout = 30f;
  ///   
  ///   BoltNetwork.Accept(connection.remoteEndPoint, token);
  /// }
  /// ```
  /// </example>
  public static void Accept(UdpEndPoint endpoint, IProtocolToken acceptToken) {
    BoltNetwork.VerifyIsRunning();
    BoltCore.AcceptConnection(endpoint, null, acceptToken);
  }

  //public static void Accept(UdpEndPoint endpoint, object userToken) {
  //  BoltNetwork.VerifyIsRunning();
  //  BoltCore.AcceptConnection(endpoint, userToken, null, null);
  //}

  //public static void Accept(UdpEndPoint endpoint, IProtocolToken acceptToken, IProtocolToken connectToken) {
  //  BoltNetwork.VerifyIsRunning();
  //  BoltCore.AcceptConnection(endpoint, null, acceptToken, connectToken);
  //}

  //public static void Accept(UdpEndPoint endpoint, object userToken, IProtocolToken acceptToken, IProtocolToken connectToken) {
  //  BoltNetwork.VerifyIsRunning();
  //  BoltCore.AcceptConnection(endpoint, userToken, acceptToken, connectToken);
  //}

  /*
   * Refuse
   * 
   * */

  /// <summary>
  /// Signal bolt to refuse an incoming connection request
  /// </summary>
  /// <param name="endpoint">The UDP address of incoming client connection</param>
  /// <example>
  /// *Example:* Refusing an incoming connection.
  /// 
  /// ```csharp
  /// public override void ConnectRequest(BoltConnection connection) {
  ///   BoltNetwork.Refuse(connection.remoteEndPoint);
  /// }
  /// ```
  /// </example>
  public static void Refuse(UdpEndPoint endpoint) {
    BoltNetwork.VerifyIsRunning();
    BoltCore.RefuseConnection(endpoint, null);
  }

  /// <summary>
  /// Signal bolt to refuse an incoming connection request
  /// </summary>
  /// <param name="endpoint">The UDP address of incoming client connection</param>
  /// <param name="token">A data token from the server</param> 
  /// <example>
  /// *Example:* Refusing an incoming connection and sending back an error message.
  /// 
  /// ```csharp
  /// public override void ConnectRequest(BoltConnection connection) {
  ///   ErrorMessage errorMessage = new ErrorMessage("Connection Refused", "Server Is Full");
  /// 
  ///   BoltNetwork.Refuse(connection.remoteEndPoint, errorMessage);
  /// }
  /// ```
  /// </example>
  public static void Refuse(UdpEndPoint endpoint, IProtocolToken token) {
    BoltNetwork.VerifyIsRunning();
    BoltCore.RefuseConnection(endpoint, token);
  }

  /// <summary>
  /// Manually add a global event listener
  /// </summary>
  /// <param name="mb">The monobehaviour to invoke events on</param>
  public static void AddGlobalEventListener(MonoBehaviour mb) {
    BoltNetwork.VerifyIsRunning();
    BoltCore._globalEventDispatcher.Add(mb);
  }

  /// <summary>
  /// Manually add a global event callback
  /// </summary>
  public static void AddGlobalEventCallback<T>(Action<T> callback) where T : Bolt.Event {
    BoltNetwork.VerifyIsRunning();
    BoltCore._globalEventDispatcher.Add<T>(callback);
  }

  /// <summary>
  /// Manually remove a global event listener
  /// </summary>
  /// <param name="mb">The monobehaviour to be removed</param>
  public static void RemoveGlobalEventListener(MonoBehaviour mb) {
    BoltNetwork.VerifyIsRunning();
    BoltCore._globalEventDispatcher.Remove(mb);
  }

  /// <summary>
  /// Manually remove a global event callback
  /// </summary>
  public static void RemoveGlobalEventCallback<T>(Action<T> callback) where T : Bolt.Event {
    BoltNetwork.VerifyIsRunning();
    BoltCore._globalEventDispatcher.Remove<T>(callback);
  }

  /// <summary>
  /// Load a scene based on name, only possible on the Server
  /// </summary>
  /// <param name="scene">The scene to load</param>
  /// <example>
  /// *Example:* A utility function to start a server and initialize the first map.
  /// ```csharp
  /// public void StartServer(UdpEndPoint addr, string map) {
  ///   BoltLauncher.StartServer(addr);
  ///   BoltNetwork.LoadScene(map);
  /// }
  /// ```
  /// </example>
  public static void LoadScene(string scene) {
    BoltNetwork.VerifyIsRunning();

    LoadScene(scene, null);
  }

  /// <summary>
  /// Load a scene based on name, only possible on the Server
  /// </summary>
  /// <param name="scene">The scene to load</param>
  /// <param name="token">A data token from the server</param>
  /// <example>
  /// *Example:* Passing a random tip to display to each client while the new map is loading.
  /// 
  /// ```csharp
  /// public void ChangeMap(string map) {
  ///   ServerMessage message = new ServerMessage("Loading Map ...", GameTips.GetNext());
  ///   BoltNetwork.LoadScene(map, message);
  /// }
  /// ```
  /// </example>
  public static void LoadScene(string scene, IProtocolToken token) {
    BoltNetwork.VerifyIsRunning();

    int sceneIndex = -1;

    try {
      sceneIndex = BoltNetworkInternal.GetSceneIndex(scene);
    }
    catch (Exception exn) {
      BoltLog.Error("Exceptiont thrown while trying to find index of scene '{0}'", scene);
      BoltLog.Exception(exn);
      return;
    }

    BoltCore.LoadScene(sceneIndex, token);
  }

  /// <summary>
  /// Connect to a server
  /// </summary>
  /// <param name="endpoint">Server end point to connect to</param>
  /// <example>
  /// *Example:* A method to connect to a known server address as a client.
  /// 
  /// ```csharp
  /// void Connect(string host, int port) {
  ///   UdpEndPoint serverAddr = new UdpEndPoint(UdpIPv4Address.Parse(host), (ushort)port);
  ///   
  ///   BoltNetwork.Connect(serverAddr);
  /// }
  /// ```
  /// </example>
  public static void Connect(UdpEndPoint endpoint) {
    BoltNetwork.VerifyIsRunning();
    BoltCore.Connect(endpoint, null);
  }

  public static void Connect(UdpSession session) {
    BoltNetwork.VerifyIsRunning();
    BoltCore.Connect(session, null);
  }

  public static void Connect(UdpSession session, IProtocolToken token) {
    BoltNetwork.VerifyIsRunning();
    BoltCore.Connect(session, token);
  }

  /// <summary>
  /// Connect to a server
  /// </summary>
  /// <param name="endpoint">Server end point to connect to</param>
  /// <example>
  /// *Example:* A method to connect to any server ip and port as a client.
  /// 
  /// ```csharp
  /// void Connect(string host, int port) {
  ///   UdpEndPoint serverAddr = new UdpEndPoint(UdpIPv4Address.Parse(host), (ushort)port);
  ///   
  ///   Credentials cred = new Credentials("DevUser01", "DevPassword");
  ///   
  ///   BoltNetwork.Connect(serverAddr, cred);
  /// }
  /// ```
  /// </example>
  public static void Connect(UdpEndPoint endpoint, IProtocolToken token) {
    BoltNetwork.VerifyIsRunning();
    BoltCore.Connect(endpoint, token);
  }

  /// <summary>
  /// Set session data for LAN Broadcast/Master Server listing
  /// </summary>
  /// <param name="serverName">Name of the server</param>
  /// <param name="userData">User definable data</param>
  /// <example>
  /// *Example:* Setting the host info to contain the max number of connections allowed by this server.
  /// 
  /// ```csharp
  /// void SetSessionData(string serverName, string description, int maxPlayers) {
  ///   SessionData sessionData = new SessionData(description, BoltNetwork.maxConnections);
  ///   
  ///   BoltNetwork.SetHostInfo(serverName, sessionData);
  /// }
  /// ```
  /// </example>
  public static void SetHostInfo(string serverName, IProtocolToken token) {
    BoltNetwork.VerifyIsRunning();
    BoltCore.SetHostInfo(serverName, false, token);
  }

  public static void SetDedicatedServerInfo(string serverName, IProtocolToken token) {
    BoltNetwork.VerifyIsRunning();
    BoltCore.SetHostInfo(serverName, true, token);
  }

  /// <summary>
  /// Disable LAN broadcasting
  /// </summary>
  /// <example>
  /// *Example:* Disabling LAN broadcasting at the end of a game.
  /// 
  /// ```csharp
  /// void GameOver() {
  ///   foreach(var client in BoltNetwork.clients) {
  ///     client.Disconnect();
  ///   }
  ///   BoltNetwork.DisableLanBroadcast();
  /// }
  /// ```
  /// </example>
  public static void DisableLanBroadcast() {
    BoltNetwork.VerifyIsRunning();
    BoltCore.DisableLanBroadcast();
  }

  /// <summary>
  /// Enable LAN broadcasting
  /// </summary>
  /// <example>
  /// *Example:* Enabling LAN broadcast after starting a new server.
  /// 
  /// ```csharp
  /// void StartServer(int port, string map) {
  ///   BoltLauncher.StartServer(new UdpEndPoint(UdpIPv4Address.Any, (ushort)serverPort));
  ///   BoltNetwork.LoadScene(map);
  ///   BoltNetwork.EnableLanBroadcast();
  /// }
  /// ```
  /// </example>
  public static void EnableLanBroadcast() {
    BoltNetwork.VerifyIsRunning();
    EnableLanBroadcast(60000);
  }

  public static bool IsSinglePlayer {
    get {
      BoltNetwork.VerifyIsRunning();
      return UdpSocket.PlatformSocket.Platform.IsNull;
    }
  }

  /// <summary>
  /// Enable LAN broadcasting
  /// </summary>
  /// <param name="port">The port to use for LAN broadcast</param>
  /// <example>
  /// *Example:* Enabling LAN broadcast after starting a new server using a specified port.
  /// 
  /// ```csharp
  /// void StartServer(int port, string map) {
  ///   BoltLauncher.StartServer(serverAddr);
  ///   BoltNetwork.EnableLanBroadcast(port);
  ///   BoltNetwork.LoadScene(map);
  /// }
  /// ```
  /// </example> 
  public static void EnableLanBroadcast(ushort port) {
    BoltNetwork.VerifyIsRunning();
    BoltCore._udpSocket.LanBroadcastEnable(UdpIPv4Address.Any, BoltCore._udpPlatform.GetBroadcastAddress(), port);
  }

  internal static Map<Guid, UdpSession> _sessionList = new Map<Guid, UdpSession>();

  public static Map<Guid, UdpSession> SessionList {
    get { return _sessionList; }
  }

  internal static void MasterServerRequestSessionList() {
    UdpSocket.MasterServerRequestSessionList();
  }

  internal static void MasterServerConnect(UdpEndPoint endpoint) {
    UdpSocket.MasterServerConnect(endpoint);
  }

  internal static void MasterServerDisconnect() {
    UdpSocket.MasterServerDisconnect();
  }
}
