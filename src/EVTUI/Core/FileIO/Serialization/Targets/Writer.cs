using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;


namespace Serialization
{
    struct Writer : IBaseBinaryTarget
    {
        private Stack<bool> IsLittleEndian = new Stack<bool>(new bool[] {BitConverter.IsLittleEndian});
        public void SetLittleEndian(bool isLittleEndian)
        {
            this.IsLittleEndian.Push(isLittleEndian);
        }
        public void ResetEndianness()
        {
            if (this.IsLittleEndian.Count > 1)
                this.IsLittleEndian.Pop();
        }

        public bool IsConstructlike() { return false; }
        public bool IsParselike() { return true; }

        private BinaryWriter bytestream;

        public Writer(BinaryWriter writer)
        {
            this.bytestream = writer;
        }

        // Basic types
        public void RwBytestring(ref byte[] value, int length)
        {
            this.bytestream.Write(value);
        }

        public void RwString(ref string value, int length, System.Text.Encoding encoding)
        {
            var buf = encoding.GetBytes(value);
            this.bytestream.Write(buf);
        }

        public void RwCBytestring(ref byte[] value)
        {
            this.bytestream.Write(value);
            this.bytestream.Write(0x00);
        }

        public void RwCString(ref string value, System.Text.Encoding encoding)
        {
            byte[] buf = encoding.GetBytes(value);
            this.RwCBytestring(ref buf);
        }

        public void RwInt8   (ref sbyte  value) { this.endian_writer(ref value); }
        public void RwUInt8  (ref byte   value) { this.endian_writer(ref value); }
        public void RwInt16  (ref Int16  value) { this.endian_writer(ref value); }
        public void RwUInt16 (ref UInt16 value) { this.endian_writer(ref value); }
        public void RwInt32  (ref Int32  value) { this.endian_writer(ref value); }
        public void RwUInt32 (ref UInt32 value) { this.endian_writer(ref value); }
        public void RwInt64  (ref Int64  value) { this.endian_writer(ref value); }
        public void RwUInt64 (ref UInt64 value) { this.endian_writer(ref value); }
        public void RwFloat16(ref Half   value) { this.endian_writer(ref value); }
        public void RwFloat32(ref float  value) { this.endian_writer(ref value); }
        public void RwFloat64(ref double value) { this.endian_writer(ref value); }

        private unsafe void endian_writer<T>(ref T value) {
            var bytes = new byte[Unsafe.SizeOf<T>()];
            Unsafe.As<byte, T>(ref bytes[0]) = value;
            //if (!this.IsLittleEndian)
            if (!this.IsLittleEndian.Peek())
                bytes = bytes.Reverse().ToArray();
            this.bytestream.Write(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private void rw_typeds<T>(ref T[] value, int count)
        {
            for (int i=0; i<count; ++i)
                this.endian_writer(ref value[i]);
        }

        public void RwInt8s   (ref sbyte [] value, int count) { this.rw_typeds(ref value, count); }
        public void RwUInt8s  (ref byte  [] value, int count) { this.rw_typeds(ref value, count); }
        public void RwInt16s  (ref Int16 [] value, int count) { this.rw_typeds(ref value, count); }
        public void RwUInt16s (ref UInt16[] value, int count) { this.rw_typeds(ref value, count); }
        public void RwInt32s  (ref Int32 [] value, int count) { this.rw_typeds(ref value, count); }
        public void RwUInt32s (ref UInt32[] value, int count) { this.rw_typeds(ref value, count); }
        public void RwInt64s  (ref Int64 [] value, int count) { this.rw_typeds(ref value, count); }
        public void RwUInt64s (ref UInt64[] value, int count) { this.rw_typeds(ref value, count); }
        public void RwFloat16s(ref Half  [] value, int count) { this.rw_typeds(ref value, count); }
        public void RwFloat32s(ref Single[] value, int count) { this.rw_typeds(ref value, count); }
        public void RwFloat64s(ref Double[] value, int count) { this.rw_typeds(ref value, count); }

        // Struct read/write
        public void RwObj<T>(T obj, Dictionary<string, object> args = null) where T : ISerializable { obj.ExbipHook(this, args); }
        public void RwObj<T>(ref T obj, Dictionary<string, object> args = null) where T : ISerializable { obj.ExbipHook(this, args); }
        public void RwObjs<T>(ref T[] objs, int count, Dictionary<string, object> args = null) where T : ISerializable 
        {
            foreach (var obj in objs)
                this.RwObj(obj, args);
        }

        public void rw_uint32s_explicit(ref UInt32[] value, int count)
        {
            for (int i=0; i<count; ++i)
                this.bytestream.Write(value[i]);
        }

        public long Tell()
        {
            return this.bytestream.BaseStream.Position;
        }

        private Stack<long> RelativeOffset = new Stack<long>(new long[] {0});
        public long GetRelativeOffset()
        {
            return this.RelativeOffset.Peek();
        }
        public void SetRelativeOffset(long val)
        {
            this.RelativeOffset.Push(val);
        }
        public void ResetRelativeOffset()
        {
            if (this.RelativeOffset.Count > 1)
                this.RelativeOffset.Pop();
        }
        public long RelativeTell()
        {
            return this.Tell() - this.RelativeOffset.Peek();
        }

        public void Seek(long position, SeekOrigin origin)
        {
            this.bytestream.BaseStream.Seek(position, origin);
        }
        public void RelativeSeek(long position, SeekOrigin origin)
        {
            this.bytestream.BaseStream.Seek(position + this.RelativeOffset.Peek(), origin);
        }

        public void Align(long position, long alignment)
        {
            long skiplength = IBaseBinaryTarget.GetAlignment(position, alignment);
            for (int i=0; i < skiplength; ++i)
                this.bytestream.Write((byte)0x00);
        }

        public void ValidatedAlign(long position, long alignment)
        {
            long skiplength = IBaseBinaryTarget.GetAlignment(position, alignment);
            for (int i=0; i < skiplength; ++i)
                this.bytestream.Write((byte)0x00);   
        }

        public bool IsEOF()
        {
            return true;
        }

        public void AssertEOF() {
            // do nothing
        }
    }
}
