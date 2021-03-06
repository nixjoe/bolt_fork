﻿using Bolt;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

partial class EntityChannel : BoltChannel {
  internal EntityLookup _outgoingLookup;
  internal EntityLookup _incommingLookup;

  internal Dictionary<Bolt.NetworkId, EntityProxy> _outgoingDict;
  internal Dictionary<Bolt.NetworkId, EntityProxy> _incommingDict;

  List<EntityProxy> _prioritized;

  public EntityChannel() {
    _outgoingDict = new Dictionary<NetworkId, EntityProxy>(2048, Bolt.NetworkId.EqualityComparer.Instance);
    _incommingDict = new Dictionary<NetworkId, EntityProxy>(2048, Bolt.NetworkId.EqualityComparer.Instance);

    _outgoingLookup = new EntityLookup(_outgoingDict);
    _incommingLookup = new EntityLookup(_incommingDict);

    _prioritized = new List<EntityProxy>();
  }

  public void ForceSync(Bolt.Entity en) {
    EntityProxy proxy;
    ForceSync(en, out proxy);
  }

  public void ForceSync(Bolt.Entity en, out EntityProxy proxy) {
    if (_outgoingDict.TryGetValue(en.NetworkId, out proxy)) {
      proxy.Flags |= Bolt.ProxyFlags.FORCE_SYNC;
      proxy.Flags &= ~Bolt.ProxyFlags.IDLE;
    }
  }

  public bool TryFindProxy(Bolt.Entity en, out EntityProxy proxy) {
    return _incommingDict.TryGetValue(en.NetworkId, out proxy) || _outgoingDict.TryGetValue(en.NetworkId, out proxy);
  }

  public void SetIdle(Bolt.Entity entity, bool idle) {
    EntityProxy proxy;

    if (_outgoingDict.TryGetValue(entity.NetworkId, out proxy)) {
      if (idle) {
        proxy.Flags |= Bolt.ProxyFlags.IDLE;
      }
      else {
        proxy.Flags &= ~Bolt.ProxyFlags.IDLE;
      }
    }
  }

  public void SetScope(Entity entity, bool inScope) {
    if (BoltCore._config.scopeMode == Bolt.ScopeMode.Automatic) {
      BoltLog.Error("SetScope has no effect when Scope Mode is set to Automatic");
      return;
    }

    if (ReferenceEquals(entity.Source, connection)) {
      return;
    }

    if (inScope) {
      if (_incommingDict.ContainsKey(entity.NetworkId)) {
        return;
      }

      EntityProxy proxy;

      if (_outgoingDict.TryGetValue(entity.NetworkId, out proxy)) {
        if (proxy.Flags & Bolt.ProxyFlags.DESTROY_REQUESTED) {
          if (proxy.Flags & Bolt.ProxyFlags.DESTROY_PENDING) {
            proxy.Flags |= Bolt.ProxyFlags.DESTROY_IGNORE;
          }
          else {
            proxy.Flags &= ~Bolt.ProxyFlags.DESTROY_IGNORE;
            proxy.Flags &= ~Bolt.ProxyFlags.DESTROY_REQUESTED;
          }
        }
      }
      else {
        CreateOnRemote(entity);
      }
    }
    else {
      if (_outgoingDict.ContainsKey(entity.NetworkId)) {
        DestroyOnRemote(entity);
      }
    }
  }

  public bool ExistsOnRemote(Entity entity) {
    if (entity == null) { return false; }
    if (_incommingDict.ContainsKey(entity.NetworkId)) { return true; }

    EntityProxy proxy;

    if (_outgoingDict.TryGetValue(entity.NetworkId, out proxy)) {
      return (proxy.Flags & ProxyFlags.CREATE_DONE) && !(proxy.Flags & ProxyFlags.DESTROY_REQUESTED);
    }

    return false;
  }

