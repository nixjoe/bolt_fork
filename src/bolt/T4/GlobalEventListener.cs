﻿ 

using UdpKit;
using UnityEngine;

namespace BoltInternal {
partial class GlobalEventListenerBase {

/// <summary>
  /// Callback triggered when the bolt simulation is shutting down.
  /// </summary>
  /// <example>
  /// *Example:* Logging a message in the bolt console when the server has shut down unexpectedly.
  /// 
  /// ```csharp
  /// public override void BoltShutdown() {
  ///   BoltConsole.Write("Warning: Server Shutting Down...");
  /// }
  /// ```
  /// </example>
 
public virtual void BoltShutdownBegin(Bolt.AddCallback registerDoneCallback) {  }

internal static void BoltShutdownBeginInvoke(Bolt.AddCallback registerDoneCallback) { 
	//BoltLog.Debug("Invoking callback BoltShutdownBegin");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.BoltShutdownBegin(registerDoneCallback);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}



 
public virtual void BoltStartBegin() {  }

internal static void BoltStartBeginInvoke() { 
	//BoltLog.Debug("Invoking callback BoltStartBegin");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.BoltStartBegin();
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}



 
public virtual void BoltStartDone() {  }

internal static void BoltStartDoneInvoke() { 
	//BoltLog.Debug("Invoking callback BoltStartDone");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.BoltStartDone();
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}



 
public virtual void BoltStartFailed() {  }

internal static void BoltStartFailedInvoke() { 
	//BoltLog.Debug("Invoking callback BoltStartFailed");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.BoltStartFailed();
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}


/// <summary>
  /// Callback triggered when binary stream data is received 
  /// </summary>
  /// <param name="connection">The sender connection</param>
  /// <param name="data">The binary stream data</param>
  /// <example>
  /// *Example:* Receiving a custom player icon.
  /// 
  /// ```csharp
  /// public override void StreamDataReceived(BoltConnection connnection, UdpStreamData data) { 
  ///   Texture2D icon = new Texture2D(4, 4);
  ///   icon.LoadImage(data.Data);
  ///   
  ///   PlayerData playerData = (PlayerData)connection.userToken;
  ///   playerData.SetIcon(icon);
  /// }
  /// ```
  /// </example>
 
public virtual void StreamDataReceived(BoltConnection connection, UdpStreamData data) {  }

internal static void StreamDataReceivedInvoke(BoltConnection connection, UdpStreamData data) { 
	//BoltLog.Debug("Invoking callback StreamDataReceived");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.StreamDataReceived(connection, data);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}


/// <summary>
  /// Callback when network router port mapping has been changed
  /// </summary>
  /// <param name="device">The current network routing device</param>
  /// <param name="portMapping">The new port mapping</param>
 
public virtual void PortMappingChanged(Bolt.INatDevice device, Bolt.IPortMapping portMapping) {  }

internal static void PortMappingChangedInvoke(Bolt.INatDevice device, Bolt.IPortMapping portMapping) { 
	//BoltLog.Debug("Invoking callback PortMappingChanged");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.PortMappingChanged(device, portMapping);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}


/// <summary>
  /// Callback triggered before the new local scene is loaded
  /// </summary>
  /// <param name="map">Name of scene being loaded</param>
  /// <example>
  /// *Example:* Showing a splash screen when clients are loading the game scene.
  /// 
  /// ```csharp
  /// public override void SceneLoadLocalBegin(string map) {
  ///   if(BoltNetwork.isClient && map.Equals("GameScene") {
  ///     SplashScreen.Show(SplashScreens.GameLoad);
  ///   }
  /// }
  /// ```
  /// </example>
 
public virtual void SceneLoadLocalBegin(string map) {  }

internal static void SceneLoadLocalBeginInvoke(string map) { 
	//BoltLog.Debug("Invoking callback SceneLoadLocalBegin");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.SceneLoadLocalBegin(map);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}



 
public virtual void SceneLoadLocalBegin(string scene, Bolt.IProtocolToken token) {  }

internal static void SceneLoadLocalBeginInvoke(string scene, Bolt.IProtocolToken token) { 
	//BoltLog.Debug("Invoking callback SceneLoadLocalBegin");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.SceneLoadLocalBegin(scene, token);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}


/// <summary>
  /// Callback triggered before the new local scene has been completely loaded
  /// </summary>
  /// <param name="map">Name of scene that has loaded</param>
  /// <example>
  /// *Example:* Hiding a splash screen that was shown during loading.
  /// 
  /// ```csharp
  /// public override void sceneLoadLocalDone(string map) {
  ///   if(BoltNetwork.isClient && map.Equals("GameScene") {
  ///     SplashScreen.Hide();
  ///   }
  /// }
  /// ```
  /// </example>
 
public virtual void SceneLoadLocalDone(string map) {  }

internal static void SceneLoadLocalDoneInvoke(string map) { 
	//BoltLog.Debug("Invoking callback SceneLoadLocalDone");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.SceneLoadLocalDone(map);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}



 
public virtual void SceneLoadLocalDone(string scene, Bolt.IProtocolToken token) {  }

internal static void SceneLoadLocalDoneInvoke(string scene, Bolt.IProtocolToken token) { 
	//BoltLog.Debug("Invoking callback SceneLoadLocalDone");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.SceneLoadLocalDone(scene, token);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}


/// <summary>
  /// Callback triggered when a remote connection has completely loaded the current scene
  /// </summary>
  /// <param name="connection">The remote connection</param>
  /// <example>
  /// *Example:* Instantiating and configuring a player entity on the server and then assigning control to the client.
  /// 
  /// ```csharp
  /// public override void SceneLoadRemoteDone(BoltConnection connection) {
  ///   var player = BoltNetwork.Instantiate(BoltPrefabs.Player);
  ///   player.transform.position = spawnPoint.transform.position;
  ///   
  ///   var initData = prototype.GetNewPlayer(GameLogic.PlayableClass.Mercenary);
  ///   Configure(player, initData);
  ///   
  ///   player.AssignControl(connection);
  /// }
  /// ```
  /// </example>
 
public virtual void SceneLoadRemoteDone(BoltConnection connection) {  }

internal static void SceneLoadRemoteDoneInvoke(BoltConnection connection) { 
	//BoltLog.Debug("Invoking callback SceneLoadRemoteDone");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.SceneLoadRemoteDone(connection);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}



 
public virtual void SceneLoadRemoteDone(BoltConnection connection, Bolt.IProtocolToken token) {  }

internal static void SceneLoadRemoteDoneInvoke(BoltConnection connection, Bolt.IProtocolToken token) { 
	//BoltLog.Debug("Invoking callback SceneLoadRemoteDone");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.SceneLoadRemoteDone(connection, token);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}


/// <summary>
  /// Callback triggered when a client has become connected to this instance
  /// </summary>
  /// <param name="connection">Endpoint of the connected client</param>
  /// <example>
  /// *Example:* Instantiating and configuring a player entity when a client connects to the server.
  /// 
  /// ```csharp
  /// public override void Connected(BoltConnection connection) {
  ///   var player = BoltNetwork.Instantiate(BoltPrefabs.Player);
  ///   player.transform.position = spawnPoint.transform.position;
  ///   
  ///   var initData = prototype.GetNewPlayer(GameLogic.PlayableClass.Mercenary);
  ///   Configure(player, initData);
  ///   
  ///   player.AssignControl(connection);
  /// }
  /// ```
  /// </example>
 
public virtual void Connected(BoltConnection connection) {  }

internal static void ConnectedInvoke(BoltConnection connection) { 
	//BoltLog.Debug("Invoking callback Connected");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.Connected(connection);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}


/// <summary>
  /// Callback triggered when a connection to remote server has failed
  /// </summary>
  /// <param name="endpoint">The remote address</param>
  /// <example>
  /// *Example:* Logging an error message when the remote connection has failed.
  /// 
  /// ```csharp
  /// public override void ConnectFailed(UdpEndPoint endpoint) {
  ///   BoltConsole.Write(string.Format("Connection To ({0}:{1}) has failed", endpoint.Address.ToString(), endpoint.ToString()));
  /// }
  /// ```
  /// </example>
 
public virtual void ConnectFailed(UdpEndPoint endpoint, Bolt.IProtocolToken token) {  }

internal static void ConnectFailedInvoke(UdpEndPoint endpoint, Bolt.IProtocolToken token) { 
	//BoltLog.Debug("Invoking callback ConnectFailed");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.ConnectFailed(endpoint, token);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}


/// <summary>
  /// Callback triggered when this instance receives an incoming client connection
  /// </summary>
  /// <param name="endpoint">The incoming client endpoint</param>
  /// <param name="token">A data token sent from the incoming client</param>
  /// <example>
  /// *Example:* Accepting an incoming connection with user credentials in the data token.
  /// 
  /// ```csharp
  /// public override void ConnectRequest(UdpEndPoint endpoint, Bolt.IProtocolToken token) {
  ///   UserCredentials creds = (UserCredentials)token);
  ///   if(Authenticate(creds.username, creds.password)) {
  ///     BoltNetwork.Accept(connection.remoteEndPoint);
  ///   }
  /// }
  /// ```
  /// </example>
 
public virtual void ConnectRequest(UdpEndPoint endpoint, Bolt.IProtocolToken token) {  }

internal static void ConnectRequestInvoke(UdpEndPoint endpoint, Bolt.IProtocolToken token) { 
	//BoltLog.Debug("Invoking callback ConnectRequest");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.ConnectRequest(endpoint, token);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}


/// <summary>
  /// Callback triggered when the connection to a remote server has been refused.
  /// </summary>
  /// <param name="endpoint">The remote server endpoint</param>
  /// <param name="token">Data token sent by refusing server</param>
  /// <example>
  /// *Example:* Logging an error message when the remote connection has been refused using an error message token from the server.
  /// 
  /// ```csharp
  /// public override void ConnectRefused(UdpEndPoint endpoint, Bolt.IProtocolToken token) {
  ///   ServerMessage.message = (ServerMessage)token;
  ///   BoltConsole.Write(string.Format("Connection To ({0}:{1}) has been refused. Reason was {3}", 
  ///     endpoint.Address.ToString(), endpoint.ToString(), serverMessage.errorDescription));
  /// }
  /// ```
  /// </example>
 
public virtual void ConnectRefused(UdpEndPoint  endpoint, Bolt.IProtocolToken  token) {  }

internal static void ConnectRefusedInvoke(UdpEndPoint  endpoint, Bolt.IProtocolToken  token) { 
	//BoltLog.Debug("Invoking callback ConnectRefused");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.ConnectRefused(endpoint, token);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}


/// <summary>
  /// Callback triggered when trying to connect to a remote endpoint
  /// </summary>
  /// <param name="endpoint">The remote server address</param>
  /// <example>
  /// *Example:* Logging a message when initializing a connection to server.
  /// 
  /// ```csharp
  /// public override void ConnectAttempt((UdpEndPoint endpoint) {
  ///   BoltConsole.Write(string.Format("To Remote Server At ({0}:{1})", endpoint.Address, endpoint.Port);
  /// }
  /// ```
  /// </example>
 
public virtual void ConnectAttempt(UdpEndPoint endpoint, Bolt.IProtocolToken token) {  }

internal static void ConnectAttemptInvoke(UdpEndPoint endpoint, Bolt.IProtocolToken token) { 
	//BoltLog.Debug("Invoking callback ConnectAttempt");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.ConnectAttempt(endpoint, token);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}


/// <summary>
  /// Callback triggered when disconnected from remote server
  /// </summary>
  /// <param name="connection">The remote server endpoint</param>
  /// <example>
  /// *Example:* Logging a disconnect message and returning to the main menu scene.
  /// 
  /// ```csharp
  /// public override void Disconnected(BoltConnection connection) {
  ///   BoltConsole.Write("Returning to main menu...");
  ///   Application.LoadLevel("MainMenu");
  /// }
  /// ```
  /// </example>
 
public virtual void Disconnected(BoltConnection connection) {  }

internal static void DisconnectedInvoke(BoltConnection connection) { 
	//BoltLog.Debug("Invoking callback Disconnected");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.Disconnected(connection);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}


/// <summary>
  /// Callback triggered when this instance of bolt loses control of a bolt entity
  /// </summary>
  /// <param name="entity">The controlled entity</param>
  /// <example>
  /// *Example:* Setting up game components to no longer control an entity.
  /// 
  /// ```csharp
  /// public override void ControlOfEntityLost(BoltEntity entity) {
  ///   GameInput.instance.RemoveControlledEntity(entity);
  ///   MiniMap.instance.RemoveControlledEntity(entity);
  /// }
  /// ```
  /// </example>
 
public virtual void ControlOfEntityLost(BoltEntity entity) {  }

internal static void ControlOfEntityLostInvoke(BoltEntity entity) { 
	//BoltLog.Debug("Invoking callback ControlOfEntityLost");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.ControlOfEntityLost(entity);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}


/// <summary>
  /// Callback triggered when this instance of bolt receieves control of a bolt entity
  /// </summary>
  /// <param name="entity">The controlled entity</param>
  /// <example>
  /// *Example:* Setting up the game minimap and other components to use a specific entity as the player's controlled entity.
  /// 
  /// ```csharp
  /// public override void ControlOfEntityGained(BoltEntity entity) {
  ///   GameInput.instance.SetControlledEntity(entity);
  ///   MiniMap.instance.SetControlledEntity(entity);
  /// }
  /// ```
  /// </example>
 
public virtual void ControlOfEntityGained(BoltEntity entity) {  }

internal static void ControlOfEntityGainedInvoke(BoltEntity entity) { 
	//BoltLog.Debug("Invoking callback ControlOfEntityGained");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.ControlOfEntityGained(entity);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}


/// <summary>
  /// Callback triggered when a new entity is attached to the bolt simulation
  /// </summary>
  /// <param name="entity">The attached entity</param>
  /// <example>
  /// *Example:* Setting up the game minimap to show a newly attached entity.
  /// 
  /// ```csharp
  /// public override void EntityAttached(BoltEntity entity) {
  ///   MiniMap.instance.SetKnownEntity(entity);
  /// }
  /// ```
  /// </example>
 
public virtual void EntityAttached(BoltEntity entity) {  }

internal static void EntityAttachedInvoke(BoltEntity entity) { 
	//BoltLog.Debug("Invoking callback EntityAttached");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.EntityAttached(entity);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}


/// <summary>
  /// Callback triggered when a new entity is detached from the bolt simulation
  /// </summary>
  /// <param name="entity">The detached entity</param>
  /// <example>
  /// *Example:* Removing the newly detached entity from the game minimap.
  /// 
  /// ```csharp
  /// public override void EntityDetached(BoltEntity entity) {
  ///   MiniMap.instance.RemoveKnownEntity(entity);
  /// }
  /// ```
  /// </example>
 
public virtual void EntityDetached(BoltEntity entity) {  }

internal static void EntityDetachedInvoke(BoltEntity entity) { 
	//BoltLog.Debug("Invoking callback EntityDetached");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.EntityDetached(entity);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}


/// <summary>
  /// Callback triggered when a bolt entity is recieved from the network
  /// </summary>
  /// <param name="entity">The recieved bolt entity</param>
  /// <example>
  /// *Example:* Loggging connections from remote players in the client bolt console
  /// 
  /// ```csharp
  /// public override void EntityReceived(BoltEntity entity) {
  ///   string name = entity.GetState&ltPlayerState&gt().Name; 
  ///   BoltConsole.Write(string.Format("{0} Has Connected", name));
  /// }
  /// ```
  /// </example>
 
public virtual void EntityReceived(BoltEntity entity) {  }

internal static void EntityReceivedInvoke(BoltEntity entity) { 
	//BoltLog.Debug("Invoking callback EntityReceived");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.EntityReceived(entity);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}



 
public virtual void EntityFrozen(BoltEntity entity) {  }

internal static void EntityFrozenInvoke(BoltEntity entity) { 
	//BoltLog.Debug("Invoking callback EntityFrozen");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.EntityFrozen(entity);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}



 
public virtual void EntityThawed(BoltEntity entity) {  }

internal static void EntityThawedInvoke(BoltEntity entity) { 
	//BoltLog.Debug("Invoking callback EntityThawed");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.EntityThawed(entity);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}



 
public virtual void ZeusConnectFailed(UdpEndPoint endpoint) {  }

internal static void ZeusConnectFailedInvoke(UdpEndPoint endpoint) { 
	//BoltLog.Debug("Invoking callback ZeusConnectFailed");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.ZeusConnectFailed(endpoint);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}



 
public virtual void ZeusConnected(UdpEndPoint endpoint) {  }

internal static void ZeusConnectedInvoke(UdpEndPoint endpoint) { 
	//BoltLog.Debug("Invoking callback ZeusConnected");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.ZeusConnected(endpoint);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}



 
public virtual void ZeusDisconnected(UdpEndPoint endpoint) {  }

internal static void ZeusDisconnectedInvoke(UdpEndPoint endpoint) { 
	//BoltLog.Debug("Invoking callback ZeusDisconnected");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.ZeusDisconnected(endpoint);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}



 
public virtual void ZeusNatProbeResult(UdpKit.NatFeatures features) {  }

internal static void ZeusNatProbeResultInvoke(UdpKit.NatFeatures features) { 
	//BoltLog.Debug("Invoking callback ZeusNatProbeResult");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.ZeusNatProbeResult(features);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}



 
public virtual void SessionListUpdated(Map<System.Guid, UdpSession> sessionList) {  }

internal static void SessionListUpdatedInvoke(Map<System.Guid, UdpSession> sessionList) { 
	//BoltLog.Debug("Invoking callback SessionListUpdated");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.SessionListUpdated(sessionList);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}



 
public virtual void SessionConnectFailed(UdpSession session, Bolt.IProtocolToken token) {  }

internal static void SessionConnectFailedInvoke(UdpSession session, Bolt.IProtocolToken token) { 
	//BoltLog.Debug("Invoking callback SessionConnectFailed");
	foreach (GlobalEventListenerBase cb in callbacks) {
		try {
			cb.SessionConnectFailed(session, token);
		} catch(System.Exception exn) {
			BoltLog.Exception(exn);
		}
	}
}

}
}

