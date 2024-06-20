using System;
using System.Collections.Generic;
using System.IO;

namespace Serialization
{
    public interface IBaseBinaryTarget
    {
        public void SetLittleEndian(bool isLittleEndian);
        public void ResetEndianness();
        public bool IsConstructlike();
        public bool IsParselike();

        // Basic type read/writes
        public void RwBytestring (ref byte[] value, int length);
        public void RwString     (ref string value, int length, System.Text.Encoding encoding);
        public void RwCBytestring(ref byte[] value);
        public void RwCString    (ref string value, System.Text.Encoding encoding);
        public void RwInt8    (ref sbyte    value);
        public void RwUInt8   (ref byte     value);
        public void RwInt16   (ref Int16    value);
        public void RwUInt16  (ref UInt16   value);
        public void RwInt32   (ref Int32    value);
        public void RwUInt32  (ref UInt32   value);
        public void RwInt64   (ref Int64    value);
        public void RwUInt64  (ref UInt64   value);
        public void RwFloat16 (ref Half     value);
        public void RwFloat32 (ref Single   value);
        public void RwFloat64 (ref Double   value);
        public void RwInt8s   (ref sbyte [] value, int count);
        public void RwUInt8s  (ref byte  [] value, int count);
        public void RwInt16s  (ref Int16 [] value, int count);
        public void RwUInt16s (ref UInt16[] value, int count);
        public void RwInt32s  (ref Int32 [] value, int count);
        public void RwUInt32s (ref UInt32[] value, int count);
        public void RwInt64s  (ref Int64 [] value, int count);
        public void RwUInt64s (ref UInt64[] value, int count);
        public void RwFloat16s(ref Half  [] value, int count);
        public void RwFloat32s(ref Single[] value, int count);
        public void RwFloat64s(ref Double[] value, int count);

        // Struct read/writes
        public void RwObj<T>(T obj, Dictionary<string, object> args = null) where T : ISerializable;
        public void RwObj<T>(ref T obj, Dictionary<string, object> args = null) where T : ISerializable;
        public void RwObjs<T>(ref T[] objs, int count, Dictionary<string, object> args = null) where T : ISerializable;

        // Stream manipulation methods
        public static long GetAlignment(long position, long alignment)
        {
            return (alignment - (position % alignment)) % alignment;
        }

        public void Align(long position, long alignment);
        public void ValidatedAlign(long position, long alignment);

        public long Tell();
        public long GetRelativeOffset();
        public void SetRelativeOffset(long val);
        public void ResetRelativeOffset();
        public long RelativeTell();
        public void Seek(long offset, SeekOrigin origin);
        public void RelativeSeek(long offset, SeekOrigin origin);
        //public long ActOnOffset(long offset);

        public bool IsEOF();
        public void AssertEOF();
    }
}
