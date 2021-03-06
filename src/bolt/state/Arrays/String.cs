﻿using Bolt;
using System;
using UnityEngine;

namespace Bolt {
  [Documentation]
  public class NetworkArray_String : NetworkArray_Values<String> {
    internal NetworkArray_String(int length, int stride)
      : base(length, stride) {
      Assert.True(stride == 1);
    }

    protected override String GetValue(int index) {
      return Storage.Values[index].String;
    }

    protected override bool SetValue(int index, String value) {
      if (Storage.Values[index].String != value) {
        Storage.Values[index].String = value;
        return true;
      }

      return false;
    }
  }
}