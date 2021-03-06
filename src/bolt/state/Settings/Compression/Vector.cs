﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  struct PropertyVectorCompressionSettings {
    public bool StrictComparison;
    public PropertyFloatCompressionSettings X;
    public PropertyFloatCompressionSettings Y;
    public PropertyFloatCompressionSettings Z;

    public int BitsRequired {
      get { return X.BitsRequired + Y.BitsRequired + Z.BitsRequired; }
    }

    public static PropertyVectorCompressionSettings Create(
      PropertyFloatCompressionSettings x,
      PropertyFloatCompressionSettings y,
      PropertyFloatCompressionSettings z) {
      return Create(x, y, z, false);
    }

    public static PropertyVectorCompressionSettings Create(
      PropertyFloatCompressionSettings x,
      PropertyFloatCompressionSettings y,
      PropertyFloatCompressionSettings z,
      bool strict) {

      return new PropertyVectorCompressionSettings {
        X = x,
        Y = y,
        Z = z,
        StrictComparison = strict
      };
    }

    public void Pack(UdpPacket stream, UE.Vector3 value) {
      X.Pack(stream, value.x);
      Y.Pack(stream, value.y);
      Z.Pack(stream, value.z);
    }

    public UE.Vector3 Read(UdpPacket stream) {
      UE.Vector3 v;

      v.x = X.Read(stream);
      v.y = Y.Read(stream);
      v.z = Z.Read(stream);

      return v;
    }

  }
}
