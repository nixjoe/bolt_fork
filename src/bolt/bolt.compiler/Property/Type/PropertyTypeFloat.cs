﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public class PropertyTypeFloat : PropertyType {
    [ProtoMember(1)]
    public FloatCompression Compression;

    public override bool HasSettings {
      get { return false; }
    }

    public override bool MecanimApplicable {
      get { return true; }
    }

    public override bool InterpolateAllowed {
      get { return true; }
    }

    public override bool CanSmoothCorrections {
      get { return true; }
    }

    public override PropertyDecorator CreateDecorator() {
      return new PropertyDecoratorFloat();
    }
  }
}