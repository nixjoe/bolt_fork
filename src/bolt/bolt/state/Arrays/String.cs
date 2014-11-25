﻿using Bolt;
using System;
using UnityEngine;

namespace Bolt {
  [Documentation]
  public class NetworkArray_String : NetworkArray_Values<String> {
    internal NetworkArray_String(int length)
      : base(length) {
    }

    protected override String GetValue(int index) {
      return Storage.Values[index].String;
    }

    protected override void SetValue(int index, String value) {
      Storage.Values[index].String = value;
    }
  }
}