﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.CodeDom;
using ProtoBuf;

namespace Bolt.Compiler {
  public struct AssetFolderWithParent {
    public AssetFolder Folder;
    public AssetFolder Parent;
  }

  public interface INamedAsset {
    string GetName();
  }

  [ProtoContract]
  public class AssetFolder : INamedAsset {
    [ProtoMember(1)]
    public string Name;

    [ProtoMember(2)]
    public bool Expanded;

    [ProtoMember(3)]
    public AssetFolder[] Folders = new AssetFolder[0];

    [ProtoMember(4)]
    public AssetDefinition[] Assets = new AssetDefinition[0];

    [ProtoMember(5)]
    public Guid Guid;

    public IEnumerable<INamedAsset> Children {
      get { return Folders.OrderBy(x => x.Name).Cast<INamedAsset>().Concat(Assets.OrderBy(x => x.Name).Cast<INamedAsset>()); }
    }

    public INamedAsset FindFirstChild() {
      return Children.FirstOrDefault();
    }

    public AssetFolder FindParentFolder(INamedAsset asset) {
      if (Children.Contains(asset)) {
        return this;
      }

      foreach (AssetFolder f in Folders) {
        AssetFolder parent = f.FindParentFolder(asset);

        if (parent != null) {
          return parent;
        }
      }

      return null;
    }

    public INamedAsset FindPrevSibling(INamedAsset asset) {
      return FindSibling(asset, true);
    }

    public INamedAsset FindNextSibling(INamedAsset asset) {
      return FindSibling(asset, false);
    }

    INamedAsset FindSibling(INamedAsset asset, bool prev) {
      if (Children.Contains(asset)) {
        var array = Children.ToArray();
        var index = Array.IndexOf(array, asset);

        if (prev) {
          if (index == 0) {
            return null;
          }

          return array[index - 1];
        }
        else {
          if (index + 1 == array.Length) {
            return null;
          }

          return array[index + 1];
        }
      }

      foreach (AssetFolder f in Folders) {
        INamedAsset sibling = f.FindSibling(asset, prev);

        if (sibling != null) {
          return sibling;
        }
      }

      return null;
    }

    public IEnumerable<AssetDefinition> AssetsAll {
      get { return Assets.Concat(Folders.SelectMany(x => x.AssetsAll)); }
    }

    string INamedAsset.GetName() {
      return Name;
    }
  }

  [ProtoContract]
  public class Project {
    //List<EventDefinition> events = new List<EventDefinition>();
    //List<CommandDefinition> commands = new List<CommandDefinition>();

    [ProtoMember(1)]
    public AssetFolder RootFolder = new AssetFolder();

    [ProtoMember(2)]
    public PropertyFilterDefinition[] Filters = new PropertyFilterDefinition[0];

    public IEnumerable<StateDefinition> States {
      get { return RootFolder.AssetsAll.Where(x => x is StateDefinition).Cast<StateDefinition>(); }
    }

    public IEnumerable<StructDefinition> Structs {
      get { return RootFolder.AssetsAll.Where(x => x is StructDefinition).Cast<StructDefinition>(); }
    }

    public void Add(PropertyFilterDefinition filter) {
      //Filters.Add(filter);
    }

    public StateDefinition FindState(Guid guid) {
      return States.First(x => x.Guid == guid);
    }

    public StructDefinition FindStruct(Guid guid) {
      return Structs.First(x => x.Guid == guid);
    }

    //public EventDefinition FindEvent(Guid guid) {
    //  return events.First(x => x.Guid == guid);
    //}

    //public CommandDefinition FindCommand(Guid guid) {
    //  return commands.First(x => x.Guid == guid);
    //}

    public void GenerateCode(string file) {
      CodeGenerator cg;

      cg = new CodeGenerator();
      cg.Run(this, file);
    }
  }
}