  public ExistsResult ExistsOnRemote(Entity entity, bool allowMaybe) {
    if (entity == null) { return ExistsResult.No; }
    if (_incommingDict.ContainsKey(entity.NetworkId)) { return ExistsResult.Yes; }

    EntityProxy proxy;

    if (_outgoingDict.TryGetValue(entity.NetworkId, out proxy)) {
      if ((proxy.Flags & ProxyFlags.CREATE_DONE) && !(proxy.Flags & ProxyFlags.DESTROY_REQUESTED)) {
        return ExistsResult.Yes;
      }

      if (allowMaybe) {
        return ExistsResult.Maybe;
      }
    }

    return ExistsResult.No;
  }

  public bool MightExistOnRemote(Bolt.Entity entity) {
    return _incommingDict.ContainsKey(entity.NetworkId) || _outgoingDict.ContainsKey(entity.NetworkId);
  }

  public void DestroyOnRemote(Bolt.Entity entity) {
    EntityProxy proxy;

    if (_outgoingDict.TryGetValue(entity.NetworkId, out proxy)) {
      // if we dont have any pending sends for this and we have not created it;
      if (proxy.Envelopes.Count == 0 && !(proxy.Flags & ProxyFlags.CREATE_DONE)) {
        DestroyOutgoingProxy(proxy);

      }
      else {
        proxy.Flags |= ProxyFlags.DESTROY_REQUESTED;
        proxy.Flags &= ~ProxyFlags.IDLE;
      }
    }
  }

  public void CreateOnRemote(Bolt.Entity entity) {
    EntityProxy proxy;
    CreateOnRemote(entity, out proxy);
  }

  public void CreateOnRemote(Bolt.Entity entity, out EntityProxy proxy) {
    if (_incommingDict.TryGetValue(entity.NetworkId, out proxy)) { return; }
    if (_outgoingDict.TryGetValue(entity.NetworkId, out proxy)) { return; }

    proxy = entity.CreateProxy();
    proxy.NetworkId = entity.NetworkId;
    proxy.Flags = ProxyFlags.CREATE_REQUESTED;
    proxy.Connection = connection;

    _outgoingDict.Add(proxy.NetworkId, proxy);

    BoltLog.Debug("Created {0} on {1}", entity, connection);
  }

  public float GetPriority(Entity entity) {
    EntityProxy proxy;

    if (_outgoingDict.TryGetValue(entity.NetworkId, out proxy)) {
      return proxy.Priority;
    }

    return float.NegativeInfinity;
  }

