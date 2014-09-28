﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public abstract class PropertyCodeEmitter {
    public PropertyDecorator Decorator;

    public CodeGenerator Generator {
      get { return Decorator.Generator; }
    }

    public virtual CodeExpression CreatePropertyArrayInitializerExpression(StateProperty p) {
      return @"new Bolt.PropertySerializerFloat(new Bolt.PropertyMetaData {{ ByteOffset = {0}, ByteLength = {1}, ObjectOffset = {2}, Priority = {3}, CallbackPath = ""{4}"", CallbackIndices = {5} }})".Expr(
        p.OffsetBytes, // {0}
        Decorator.ByteSize, // {1}
        p.OffsetObjects, // {2}
        Decorator.Definition.Priority, // {3}
        p.CallbackPath, // {4}
        p.CreateIndicesExpr() // {5}
      );
    }

    public virtual void EmitStateInterfaceMembers(CodeTypeDeclaration type) {
      EmitSimpleIntefaceMember(type, true, false);
    }

    public virtual void EmitStateClassMembers(StateDecorator decorator, CodeTypeDeclaration type) {
      EmitForwardStateMember(decorator, type);
    }

    public virtual void EmitShimMembers(CodeTypeDeclaration type) {
      throw new NotImplementedException();
    }

    public virtual void EmitModifierMembers(CodeTypeDeclaration type) {
      throw new NotImplementedException();
    }

    public virtual void EmitModifierInterfaceMembers(CodeTypeDeclaration type) {
      EmitSimpleIntefaceMember(type, true, true);
    }

    protected void EmitSimpleIntefaceMember(CodeTypeDeclaration type, bool get, bool set) {
      if (get && set) {
        type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, (_) => { }, (_) => { });
      }
      else if (get) {
        type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, (_) => { }, null);
      }
      else if (set) {
        type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, null, (_) => { });
      }
    }

    protected void EmitForwardStateMember(StateDecorator decorator, CodeTypeDeclaration type) {
      type.DeclareProperty(Decorator.ClrType, Decorator.Definition.Name, get => {
        get.Expr("return (new {0}(Frames.first, 0, 0)).{1}", decorator.RootStruct.Name, Decorator.Definition.Name);
      });
    }

    protected string CallbackDelegateType {
      get {
        return Decorator.DefiningAsset is StateDecorator
        ? String.Format("Action<{0}>", ((StateDecorator)Decorator.DefiningAsset).InterfaceName)
        : String.Format("Action<{0}>", Decorator.DefiningAsset.Definition.Name);
      }
    }

    public static PropertyCodeEmitter Create(PropertyDecorator decorator) {
      PropertyCodeEmitter emitter;

      emitter = decorator.CreateEmitter();
      emitter.Decorator = decorator;

      return emitter;
    }
  }

  public abstract class PropertyCodeEmitter<T> : PropertyCodeEmitter where T : PropertyDecorator {
    public new T Decorator { get { return (T)base.Decorator; } }
  }

}
