﻿using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using UE = UnityEngine;

namespace Bolt {
  [StructLayout(LayoutKind.Explicit)]
  public struct Block {
    [FieldOffset(0)]
    public int Offset;

    [FieldOffset(4)]
    public uint Length;
  }

  public static class Blit {
#if NATIVE_DIFF
    [SuppressUnmanagedCodeSecurity]
    [DllImport("BoltFastCompare", ExactSpelling = true)]
    static extern unsafe int BoltFastCompare(byte* a, byte* b, Block* blocks, uint size, int* Data);

    public unsafe static int Diff(byte[] a, byte[] b, Block[] blocks, int[] Data) {
      int count = 0;

      fixed (byte* aPtr = a) {
        fixed (byte* bPtr = b) {
          fixed (Block* blocksPtr = blocks) {
            fixed (int* resultPtr = Data) {
              count = BoltFastCompare(aPtr, bPtr, blocksPtr, (uint)blocks.Length, resultPtr);
            }
          }
        }
      }

      return count;
    }
#else
    public unsafe static int Diff(byte[] a, byte[] b, Block[] blocks, int[] result) {
      if (blocks.Length == 0) {
        return 0;
      }

      int count = 0;
      int countBlocks = blocks.Length;

      fixed (byte* aPtr = a) {
        fixed (byte* bPtr = b) {
          for (int i = 0; i < countBlocks; ++i) {
            int offset = blocks[i].Offset;
            uint length = blocks[i].Length;

            while (length > 0) {
              if (aPtr[offset] != bPtr[offset]) {
                result[count] = i;
                count += 1;
                break;
              }

              ++offset;
              --length;
            }
          }
        }
      }

      return count;
    }
#endif

    public static void PackEntity(this byte[] data, int offset, BoltEntity entity) {
      if (entity && entity.isAttached) {
        data.PackNetworkId(offset, entity.networkId);
      }
      else {
        data.PackNetworkId(offset, default(NetworkId));
      }
    }

    public static BoltEntity ReadEntity(this byte[] data, int offset) {
      Entity en = BoltCore.FindEntity(data.ReadNetworkId(offset));

      if (en && en.IsAttached) {
        return en.UnityObject;
      }

      return null;
    }

    public static void PackBool(this byte[] data, int offset, bool value) {
      data.PackI32(offset, value ? 1 : 0);
    }

    public static bool ReadBool(this byte[] data, int offset) {
      return data.ReadI32(offset) == 1;
    }

    public static void PackI32(this byte[] data, int offset, int value) {
      data[offset + 0] = (byte)value;
      data[offset + 1] = (byte)(value >> 8);
      data[offset + 2] = (byte)(value >> 16);
      data[offset + 3] = (byte)(value >> 24);
    }

    public static int ReadI32(this byte[] data, int offset) {
      return (data[offset + 0]) | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24);
    }

    public static void PackPrefabId(this byte[] data, int offset, PrefabId value) {
      data.PackI32(offset, value.Value);
    }

    public static PrefabId ReadPrefabId(this byte[] data, int offset) {
      return new PrefabId(data.ReadI32(offset));
    }

    public static void PackU32(this byte[] data, int offset, uint value) {
      data[offset + 0] = (byte)value;
      data[offset + 1] = (byte)(value >> 8);
      data[offset + 2] = (byte)(value >> 16);
      data[offset + 3] = (byte)(value >> 24);
    }