  public override void Pack(Packet packet) {
    int startPos = packet.UdpPacket.Position;

    // always clear before starting
    _prioritized.Clear();

    foreach (EntityProxy proxy in _outgoingDict.Values) {
      if (proxy.Flags & ProxyFlags.DESTROY_REQUESTED) {
        if (proxy.Flags & ProxyFlags.DESTROY_PENDING) {
          continue;
        }

        proxy.ClearAll();
        proxy.Priority = 1 << 17;
      }
      else {
        if (proxy.Entity.IsFrozen) {
          continue;
        }

        // check update rate of this entity
        if ((packet.Number % proxy.Entity.UpdateRate) != 0) {
          continue;
        }

        // meep
        if (proxy.Envelopes.Count >= 256) {
          BoltLog.Error("Envelopes for {0} to {1} full", proxy, connection);
          continue;
        }

        // if this connection is loading a map dont send any creates or state updates
        if (proxy.Entity.UnityObject._alwaysProxy == false) {
          if (connection.IsLoadingMap || BoltSceneLoader.IsLoading || (connection._canReceiveEntities == false)) {
            continue;
          }
        }

        if (proxy.Flags & ProxyFlags.FORCE_SYNC) {
          if (proxy.Entity.ReplicationFilter.AllowReplicationTo(connection)) {
            proxy.Priority = 1 << 20;
          }
          else {
            continue;
          }
        }
        else {
          if (proxy.Flags & ProxyFlags.CREATE_DONE) {
            if (proxy.IsZero) {
              continue;
            }

            // check for idle flag
            if (proxy.Flags & ProxyFlags.IDLE) {
              continue;
            }

            proxy.Priority = proxy.Entity.PriorityCalculator.CalculateStatePriority(connection, proxy.Skipped);
            proxy.Priority = Mathf.Clamp(proxy.Priority, 0, Mathf.Min(1 << 16, BoltCore._config.maxEntityPriority));
          }
          else {
            if ((proxy.Entity.IsFrozen == false) || proxy.Entity.AllowFirstReplicationWhenFrozen) {
              if (proxy.Entity.ReplicationFilter.AllowReplicationTo(connection)) {
                proxy.Priority = 1 << 18;
              }
              else {
                continue;
              }
            }
          }
        }
      }

      // if this is the controller give it the max priority
      if (proxy.Entity.IsController(connection)) {
        proxy.Priority = 1 << 19;
      }

      _prioritized.Add(proxy);
    }

    if (_prioritized.Count > 0) {
      try {
        _prioritized.Sort(EntityProxy.PriorityComparer.Instance);

        // write as many proxies into the packet as possible

        int failCount = 0;

        for (int i = 0; i < _prioritized.Count; ++i) {
          if ((_prioritized[i].Priority <= 0) || (failCount >= 2)) {
            _prioritized[i].Skipped += 1;
          }
          else {
            switch (PackUpdate(packet, _prioritized[i])) {
              case -1:
                failCount += 1;
                _prioritized[i].Skipped += 1;
                break;

              case 0:
                _prioritized[i].Skipped += 1;
                break;

              case +1:
                _prioritized[i].Skipped = 0;
                _prioritized[i].Priority = 0;
                break;
            }
          }
        }
      }
      finally {
        _prioritized.Clear();
      }
    }

    packet.UdpPacket.WriteStopMarker();
    packet.Stats.StateBits = packet.UdpPacket.Position - startPos;
  }

  public override void Read(Packet packet) {
    int startPtr = packet.UdpPacket.Position;

    // unpack all of our data
    while (packet.UdpPacket.CanRead()) {
      if (ReadUpdate(packet) == false) {
        break;
      }
    }

    packet.Stats.StateBits = packet.UdpPacket.Position - startPtr;
  }

  public override void Lost(Packet packet) {
    while (packet.EntityUpdates.Count > 0) {
      var env = packet.EntityUpdates.Dequeue();
      var pending = env.Proxy.Envelopes.Dequeue();

      //BoltLog.Error("LOST ENV {0}, IN TRANSIT: {1}", env.Proxy, env.Proxy.Envelopes.Count);
      //Assert.Same(env.Proxy, _outgoingProxiesByNetId[env.Proxy.NetId], string.Format("PROXY MISS-MATCH {0} <> {1}", env.Proxy, _outgoingProxiesByNetId[env.Proxy.NetId]));
      //Assert.Same(env, pending, string.Format("ENVELOPE MISS-MATCH {0} <> {1}", env.PacketNumber, pending.PacketNumber));

      // copy back all priorities
      ApplyPropertyPriorities(env);

      // push skipped count up one
      env.Proxy.Skipped += 1;

      // if this was a forced sync, set flag on proxy again
      if (env.Flags & ProxyFlags.FORCE_SYNC) {
        env.Proxy.Flags |= ProxyFlags.FORCE_SYNC;
      }

      // if we failed to destroy this clear destroying flag
      if (env.Flags & ProxyFlags.DESTROY_PENDING) {
        Assert.True(env.Proxy.Flags & ProxyFlags.DESTROY_PENDING);
        env.Proxy.Flags &= ~ProxyFlags.DESTROY_PENDING;
      }

      env.Dispose();
    }
  }

