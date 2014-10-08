﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public struct FloatArray {
  internal Bolt.State.Frame frame;
  internal int offsetObjects;
  internal int offsetBytes;
  internal int length;

  internal FloatArray(Bolt.State.Frame frame, int offsetBytes, int offsetObjects, int length) {
    this.frame = frame;
    this.offsetBytes = offsetBytes;
    this.offsetObjects = offsetObjects;
    this.length = length;
  }

  public int Length {
    get {
      return length;
    }
  }

  public float this[int index] {
    get {
      if (index < 0 || index >= length) throw new IndexOutOfRangeException();
      return Bolt.Blit.ReadF32(frame.Data, offsetBytes + (index * 4));
    }
  }

  public FloatArrayModifier Modify() {
    return new FloatArrayModifier(this);
  }
}

public class FloatArrayModifier : IDisposable {
  FloatArray array;

  internal FloatArrayModifier(FloatArray array) {
    this.array = array;
  }

  public int Length {
    get {
      return array.length;
    }
  }

  public float this[int index] {
    get { return array[index]; }
    set {
      if (index < 0 || index >= array.length) throw new IndexOutOfRangeException();
      Bolt.Blit.PackF32(array.frame.Data, array.offsetBytes + (index * 4), value);
    }
  }

  public void Dispose() {

  }
}