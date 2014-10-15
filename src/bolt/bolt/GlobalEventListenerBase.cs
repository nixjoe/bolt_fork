﻿using UnityEngine;


namespace BoltInternal {
  /// <summary>
  /// Base class for all BoltCallbacks objects
  /// </summary>
  public abstract partial class GlobalEventListenerBase : MonoBehaviour, IBoltListNode {
    static readonly BoltDoubleList<GlobalEventListenerBase> callbacks = new BoltDoubleList<GlobalEventListenerBase>();

    object IBoltListNode.prev { get; set; }
    object IBoltListNode.next { get; set; }
    object IBoltListNode.list { get; set; }

    protected void OnEnable() {
      BoltCore._globalEventDispatcher.Add(this);
      callbacks.AddLast(this);
    }

    protected void OnDisable() {
      BoltCore._globalEventDispatcher.Remove(this);
      callbacks.Remove(this);
    }

    /// <summary>
    /// Override this method and return true if you want the event listener to keep being attached to Bolt even when bBolt shuts down and starts again.
    /// </summary>
    /// <returns>True/False</returns>
    public virtual bool PersistBetweenStartupAndShutdown() {
      return false;
    }
  }
}