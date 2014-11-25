﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  internal class NetworkStorage : IBoltListNode {
    public int Frame;
    public BitSet Changed;
    public NetworkObj Root;
    public NetworkValue[] Values;

    public NetworkStorage(int size) {
      Values = new NetworkValue[size];
    }

    public void PropertyChanged(int property) {
      Changed.Set(property);
      Assert.False(Changed.IsZero);
    }

    object IBoltListNode.prev {
      get;
      set;
    }

    object IBoltListNode.next {
      get;
      set;
    }

    object IBoltListNode.list {
      get;
      set;
    }
  }

  //class NetworkFrame : IBoltListNode {
  //  public int Number;
  //  public bool Pooled;

  //  public State State;
  //  public BitSet Changed;

  //  public readonly NetworkValue[] Storage;

  //  public NetworkFrame(int number, int size) {
  //    Number = number;
  //    Storage = new NetworkValue[size];
  //  }

  //  public void PropertyChanged(int property) {
  //    Changed.Set(property);
  //  }

  //  object IBoltListNode.prev {
  //    get;
  //    set;
  //  }

  //  object IBoltListNode.next {
  //    get;
  //    set;
  //  }

  //  object IBoltListNode.list {
  //    get;
  //    set;
  //  }
  //}

  //class NetworkFramePool {
  //  public readonly int Size;
  //  public readonly Stack<NetworkFrame> Pool = new Stack<NetworkFrame>();

  //  public NetworkFramePool(int frameSize) {
  //    Size = frameSize;
  //  }

  //  public NetworkFrame Allocate(State state, int number) {
  //    NetworkFrame f;

  //    if (Pool.Count > 0) {
  //      f = Pool.Pop();
  //      Assert.True(f.Pooled);
  //    }
  //    else {
  //      f = new NetworkFrame(0, Size);
  //    }

  //    f.Pooled = false;
  //    f.Number = number;
  //    f.State = state;

  //    return f;
  //  }

  //  public void Free(NetworkFrame f) {
  //    Assert.False(f.Pooled);
  //    Assert.True(f.Storage.Length == Size);

  //    Array.Clear(f.Storage, 0, Size);

  //    Pool.Push(f);

  //    f.Pooled = true;
  //  }

  //  public NetworkFrame Duplicate(NetworkFrame f, int number) {
  //    NetworkFrame c = Allocate(f.State, number);

  //    // make sure they are the same
  //    Assert.True(f.Storage.Length == c.Storage.Length);

  //    // copy storage
  //    Array.Copy(f.Storage, 0, c.Storage, 0, f.Storage.Length);

  //    return c;
  //  }
  //}

}
