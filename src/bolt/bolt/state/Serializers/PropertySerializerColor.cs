﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  class PropertySerializerColor : PropertySerializerSimple {
    public override object GetDebugValue(State state) {
      var c = Blit.ReadColor(state.Frames.first.Data, Settings.ByteOffset);
      return string.Format("R:{0} G:{1} B:{2} A:{3}", c.r.ToString("F3"), c.g.ToString("F3"), c.b.ToString("F3"), c.a.ToString("F3"));
    }

    public override int StateBits(State state, State.Frame frame) {
      return 32 * 4;
    }

    protected override bool Pack(byte[] data, BoltConnection connection, UdpPacket stream) {
      stream.WriteColorRGBA(Blit.ReadColor(data, Settings.ByteOffset));
      return true;
    }

    protected override void Read(byte[] data, BoltConnection connection, UdpPacket stream) {
      Blit.PackColor(data, Settings.ByteOffset, stream.ReadColorRGBA());
    }
  }
}