  public override void Delivered(Packet packet) {
    while (packet.EntityUpdates.Count > 0) {
      var env = packet.EntityUpdates.Dequeue();
      var pending = env.Proxy.Envelopes.Dequeue();

      //BoltLog.Info("DELIVERED ENV {0}, IN TRANSIT: {1}", env.Proxy, env.Proxy.Envelopes.Count);
      //Assert.Same(env.Proxy, _outgoingProxiesByNetId[env.Proxy.NetId], string.Format("PROXY MISS-MATCH {0} <> {1}", env.Proxy, _outgoingProxiesByNetId[env.Proxy.NetId]));
      //Assert.Same(env, pending, string.Format("ENVELOPE MISS-MATCH {0} <> {1}", env.PacketNumber, pending.PacketNumber));

      if (env.Flags & ProxyFlags.DESTROY_PENDING) {
        Assert.True(env.Proxy.Flags & ProxyFlags.DESTROY_PENDING);

        // delete proxy
        DestroyOutgoingProxy(env.Proxy);
      }
      else if (env.Flags & ProxyFlags.CREATE_REQUESTED) {
        // if this token has been sent, clear it
        if (ReferenceEquals(env.ControlTokenGained, env.Proxy.ControlTokenGained)) {
          env.Proxy.ControlTokenGained = null;
        }

        // clear out request / progress for create
        env.Proxy.Flags &= ~ProxyFlags.CREATE_REQUESTED;

        // set create done
        env.Proxy.Flags |= ProxyFlags.CREATE_DONE;
      }

      env.Dispose();
    }
  }

  public override void Disconnected() {
    foreach (EntityProxy proxy in _outgoingDict.Values.ToArray()) {
      if (proxy) {
        DestroyOutgoingProxy(proxy);
      }
    }

    foreach (EntityProxy proxy in _incommingDict.Values.ToArray()) {
      if (proxy) {
        DestroyIncommingProxy(proxy, null);
      }
    }
  }

  public int GetSkippedUpdates(Entity en) {
    EntityProxy proxy;

    if (_outgoingDict.TryGetValue(en.NetworkId, out proxy)) {
      return proxy.Skipped;
    }

    return -1;
  }

  void ApplyPropertyPriorities(EntityProxyEnvelope env) {
    for (int i = 0; i < env.Written.Count; ++i) {
      Priority p = env.Written[i];

      // set flag for sending this property again
      env.Proxy.Set(p.PropertyIndex);

      // increment priority
      env.Proxy.PropertyPriority[p.PropertyIndex].PropertyPriority += p.PropertyPriority;
    }
  }

