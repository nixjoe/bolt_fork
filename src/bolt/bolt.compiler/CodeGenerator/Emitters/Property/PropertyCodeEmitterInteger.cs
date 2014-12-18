﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class PropertyCodeEmitterInteger : PropertyCodeEmitter<PropertyDecoratorInteger> {
    public override string StorageField {
      get { return "Int0"; }
    }

    public override bool AllowSetter {
      get {
        var s = Decorator.Definition.StateAssetSettings;
        if (s != null) {
          return s.MecanimMode == MecanimMode.Disabled || s.MecanimDirection == MecanimDirection.UsingBoltProperties;
        }

        return true;
      }
    }

    public override void AddSettings(CodeExpression expr, CodeStatementCollection statements) {
      if (Decorator.PropertyType.CompressionEnabled) {
        statements.Call(expr, "Settings_Integer",
          "Bolt.PropertyIntCompressionSettings.Create({0}, {1})".Expr( Decorator.PropertyType.BitsRequired, -Decorator.PropertyType.MinValue)
        );
      }
      else {
        statements.Call(expr, "Settings_Integer", "Bolt.PropertyIntCompressionSettings.Create()".Expr());
      }

    }
  }
}
