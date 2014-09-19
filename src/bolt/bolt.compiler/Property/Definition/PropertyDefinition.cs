﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace bolt.compiler {
  [ProtoContract]
  public class PropertyDefinition {
    [ProtoIgnore]
    public bool Deleted;

    [ProtoIgnore]
    public Context Context;

    [ProtoMember(3)]
    public bool Enabled;

    [ProtoMember(4)]
    public bool Replicated;

    [ProtoMember(5)]
    public bool Expanded;

    [ProtoMember(7)]
    public string Comment;

    [ProtoMember(2)]
    public PropertyType PropertyType;

    [ProtoMember(6)]
    public PropertyDefinitionAssetSettings AssetSettings;

    [ProtoMember(8)]
    public HashSet<int> Filters = new HashSet<int>(new[] { 0, 1 });

    public PropertyDefinitionStateAssetSettings StateAssetSettings {
      get { return (PropertyDefinitionStateAssetSettings)AssetSettings; }
    }

    public PropertyDefinitionEventAssetSettings EventAssetSettings {
      get { return (PropertyDefinitionEventAssetSettings)AssetSettings; }
    }
  }
}
