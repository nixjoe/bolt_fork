﻿using Bolt;
using System;
using System.Collections;
using BoltInternal;
using UE = UnityEngine;

[Documentation]
public class BoltEntitySettingsModifier : IDisposable {
  BoltEntity _entity;

  internal BoltEntitySettingsModifier(BoltEntity entity) {
    _entity = entity;
  }

  public PrefabId prefabId {
    get { return _entity.prefabId; }
    set { _entity.VerifyNotAttached(); _entity._prefabId = value.Value; }
  }

  public UniqueId sceneId {
    get { return _entity.sceneGuid; }
    set { _entity.VerifyNotAttached(); _entity.sceneGuid = value; }
  }

  public UniqueId serializerId {
    get { return _entity.serializerGuid; }
    set { _entity.VerifyNotAttached(); _entity.serializerGuid = value; }
  }

  public int updateRate {
    get { return _entity._updateRate; }
    set { _entity.VerifyNotAttached(); _entity._updateRate = value; }
  }

  public bool clientPredicted {
    get { return _entity._clientPredicted; }
    set { _entity.VerifyNotAttached(); _entity._clientPredicted = value; }
  }

  public bool allowInstantiateOnClient {
    get { return _entity._allowInstantiateOnClient; }
    set { _entity.VerifyNotAttached(); _entity._allowInstantiateOnClient = value; }
  }

  public bool persistThroughSceneLoads {
    get { return _entity._persistThroughSceneLoads; }
    set { _entity.VerifyNotAttached(); _entity._persistThroughSceneLoads = value; }
  }

  void IDisposable.Dispose() {

  }
}

[Documentation]
[UE.ExecuteInEditMode]
[BoltExecutionOrder(-2500)]
public class BoltEntity : UE.MonoBehaviour, IBoltListNode {
  internal Entity _entity;

  [UE.SerializeField]
  internal string _sceneGuid;

  [UE.SerializeField]
  internal string _serializerGuid;

  [UE.SerializeField]
  internal int _prefabId = -1;

  [UE.SerializeField]
  internal int _updateRate = 1;

  [UE.SerializeField]
  internal bool _clientPredicted = true;

  [UE.SerializeField]
  internal bool _allowInstantiateOnClient = true;

  [UE.SerializeField]
  internal bool _persistThroughSceneLoads = false;

  internal Entity Entity {
    get {
      if (_entity == null) {
        throw new BoltException("You can't access any Bolt specific methods or properties on an entity which is detached");
      }

      return _entity;
    }
  }

  internal UniqueId sceneGuid {
    get { return UniqueId.Parse(_sceneGuid); }
    set { _sceneGuid = value.guid.ToString(); }
  }

  internal UniqueId serializerGuid {
    get { return UniqueId.Parse(_serializerGuid); }
    set { _serializerGuid = value.guid.ToString(); }
  }

  object IBoltListNode.prev { get; set; }
  object IBoltListNode.next { get; set; }
  object IBoltListNode.list { get; set; }

  public PrefabId prefabId {
    get { return new PrefabId(_prefabId); }
  }

  /// <summary>
  /// If this entity was created on another computer, contains the connection we received this entity from, otherwise null
  /// </summary>
  public BoltConnection source {
    get { return Entity.Source; }
  }

  public NetworkId networkId {
    get { return Entity.NetworkId; }
  }

  /// <summary>
  /// If this entity is controlled by a remote connection it contains that connection, otherwise null
  /// </summary>
  public BoltConnection controller {
    get { return Entity.Controller; }
  }

  /// <summary>
  /// If this entity is attached to Bolt or not
  /// </summary>
  public bool isAttached {
    get { return (_entity != null) && _entity.IsAttached; }
  }

  /// <summary>
  /// This is a scene object placed in the scene in the Unity editor
  /// </summary>
  public bool isSceneObject {
    get { return Entity.IsSceneObject; }
  }

