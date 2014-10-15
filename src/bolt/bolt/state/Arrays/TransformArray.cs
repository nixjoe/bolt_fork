﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Represents an array of transforms on a state
/// </summary>
public struct TransformArray {
  internal Bolt.State.Frame frame;
  internal int offsetObjects;
  internal int offsetBytes;
  internal int length;

  internal TransformArray(Bolt.State.Frame frame, int offsetBytes, int offsetObjects, int length) {
    this.frame = frame;
    this.offsetBytes = offsetBytes;
    this.offsetObjects = offsetObjects;
    this.length = length;
  }

  /// <summary>
  /// The size of the array
  /// </summary>
  public int Length {
    get {
      return length;
    }
  }

  public Bolt.TransformData this[int index] {
    get {
      if (index < 0 || index >= length) throw new IndexOutOfRangeException();
      return frame.Objects[offsetObjects + index] as Bolt.TransformData;
    }
  }
}
