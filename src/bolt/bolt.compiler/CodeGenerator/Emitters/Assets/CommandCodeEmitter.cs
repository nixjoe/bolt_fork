﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  class CommandCodeEmitter : AssetCodeEmitter {
    public new CommandDecorator Decorator {
      get { return (CommandDecorator)base.Decorator; }
      set { base.Decorator = value; }
    }

    public void EmitTypes() {
      EmitClass();
      EmitInputInterface();
      EmitResultInterface();
      EmitFactoryClass();
    }

    void EmitClass() {
      CodeTypeDeclaration type;

      type = Generator.DeclareClass(Decorator.Name);

      type.BaseTypes.Add("Bolt.Command");
      type.BaseTypes.Add(Decorator.InputInterfaceName);
      type.BaseTypes.Add(Decorator.ResultInterfaceName);

      type.DeclareProperty(Decorator.InputInterfaceName, "Input", get => get.Expr("return ({0})this", Decorator.InputInterfaceName));
      type.DeclareProperty(Decorator.ResultInterfaceName, "Result", get => get.Expr("return ({0})this", Decorator.ResultInterfaceName));

      type.DeclareField("Bolt.CommandMetaData", "_meta").Attributes = MemberAttributes.Static;
      type.DeclareConstructorStatic(ctor => {
        ctor.Statements.Expr("_meta.TypeId = new Bolt.TypeId({0})", Decorator.TypeId);
        ctor.Statements.Expr("_meta.SmoothFrames = {0}", Decorator.Definition.SmoothFrames);

        ctor.Statements.Expr("_meta.InputByteSize = {0}", Decorator.InputByteSize);
        ctor.Statements.Expr("_meta.InputSerializers = new Bolt.PropertySerializer[{0}]", Decorator.InputProperties.Count);

        ctor.Statements.Expr("_meta.ResultByteSize = {0}", Decorator.ResultByteSize);
        ctor.Statements.Expr("_meta.ResultSerializers = new Bolt.PropertySerializer[{0}]", Decorator.ResultProperties.Count);

        for (int i = 0; i < Decorator.InputProperties.Count; ++i) {
          CodeExpression expr = PropertyCodeEmitter.Create(Decorator.InputProperties[i]).EmitCommandPropertyInitializer();
          ctor.Statements.Assign("_meta.InputSerializers[{0}]".Expr(i), expr);
        }

        for (int i = 0; i < Decorator.ResultProperties.Count; ++i) {
          CodeExpression expr = PropertyCodeEmitter.Create(Decorator.ResultProperties[i]).EmitCommandPropertyInitializer();
          ctor.Statements.Assign("_meta.ResultSerializers[{0}]".Expr(i), expr);
        }
      });

      type.DeclareConstructor(ctor => {
        ctor.Attributes = MemberAttributes.Assembly;
        ctor.BaseConstructorArgs.Add("_meta".Expr());
      });

      type.DeclareMethod(Decorator.InputInterfaceName, "Create", method => {
        method.Attributes = MemberAttributes.Public | MemberAttributes.Static;
        method.Statements.Expr("return new {0}()", Decorator.Definition.Name);
      });

      for (int i = 0; i < Decorator.InputProperties.Count; ++i) {
        PropertyCodeEmitter.Create(Decorator.InputProperties[i]).EmitCommandMembers(type, "InputData", Decorator.InputInterfaceName);
      }

      for (int i = 0; i < Decorator.ResultProperties.Count; ++i) {
        PropertyCodeEmitter.Create(Decorator.ResultProperties[i]).EmitCommandMembers(type, "ResultData", Decorator.ResultInterfaceName);
      }
    }

    void EmitInputInterface() {
      CodeTypeDeclaration type = Generator.DeclareInterface(Decorator.InputInterfaceName);
      type.BaseTypes.Add("Bolt.ICommandInput");

      for (int i = 0; i < Decorator.InputProperties.Count; ++i) {
        PropertyCodeEmitter.Create(Decorator.InputProperties[i]).EmitSimpleIntefaceMember(type, true, true);
      }
    }

    void EmitResultInterface() {
      CodeTypeDeclaration type = Generator.DeclareInterface(Decorator.ResultInterfaceName);

      for (int i = 0; i < Decorator.ResultProperties.Count; ++i) {
        PropertyCodeEmitter.Create(Decorator.ResultProperties[i]).EmitSimpleIntefaceMember(type, true, true);
      }
    }

    void EmitFactoryClass() {
      CodeTypeDeclaration type;

      type = Generator.DeclareClass(Decorator.FactoryName);
      type.TypeAttributes = System.Reflection.TypeAttributes.NotPublic;
      type.BaseTypes.Add("Bolt.IFactory");
      type.DeclareProperty("System.Type", "TypeObject", get => get.Expr("return typeof({0})", Decorator.Name));
      type.DeclareProperty("Bolt.TypeId", "TypeId", get => get.Expr("return new Bolt.TypeId({0})", Decorator.TypeId));
      type.DeclareMethod(typeof(object).FullName, "Create", method => method.Statements.Expr("return new {0}()", Decorator.Name));
    }
  }
}