  /// <summary>
  /// Did the local computer create this entity or not?
  /// </summary>
  public bool isOwner {
    get { return Entity.IsOwner; }
  }

  /// <summary>
  /// Do we have control of this entity?
  /// </summary>
  public bool hasControl {
    get { return Entity.HasControl; }
  }

  /// <summary>
  /// Do we have control of this entity and are we using client side prediction
  /// </summary>
  public bool hasControlWithPrediction {
    get { return Entity.HasPredictedControl; }
  }

  /// <summary>
  /// Should this entity persist between scene loads
  /// </summary>
  public bool persistsOnSceneLoad {
    get { return Entity.PersistsOnSceneLoad; }
  }

  /// <summary>
  /// Creates an object which lets you modify the internal settings of an entity before it is attached to Bolt.
  /// </summary>
  /// <returns>The object used to modify entity settings</returns>
  public BoltEntitySettingsModifier ModifySettings() {
    VerifyNotAttached();
    return new BoltEntitySettingsModifier(this);
  }

  /// <summary>
  /// Sets the scope of all currently active connections for this entity. Only usable if Scope Mode has been set to Manual.
  /// </summary>
  /// <param name="inScope">If this entity should be in scope or not</param>
  public void SetScopeAll(bool inScope) {
    Entity.SetScopeAll(inScope);
  }

  /// <summary>
  /// Sets the scope for the connection passed in for this entity. Only usable if Scope Mode has been set to Manual.
  /// </summary>
  /// <param name="inScope">If this entity should be in scope or not</param>
  public void SetScope(BoltConnection connection, bool inScope) {
    Entity.SetScope(connection, inScope);
  }

  /// <summary>
  /// Sets the parent of this entity
  /// </summary>
  /// <param name="parent">The parent of this entity</param>
  public void SetParent(BoltEntity parent) {
    if (parent) {
      Entity.SetParent(parent.Entity);
    }
    else {
      Entity.SetParent(null);
    }
  }

  /// <summary>
  /// Takes local control of this entity
  /// </summary>
  public void TakeControl() {
    Entity.TakeControl(null);
  }

  public void TakeControl(IProtocolToken token) {
    Entity.TakeControl(token);
  }

  /// <summary>
  /// Releases local control of this entity
  /// </summary>
  public void ReleaseControl() {
    Entity.ReleaseControl(null);
  }

  public void ReleaseControl(IProtocolToken token) {
    Entity.ReleaseControl(token);
  }

  /// <summary>
  /// Assigns control of this entity to a connection
  /// </summary>
  /// <param name="connection">The connection to assign control to</param>
  public void AssignControl(BoltConnection connection) {
    Entity.AssignControl(connection, null);
  }

  public void AssignControl(BoltConnection connection, IProtocolToken token) {
    Entity.AssignControl(connection, token);
  }

  /// <summary>
  /// Revokes control of this entity from a connection
  /// </summary>
  public void RevokeControl() {
    Entity.RevokeControl(null);
  }

  /// <summary>
  /// Revokes control of this entity from a connection
  /// </summary>
  public void RevokeControl(IProtocolToken token) {
    Entity.RevokeControl(token);
  }

  /// <summary>
  /// Checks if this entity is being controlled by the connection
  /// </summary>
  /// <param name="connection">The connection to check</param>
  public bool IsController(BoltConnection connection) {
    return ReferenceEquals(Entity.Controller, connection);
  }

  /// <summary>
  /// Queue a Data not his entity for execution. This is called on a client which is 
  /// controlling a proxied entity the Data will also be sent to the server.
  /// </summary>
  /// <param name="data">The Data to queue</param>
  public bool QueueInput(INetworkCommandData data) {
    return Entity.QueueInput(((NetworkCommand_Data)data).RootCommand);
  }

  /// <summary>
  /// Set this entity as idle on the supplied connection, this means that the connection 
  /// will not receive update state for this entity as long as it's idle.
  /// </summary>
  /// <param name="connection">The connection to idle the entity on</param>
  /// <param name="idle">If this should be idle or not</param>
  public void Idle(BoltConnection connection, bool idle) {
    Entity.SetIdle(connection, idle);
  }

