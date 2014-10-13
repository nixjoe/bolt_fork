﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {

  struct CommandPropertyMetaData {
    public int ByteOffset;
    public String PropertyName;
  }

  struct EventPropertyMetaData {
    public int ByteOffset;
    public String PropertyName;
  }

  struct StatePropertyMetaData {
    public int ByteOffset;
    public int ByteLength;

    public int Priority;
    public int ObjectOffset;

    public String PropertyName;
    public String PropertyPath;
    public String[] CallbackPaths;
    public ArrayIndices CallbackIndices;
  }

  abstract class PropertySerializer {
    public readonly EventPropertyMetaData EventData;
    public readonly StatePropertyMetaData StateData;
    public readonly CommandPropertyMetaData CommandData;

    protected PropertySerializer(StatePropertyMetaData stateData) {
      StateData = stateData;
      StateData.Priority = UE.Mathf.Max(1, StateData.Priority);
    }

    protected PropertySerializer(EventPropertyMetaData eventData) {
      EventData = eventData;
    }

    protected PropertySerializer(CommandPropertyMetaData commandData) {
      CommandData = commandData;
    }

    public virtual object GetDebugValue(State state) { return null; }

    public virtual int StateBits(State state, State.Frame frame) { throw new NotSupportedException(); }
    public virtual bool StatePack(State state, State.Frame frame, BoltConnection connection, UdpStream stream) { throw new NotSupportedException(); }
    public virtual void StateRead(State state, State.Frame frame, BoltConnection connection, UdpStream stream) { throw new NotSupportedException(); }

    public virtual bool EventPack(Event data, BoltConnection connection, UdpStream stream) { throw new NotSupportedException(); }
    public virtual void EventRead(Event data, BoltConnection connection, UdpStream stream) { throw new NotSupportedException(); }

    public virtual void CommandPack(Command cmd, byte[] data, BoltConnection connection, UdpStream stream) { throw new NotSupportedException(); }
    public virtual void CommandRead(Command cmd, byte[] data, BoltConnection connection, UdpStream stream) { throw new NotSupportedException(); }
    public virtual void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {
      throw new NotSupportedException();
    }

    public virtual void OnInit(State state) { }
    public virtual void OnSimulateBefore(State state) { }
    public virtual void OnSimulateAfter(State state) { }
    public virtual void OnRender(State state, State.Frame frame) { }
    public virtual void OnParentChanged(State state, Entity newParent, Entity oldParent) { }
  }

  abstract class PropertySerializerSimple : PropertySerializer {
    public PropertySerializerSimple(StatePropertyMetaData info) : base(info) { }
    public PropertySerializerSimple(EventPropertyMetaData meta) : base(meta) { }
    public PropertySerializerSimple(CommandPropertyMetaData meta) : base(meta) { }

    public override bool EventPack(Event data, BoltConnection connection, UdpStream stream) {
      return Pack(data.Data, EventData.ByteOffset, connection, stream);
    }

    public override void EventRead(Event data, BoltConnection connection, UdpStream stream) {
      Read(data.Data, EventData.ByteOffset, connection, stream);
    }

    public override bool StatePack(State state, State.Frame frame, BoltConnection connection, UdpStream stream) {
      return Pack(frame.Data, StateData.ByteOffset, connection, stream);
    }

    public override void StateRead(State state, State.Frame frame, BoltConnection connection, UdpStream stream) {
      Read(frame.Data, StateData.ByteOffset, connection, stream);
    }

    public override void CommandPack(Command cmd, byte[] data, BoltConnection connection, UdpStream stream) {
      Pack(data, CommandData.ByteOffset, connection, stream);
    }

    public override void CommandRead(Command cmd, byte[] data, BoltConnection connection, UdpStream stream) {
      Read(data, CommandData.ByteOffset, connection, stream);
    }

    protected virtual bool Pack(byte[] data, int offset, BoltConnection connection, UdpStream stream) { throw new NotSupportedException(); }
    protected virtual void Read(byte[] data, int offset, BoltConnection connection, UdpStream stream) { throw new NotSupportedException(); }
  }

  abstract class PropertySerializerMecanim : PropertySerializerSimple {
    protected PropertyMecanimData MecanimData;

    protected bool ShouldPullDataFromMecanim(State state) {
      return MecanimData.Direction == MecanimDirection.UsingAnimatorMethods && (state.Entity.IsOwner || state.Entity.HasPredictedControl);
    }

    public PropertySerializerMecanim(StatePropertyMetaData meta) : base(meta) { }
    public PropertySerializerMecanim(EventPropertyMetaData meta) : base(meta) { }
    public PropertySerializerMecanim(CommandPropertyMetaData meta) : base(meta) { }

    public void SetPropertyData(PropertyMecanimData mecanimData) {
      MecanimData = mecanimData;
    }

    public override void OnSimulateAfter(State state) {
      if (MecanimData.Enabled && state.Animator) {
        if (MecanimData.Mode == MecanimMode.LayerWeight) {
          if (ShouldPullDataFromMecanim(state)) {
            PullMecanimLayer(state);
          }
          else {
            PushMecanimLayer(state);
          }
        }
        else {
          if (ShouldPullDataFromMecanim(state)) {
            PullMecanimValue(state);
          }
          else {
            PushMecanimValue(state);
          }
        }
      }
    }

    protected virtual void PullMecanimValue(State state) { }
    protected virtual void PushMecanimValue(State state) { }

    void PullMecanimLayer(State state) {
      state.Frames.first.Data.PackF32(StateData.ByteOffset, state.Animator.GetLayerWeight(MecanimData.Layer));
    }

    void PushMecanimLayer(State state) {
      state.Animator.SetLayerWeight(MecanimData.Layer, state.Frames.first.Data.ReadF32(StateData.ByteOffset));
    }
  }
}
