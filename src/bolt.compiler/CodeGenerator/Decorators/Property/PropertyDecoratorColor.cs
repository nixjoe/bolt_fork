﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyDecoratorColor : PropertyDecorator<PropertyTypeColor> {
    public override string ClrType {
      get { return "UE.Color"; }
    }

    public override PropertyCodeEmitter CreateEmitter() {
      return new PropertyCodeEmitterColor();
    }
  }
}