  int PackUpdate(Packet packet, EntityProxy proxy) {
    int pos = packet.UdpPacket.Position;
    int packCount = 0;

    EntityProxyEnvelope env = proxy.CreateEnvelope();

    packet.UdpPacket.WriteBool(true);
    packet.UdpPacket.WriteNetworkId(proxy.NetworkId);

    if (packet.UdpPacket.WriteBool(proxy.Entity.IsController(connection))) {
      packet.UdpPacket.WriteToken(proxy.ControlTokenGained);
      proxy.ControlTokenLost = null;
    }
    else {
      packet.UdpPacket.WriteToken(proxy.ControlTokenLost);
      proxy.ControlTokenGained = null;
    }

    if (packet.UdpPacket.WriteBool(proxy.Flags & ProxyFlags.DESTROY_REQUESTED)) {
      packet.UdpPacket.WriteToken(proxy.Entity.DetachToken);
    }
    else {
      // data for first packet
      if (packet.UdpPacket.WriteBool(proxy.Flags & ProxyFlags.CREATE_REQUESTED)) {
        packet.UdpPacket.WriteToken(proxy.Entity.AttachToken);

        packet.UdpPacket.WritePrefabId(proxy.Entity.PrefabId);
        packet.UdpPacket.WriteTypeId(proxy.Entity.Serializer.TypeId);

        packet.UdpPacket.WriteVector3(proxy.Entity.UnityObject.transform.position);
        packet.UdpPacket.WriteQuaternion(proxy.Entity.UnityObject.transform.rotation);

        if (packet.UdpPacket.WriteBool(proxy.Entity.IsSceneObject)) {
          Assert.False(proxy.Entity.SceneId.IsNone, string.Format("'{0}' is marked a scene object but has no scene id ", proxy.Entity.UnityObject.gameObject));
          packet.UdpPacket.WriteUniqueId(proxy.Entity.SceneId);
        }
      }

      packCount = proxy.Entity.Serializer.Pack(connection, packet.UdpPacket, env);
    }

    if (packet.UdpPacket.Overflowing) {
      packet.UdpPacket.Position = pos;
      return -1;
    }
    if (packCount == -1) {
      packet.UdpPacket.Position = pos;
      return 0;
    }
    else {
      var isForce = proxy.Flags & ProxyFlags.FORCE_SYNC;
      var isCreate = proxy.Flags & ProxyFlags.CREATE_REQUESTED;
      var isDestroy = proxy.Flags & ProxyFlags.DESTROY_REQUESTED;

      // if we didn't pack anything and we are not creating or destroying this, just goto next
      if ((packCount == 0) && !isCreate && !isDestroy && !isForce) {
        packet.UdpPacket.Position = pos;
        return 0;
      }

      // set in progress flags
      if (isDestroy) { env.Flags = (proxy.Flags |= ProxyFlags.DESTROY_PENDING); }

      // clear force sync flag
      proxy.Flags &= ~ProxyFlags.FORCE_SYNC;

      // clear skipped count
      proxy.Skipped = 0;

      // set packet number
      env.PacketNumber = packet.Number;

      // put on packets list
      packet.EntityUpdates.Enqueue(env);

      // put on proxies pending queue
      // BoltLog.Info("adding envelope to {0}, count: {1}", proxy, proxy.Envelopes.Count + 1);
      proxy.Envelopes.Enqueue(env);

      // keep going!
      return 1;
    }
  }

