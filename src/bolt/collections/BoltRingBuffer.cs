﻿using System;
using Bolt;

[Documentation(Ignore = true)]
public class BoltRingBuffer<T> : System.Collections.Generic.IEnumerable<T> {
  int _head;
  int _tail;
  int _count;
  bool _autofree;

  readonly T[] array;

  public bool full {
    get { return _count == array.Length; }
  }

  public bool empty {
    get { return _count == 0; }
  }

  public bool autofree {
    get { return _autofree; }
    set { _autofree = value; }
  }

  public int count {
    get { return _count; }
  }

  public T last {
    get {
      VerifyNotEmpty();
      return this[count - 1];
    }
    set {
      VerifyNotEmpty();
      this[count - 1] = value;
    }
  }

  public T first {
    get {
      VerifyNotEmpty();
      return this[0];
    }
    set {
      VerifyNotEmpty();
      this[0] = value;
    }
  }

  public T this[int index] {
    get {
      VerifyNotEmpty();
      return array[(_tail + index) % array.Length];
    }
    set {
      if (index >= _count) {
        throw new IndexOutOfRangeException("can't change value of non-existand index");
      }

      array[(_tail + index) % array.Length] = value;
    }
  }

  public BoltRingBuffer (int size) {
    array = new T[size];
  }

  public void Enqueue (T item) {
    if (_count == array.Length) {
      if (_autofree) {
        Dequeue();
      } else {
        throw new InvalidOperationException("buffer is full");
      }
    }

    array[_head] = item;
    _head = (_head + 1) % array.Length;
    _count += 1;
  }

  public T Dequeue () {
    VerifyNotEmpty();
    T item = array[_tail];
    array[_tail] = default(T);
    _tail = (_tail + 1) % array.Length;
    _count -= 1;
    return item;
  }

  public T Peek () {
    VerifyNotEmpty();
    return array[_tail];
  }

  public void Clear () {
    Array.Clear(array, 0, array.Length);
    _count = _tail = _head = 0;
  }

  public void CopyTo (BoltRingBuffer<T> other) {
    if (this.array.Length != other.array.Length) {
      throw new InvalidOperationException("buffers must be of the same capacity");
    }

    other._head = this._head;
    other._tail = this._tail;
    other._count = this._count;

    Array.Copy(this.array, 0, other.array, 0, this.array.Length);
  }

  void VerifyNotEmpty () {
    if (_count == 0) {
      throw new InvalidOperationException("buffer is empty");
    }
  }

  public System.Collections.Generic.IEnumerator<T> GetEnumerator () {
    for (int i = 0; i < _count; ++i) {
      yield return this[i];
    }
  }

  System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
    return this.GetEnumerator();
  }
}