    public static uint ReadU32(this byte[] data, int offset) {
      return (uint)((data[offset + 0]) | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24));
    }

    public static void PackNetworkId(this byte[] data, int offset, NetworkId networkId) {
      data.PackU32(offset, networkId.Connection);
      data.PackU32(offset + 4, networkId.Entity);
    }

    public static NetworkId ReadNetworkId(this byte[] data, int offset) {
      uint connection = data.ReadU32(offset);
      uint entity = data.ReadU32(offset + 4);
      return new NetworkId(connection, entity);
    }

    public static void PackF32(this byte[] data, int offset, float value) {
      BitUnion c = default(BitUnion);
      c.Float32 = value;
      data[offset + 0] = c.Byte0;
      data[offset + 1] = c.Byte1;
      data[offset + 2] = c.Byte2;
      data[offset + 3] = c.Byte3;
    }

    public static float ReadF32(this byte[] data, int offset) {
      BitUnion c = default(BitUnion);
      c.Byte0 = data[offset + 0];
      c.Byte1 = data[offset + 1];
      c.Byte2 = data[offset + 2];
      c.Byte3 = data[offset + 3];
      return c.Float32;
    }

    public static UE.Vector2 ReadVector2(this byte[] data, int offset) {
      BitUnion x = default(BitUnion);
      x.Byte0 = data[offset + 0];
      x.Byte1 = data[offset + 1];
      x.Byte2 = data[offset + 2];
      x.Byte3 = data[offset + 3];

      BitUnion y = default(BitUnion);
      y.Byte0 = data[offset + 4];
      y.Byte1 = data[offset + 5];
      y.Byte2 = data[offset + 6];
      y.Byte3 = data[offset + 7];

      return new UE.Vector2(x.Float32, y.Float32);
    }

    public static void PackVector2(this byte[] data, int offset, UE.Vector2 value) {
      BitUnion x = default(BitUnion);
      BitUnion y = default(BitUnion);

      x.Float32 = value.x;
      y.Float32 = value.y;

      data[offset + 0] = x.Byte0;
      data[offset + 1] = x.Byte1;
      data[offset + 2] = x.Byte2;
      data[offset + 3] = x.Byte3;

      data[offset + 4] = y.Byte0;
      data[offset + 5] = y.Byte1;
      data[offset + 6] = y.Byte2;
      data[offset + 7] = y.Byte3;
    }

    public static UE.Vector3 ReadVector3(this byte[] data, int offset) {
      BitUnion x = default(BitUnion);
      x.Byte0 = data[offset + 0];
      x.Byte1 = data[offset + 1];
      x.Byte2 = data[offset + 2];
      x.Byte3 = data[offset + 3];

      BitUnion y = default(BitUnion);
      y.Byte0 = data[offset + 4];
      y.Byte1 = data[offset + 5];
      y.Byte2 = data[offset + 6];
      y.Byte3 = data[offset + 7];

      BitUnion z = default(BitUnion);
      z.Byte0 = data[offset + 8];
      z.Byte1 = data[offset + 9];
      z.Byte2 = data[offset + 10];
      z.Byte3 = data[offset + 11];

      return new UE.Vector3(x.Float32, y.Float32, z.Float32);
    }

    public static void PackVector3(this byte[] data, int offset, UE.Vector3 value) {
      BitUnion x = default(BitUnion);
      BitUnion y = default(BitUnion);
      BitUnion z = default(BitUnion);

      x.Float32 = value.x;
      y.Float32 = value.y;
      z.Float32 = value.z;

      data[offset + 0] = x.Byte0;
      data[offset + 1] = x.Byte1;
      data[offset + 2] = x.Byte2;
      data[offset + 3] = x.Byte3;

      data[offset + 4] = y.Byte0;
      data[offset + 5] = y.Byte1;
      data[offset + 6] = y.Byte2;
      data[offset + 7] = y.Byte3;

      data[offset + 8] = z.Byte0;
      data[offset + 9] = z.Byte1;
      data[offset + 10] = z.Byte2;
      data[offset + 11] = z.Byte3;
    }

    public static UE.Color ReadColor(this byte[] data, int offset) {
      BitUnion r = default(BitUnion);
      r.Byte0 = data[offset + 0];
      r.Byte1 = data[offset + 1];
      r.Byte2 = data[offset + 2];
      r.Byte3 = data[offset + 3];

      BitUnion g = default(BitUnion);
      g.Byte0 = data[offset + 4];
      g.Byte1 = data[offset + 5];
      g.Byte2 = data[offset + 6];
      g.Byte3 = data[offset + 7];

      BitUnion b = default(BitUnion);
      b.Byte0 = data[offset + 8];
      b.Byte1 = data[offset + 9];
      b.Byte2 = data[offset + 10];
      b.Byte3 = data[offset + 11];

      return new UE.Color(r.Float32, g.Float32, b.Float32);
    }

    public static void PackColor(this byte[] data, int offset, UE.Color value) {
      BitUnion r = default(BitUnion);
      BitUnion g = default(BitUnion);
      BitUnion b = default(BitUnion);

      r.Float32 = value.r;
      g.Float32 = value.g;
      b.Float32 = value.b;

      data[offset + 0] = r.Byte0;
      data[offset + 1] = r.Byte1;
      data[offset + 2] = r.Byte2;
      data[offset + 3] = r.Byte3;

      data[offset + 4] = g.Byte0;
      data[offset + 5] = g.Byte1;
      data[offset + 6] = g.Byte2;
      data[offset + 7] = g.Byte3;

      data[offset + 8] = b.Byte0;
      data[offset + 9] = b.Byte1;
      data[offset + 10] = b.Byte2;
      data[offset + 11] = b.Byte3;
    }

    public static UE.Vector4 ReadVector4(this byte[] data, int offset) {
      BitUnion x = default(BitUnion);
      x.Byte0 = data[offset + 0];
      x.Byte1 = data[offset + 1];
      x.Byte2 = data[offset + 2];
      x.Byte3 = data[offset + 3];

      BitUnion y = default(BitUnion);
      y.Byte0 = data[offset + 4];
      y.Byte1 = data[offset + 5];
      y.Byte2 = data[offset + 6];
      y.Byte3 = data[offset + 7];

      BitUnion z = default(BitUnion);
      z.Byte0 = data[offset + 8];
      z.Byte1 = data[offset + 9];
      z.Byte2 = data[offset + 10];
      z.Byte3 = data[offset + 11];

      BitUnion w = default(BitUnion);
      w.Byte0 = data[offset + 12];
      w.Byte1 = data[offset + 13];
      w.Byte2 = data[offset + 14];
      w.Byte3 = data[offset + 15];

      return new UE.Vector4(x.Float32, y.Float32, z.Float32, w.Float32);
    }

    public static void PackVector4(this byte[] data, int offset, UE.Vector4 value) {
      BitUnion x = default(BitUnion);
      BitUnion y = default(BitUnion);
      BitUnion z = default(BitUnion);
      BitUnion w = default(BitUnion);

      x.Float32 = value.x;
      y.Float32 = value.y;
      z.Float32 = value.z;
      w.Float32 = value.w;

      data[offset + 0] = x.Byte0;
      data[offset + 1] = x.Byte1;
      data[offset + 2] = x.Byte2;
      data[offset + 3] = x.Byte3;

      data[offset + 4] = y.Byte0;
      data[offset + 5] = y.Byte1;
      data[offset + 6] = y.Byte2;
      data[offset + 7] = y.Byte3;

      data[offset + 8] = z.Byte0;
      data[offset + 9] = z.Byte1;
      data[offset + 10] = z.Byte2;
      data[offset + 11] = z.Byte3;

      data[offset + 12] = w.Byte0;
      data[offset + 13] = w.Byte1;
      data[offset + 14] = w.Byte2;
      data[offset + 15] = w.Byte3;
    }

    public static UE.Quaternion ReadQuaternion(this byte[] data, int offset) {
      BitUnion x = default(BitUnion);
      x.Byte0 = data[offset + 0];
      x.Byte1 = data[offset + 1];
      x.Byte2 = data[offset + 2];
      x.Byte3 = data[offset + 3];

      BitUnion y = default(BitUnion);
      y.Byte0 = data[offset + 4];
      y.Byte1 = data[offset + 5];
      y.Byte2 = data[offset + 6];
      y.Byte3 = data[offset + 7];

      BitUnion z = default(BitUnion);
      z.Byte0 = data[offset + 8];
      z.Byte1 = data[offset + 9];
      z.Byte2 = data[offset + 10];
      z.Byte3 = data[offset + 11];

      BitUnion w = default(BitUnion);
      w.Byte0 = data[offset + 12];
      w.Byte1 = data[offset + 13];
      w.Byte2 = data[offset + 14];
      w.Byte3 = data[offset + 15];

      return new UE.Quaternion(x.Float32, y.Float32, z.Float32, w.Float32);
    }

    public static void PackQuaternion(this byte[] data, int offset, UE.Quaternion value) {
      BitUnion x = default(BitUnion);
      BitUnion y = default(BitUnion);
      BitUnion z = default(BitUnion);
      BitUnion w = default(BitUnion);

      x.Float32 = value.x;
      y.Float32 = value.y;
      z.Float32 = value.z;
      w.Float32 = value.w;

      data[offset + 0] = x.Byte0;
      data[offset + 1] = x.Byte1;
      data[offset + 2] = x.Byte2;
      data[offset + 3] = x.Byte3;

      data[offset + 4] = y.Byte0;
      data[offset + 5] = y.Byte1;
      data[offset + 6] = y.Byte2;
      data[offset + 7] = y.Byte3;

      data[offset + 8] = z.Byte0;
      data[offset + 9] = z.Byte1;
      data[offset + 10] = z.Byte2;
      data[offset + 11] = z.Byte3;

      data[offset + 12] = w.Byte0;
      data[offset + 13] = w.Byte1;
      data[offset + 14] = w.Byte2;
      data[offset + 15] = w.Byte3;
    }

    public static void PackString(this byte[] data, int offset, Encoding encoding, string value, int maxLength, int maxBytes) {
      if (value.Length > maxLength) { value = value.Substring(0, maxLength); }

      int bytes = encoding.GetByteCount(value);
      if (bytes > maxBytes) {
        throw new BoltException("Byte count did not match string length");
      }

      data.PackI32(offset, bytes);

      encoding.GetBytes(value, 0, value.Length, data, offset + 4);
    }

    public static string ReadString(this byte[] data, int offset, Encoding encoding) {
      return encoding.GetString(data, offset + 4, data.ReadI32(offset));
    }

    public static void SetTrigger(this byte[] bytes, int frameNew, int offset, bool set) {
      int frame = bytes.ReadI32(offset);
      int bits = bytes.ReadI32(offset + 4);

      if (frame != frameNew) {
        Assert.True(frameNew > frame);

        int diff = frameNew - frame;

        // update bits
        bits = diff < 32 ? bits << diff : 0;

        // flag current frame
        if (set) {
          bits |= 1;
        }

        frame = frameNew;
      }

      bytes.PackI32(offset, frame);
      bytes.PackI32(offset + 4, bits);
    }

    public static void PackBytes(byte[] data, int offset, byte[] bytes) {
      Array.Copy(bytes, 0, data, offset, bytes.Length);
    }

    [StructLayout(LayoutKind.Explicit)]
    struct BitUnion {
      [FieldOffset(0)]
      public Single Float32;

      [FieldOffset(0)]
      public Byte Byte0;
      [FieldOffset(1)]
      public Byte Byte1;
      [FieldOffset(2)]
      public Byte Byte2;
      [FieldOffset(3)]
      public Byte Byte3;
    }


  }
}