  bool ReadUpdate(Packet packet) {
    if (packet.UdpPacket.ReadBool() == false) {
      return false;
    }

    // grab networkid
    NetworkId networkId = packet.UdpPacket.ReadNetworkId();
    bool isController = packet.UdpPacket.ReadBool();
    IProtocolToken controlToken = packet.UdpPacket.ReadToken();
    bool destroyRequested = packet.UdpPacket.ReadBool();

    // we're destroying this proxy
    if (destroyRequested) {
      EntityProxy proxy;
      IProtocolToken detachToken = packet.UdpPacket.ReadToken();

      if (_incommingDict.TryGetValue(networkId, out proxy)) {
        if (proxy.Entity.HasControl) {
          proxy.Entity.ReleaseControlInternal(controlToken);
        }

        DestroyIncommingProxy(proxy, detachToken);
      }
      else {
        BoltLog.Warn("Received destroy of {0} but no such proxy was found", networkId);
      }
    }
    else {
      IProtocolToken attachToken = null;

      bool isSceneObject = false;
      bool createRequested = packet.UdpPacket.ReadBool();

      UniqueId sceneId = UniqueId.None;
      PrefabId prefabId = new PrefabId();
      TypeId serializerId = new TypeId();
      Vector3 spawnPosition = new Vector3();
      Quaternion spawnRotation = new Quaternion();

      if (createRequested) {
        attachToken = packet.UdpPacket.ReadToken();

        prefabId = packet.UdpPacket.ReadPrefabId();
        serializerId = packet.UdpPacket.ReadTypeId();
        spawnPosition = packet.UdpPacket.ReadVector3();
        spawnRotation = packet.UdpPacket.ReadQuaternion();
        isSceneObject = packet.UdpPacket.ReadBool();

        if (isSceneObject) {
          sceneId = packet.UdpPacket.ReadUniqueId();
        }
      }

      Entity entity = null;
      EntityProxy proxy = null;

      if (createRequested && (_incommingDict.ContainsKey(networkId) == false)) {
        // create entity

        if (isSceneObject) {
          GameObject go = BoltCore.FindSceneObject(sceneId);

          if (!go) {
            BoltLog.Warn("Could not find scene object with {0}", sceneId);
            go = BoltCore.PrefabPool.Instantiate(prefabId, spawnPosition, spawnRotation);
          }

          entity = Entity.CreateFor(go, prefabId, serializerId, EntityFlags.SCENE_OBJECT);
        }
        else {
          GameObject go = BoltCore.PrefabPool.LoadPrefab(prefabId);

          // prefab checks (if applicable)
          if (go) {
            if (BoltCore.isServer && !go.GetComponent<BoltEntity>()._allowInstantiateOnClient) {
              throw new BoltException("Received entity of prefab {0} from client at {1}, but this entity is not allowed to be instantiated from clients", go.name, connection.RemoteEndPoint);
            }
          }

          entity = Entity.CreateFor(prefabId, serializerId, spawnPosition, spawnRotation);
        }

        entity.Source = connection;
        entity.SceneId = sceneId;
        entity.NetworkId = networkId;

        // handle case where we are given control (it needs to be true during the initialize, read and attached callbacks)
        if (isController) {
          entity.Flags |= EntityFlags.HAS_CONTROL;
        }

        // initialize entity
        entity.Initialize();

        // create proxy
        proxy = entity.CreateProxy();
        proxy.NetworkId = networkId;
        proxy.Connection = connection;

        // register proxy
        _incommingDict.Add(proxy.NetworkId, proxy);

        // read packet
        entity.Serializer.Read(connection, packet.UdpPacket, packet.Frame);

        // attach entity
        proxy.Entity.AttachToken = attachToken;
        proxy.Entity.Attach();

        // assign control properly
        if (isController) {
          proxy.Entity.Flags &= ~EntityFlags.HAS_CONTROL;
          proxy.Entity.TakeControlInternal(controlToken);
        }

        // log debug info
        BoltLog.Debug("Received {0} from {1}", entity, connection);

        // update last received frame
        proxy.Entity.LastFrameReceived = BoltNetwork.frame;
        proxy.Entity.Freeze(false);

        // notify user
        BoltInternal.GlobalEventListenerBase.EntityReceivedInvoke(proxy.Entity.UnityObject);
      }
      else {
        // find proxy
        proxy = _incommingDict[networkId];

        if (proxy == null) {
          throw new BoltException("Couldn't find entity for {0}", networkId);
        }

        // update control state yes/no
        if (proxy.Entity.HasControl ^ isController) {
          if (isController) {
            proxy.Entity.TakeControlInternal(controlToken);
          }
          else {
            proxy.Entity.ReleaseControlInternal(controlToken);
          }
        }

        // read update
        proxy.Entity.Serializer.Read(connection, packet.UdpPacket, packet.Frame);
        proxy.Entity.LastFrameReceived = BoltNetwork.frame;
        proxy.Entity.Freeze(false);
      }
    }

    return true;
  }

  void DestroyOutgoingProxy(EntityProxy proxy) {
    // remove outgoing proxy index
    _outgoingDict.Remove(proxy.NetworkId);

    // remove proxy from entity
    if (proxy.Entity && proxy.Entity.IsAttached) {
      proxy.Entity.Proxies.Remove(proxy);
      ;
    }

    if (proxy.Flags & ProxyFlags.DESTROY_IGNORE) {
      CreateOnRemote(proxy.Entity);
    }
  }

  void DestroyIncommingProxy(EntityProxy proxy, IProtocolToken token) {
    // remove incomming proxy
    _incommingDict.Remove(proxy.NetworkId);

    // destroy entity
    proxy.Entity.DetachToken = token;

    // destroy entity
    BoltCore.DestroyForce(proxy.Entity);
  }
}
