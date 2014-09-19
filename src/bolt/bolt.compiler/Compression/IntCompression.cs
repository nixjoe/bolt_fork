﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bolt.compiler {
  [ProtoContract]
  public class IntCompression {
    [ProtoMember(1)]
    public int MinValue;

    [ProtoMember(2)]
    public int MaxValue;
  }
}