  /// <summary>
  /// Add an event listener to this entity.
  /// </summary>
  /// <param name="behaviour">The behaviour to invoke event callbacks on</param>
  public void AddEventListener(UE.MonoBehaviour behaviour) {
    Entity.AddEventListener(behaviour);
  }

  /// <summary>
  /// Remove an event listern from this entity
  /// </summary>
  /// <param name="behaviour">The behaviour to remove</param>
  public void RemoveEventListener(UE.MonoBehaviour behaviour) {
    Entity.RemoveEventListener(behaviour);
  }

  /// <summary>
  /// Get the state if this entity
  /// </summary>
  /// <typeparam name="TState">The type of state to get</typeparam>
  /// <returns>The state</returns>
  public TState GetState<TState>() {
    if (Entity.Serializer is TState) {
      return (TState)(object)Entity.Serializer;
    }

    BoltLog.Error("You are trying to access the state of {0} as '{1}'", Entity, typeof(TState));
    return default(TState);
  }

  public bool TryFindState<TState>(out TState state) {
    if (Entity.Serializer is TState) {
      state = (TState)(object)Entity.Serializer;
      return true;
    }

    state = default(TState);
    return false;
  }

  /// <summary>
  /// Checks which type of state this entity has
  /// </summary>
  /// <typeparam name="TState">The type of state to check for</typeparam>
  /// <returns>True if this entity has a state of type TState otherwise false</returns>
  public bool StateIs<TState>() {
    return Entity.Serializer is TState;
  }

  public bool StateIs(Type t) {
    return t.IsAssignableFrom(Entity.Serializer.GetType());
  }

  public override string ToString() {
    if (isAttached) {
      return Entity.ToString();
    }
    else {
      return string.Format("[DetachedEntity {2} SceneId={0} SerializerId={1} {3}]", sceneGuid, serializerGuid, prefabId, gameObject.name);
    }
  }

  public void DestroyDelayed(float time) {
    StartCoroutine(DestroyDelayedInternal(time));
  }

  internal void VerifyNotAttached() {
    if (isAttached) {
      throw new InvalidOperationException("You can't modify a BoltEntity behaviour which is attached to Bolt");
    }
  }

  IEnumerator DestroyDelayedInternal(float time) {
    yield return new UE.WaitForSeconds(time);

    if (isAttached) {
      BoltNetwork.Destroy(gameObject);
    }
  }

  void Awake() {
    // only in the editor
    if ((UE.Application.isEditor == true) && (UE.Application.isPlaying == false)) {
      // check if we don't have a valid scene guid
      if (sceneGuid == UniqueId.None) {
        // set a new one
        sceneGuid = UniqueId.New();

        // tell editor to save us
        BoltCoreInternal.ChangedEditorEntities.Add(this);
      }
    }
  }

  void OnDisable() {
    if (UE.Application.isPlaying) {
      OnDestroy();
    }
  }

  void OnDestroy() {
    if (_entity && _entity.IsAttached && UE.Application.isPlaying) {
      if (_entity.IsOwner) {
        BoltLog.Warn("{0} is being destroyed/disabled without being detached, forcing detach", Entity);
      }
      else {
        BoltLog.Error("{0} is being destroyed/disabled without being detached by the owner, this will cause this peer to disconnect the next time it receives an update for this entity");
      }

      // force detach
      _entity.Detach();
      _entity = null;
    }
  }

  void OnDrawGizmos() {
    UE.Gizmos.DrawIcon(transform.position, "BoltEntity Gizmo", true);
  }

  void Update() {
    if (isAttached && UE.Application.isPlaying) {
      Entity.Render();
    }
  }

  public static implicit operator UE.GameObject(BoltEntity entity) {
    return entity == null ? null : entity.gameObject;
  }
}