﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  [ProtoInclude(100, typeof(PropertyTypeFloat))]
  [ProtoInclude(200, typeof(PropertyTypeStruct))]
  [ProtoInclude(300, typeof(PropertyTypeArray))]
  [ProtoInclude(400, typeof(PropertyTypeVector))]
  [ProtoInclude(500, typeof(PropertyTypeString))]
  [ProtoInclude(600, typeof(PropertyTypeTrigger))]
  [ProtoInclude(700, typeof(PropertyTypeTransform))]
  public abstract class PropertyType {
    [ProtoIgnore]
    public Project Context;

    [ProtoIgnore]
    public virtual bool IsValue { get { return true; } }

    [ProtoIgnore]
    public virtual bool CallbackAllowed { get { return true; } }

    [ProtoIgnore]
    public virtual bool InterpolateAllowed { get { return false; } }

    public virtual PropertyDecorator CreateDecorator() {
      throw new NotImplementedException();
    }
  }
}
