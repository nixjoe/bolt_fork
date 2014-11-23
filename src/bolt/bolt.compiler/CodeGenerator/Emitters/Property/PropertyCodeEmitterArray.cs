﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public class PropertyCodeEmitterArray : PropertyCodeEmitter<PropertyDecoratorArray> {
    public override void EmitObjectMembers(CodeTypeDeclaration type) {
      type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, get => {
        get.Expr("return ({0}) (Objects[this.OffsetObjects + {1}])", Decorator.ClrType, Decorator.OffsetObjects);
      });
    }

    public override void EmitStateMembers(StateDecorator decorator, CodeTypeDeclaration type) {
      EmitForwardStateMember(decorator, type, false);
    }

    public override void EmitStateInterfaceMembers(CodeTypeDeclaration type) {
      EmitSimpleIntefaceMember(type, true, false);
    }

    public override void EmitObjectSetup(DomBlock block, Offsets offsets) {
      EmitInitObject(Decorator.ClrType, block, offsets, Decorator.PropertyType.ElementCount.Literal());

      if (Decorator.PropertyType.ElementType is PropertyTypeStruct) {
        var tmp1 = block.TempVar();
        var element = (PropertyDecoratorStruct)Decorator.ElementDecorator;

        offsets.OffsetStorage = "offsets.OffsetStorage + {0} + ({1} * {2})".Expr(Decorator.OffsetStorage, tmp1, element.RequiredStorage);
        offsets.OffsetObjects = "offsets.OffsetObjects + {0} + ({1} * {2})".Expr(Decorator.OffsetObjects + 1, tmp1, element.RequiredObjects);
        offsets.OffsetProperties = "offsets.OffsetProperties + {0} + ({1} * {2})".Expr(Decorator.OffsetProperties, tmp1, element.RequiredProperties);

        block.Stmts.For(tmp1, tmp1 + " < " + Decorator.PropertyType.ElementCount, body => {
          PropertyCodeEmitter.Create(element).EmitObjectSetup(new DomBlock(body, tmp1 + "_"), offsets);
        });
      }
    }

    public override void EmitMetaSetup(DomBlock block, Offsets offsets) {
      var tmp = block.TempVar();
      var element = Decorator.ElementDecorator;

      offsets.OffsetStorage = "{0} + ({1} * {2}) /*required-storage:{2}*/".Expr(Decorator.OffsetStorage, tmp, element.RequiredStorage);
      offsets.OffsetProperties = "{0} + ({1} * {2}) /*required-properties:{2}*/".Expr(Decorator.OffsetProperties, tmp, element.RequiredProperties);

      if (element.RequiredObjects == 0) {
        offsets.OffsetObjects = "0 /*required-objects:{0}*/".Expr(element.RequiredObjects);
      }
      else {
        offsets.OffsetObjects = "{0} + ({1} * {2}) /*required-objects:{2}*/".Expr(Decorator.OffsetObjects + 1, tmp, element.RequiredObjects);
      }

      block.Stmts.For(tmp, tmp + " < " + Decorator.PropertyType.ElementCount, body => {
        PropertyCodeEmitter.Create(element).EmitMetaSetup(new DomBlock(body, tmp + "_"), offsets);
      });
    }
  }
}
