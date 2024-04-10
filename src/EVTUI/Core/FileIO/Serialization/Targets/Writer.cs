using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;


namespace Serialization
{
    struct Writer : IBaseBinaryTarget
    {
        private bool IsLittleEndian = BitConverter.IsLittleEndian;
        public void SetEndianness(string endianness) {
            if (endianness == "little")
                this.IsLittleEndian = true;
            else if (endianness == "big")
                this.IsLittleEndian = false;
            else
                throw new Exception("Endianness must be little or big");
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
            if (!this.IsLittleEndian)
			    value = value.Reverse().ToArray();
            this.bytestream.Write(value);
        }

        public void RwString(ref string value, int length, System.Text.Encoding encoding)
        {
            var buf = encoding.GetBytes(value);
            if (!this.IsLittleEndian)
			    buf = buf.Reverse().ToArray();
            this.bytestream.Write(buf);
        }

        public void RwCBytestring(ref byte[] value)
        {
            if (!this.IsLittleEndian)
			    value = value.Reverse().ToArray();
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
        public void RwUint64 (ref UInt64 value) { this.endian_writer(ref value); }
        public void RwFloat16(ref Half   value) { this.endian_writer(ref value); }
        public void RwFloat32(ref float  value) { this.endian_writer(ref value); }
        public void RwFloat64(ref double value) { this.endian_writer(ref value); }

        private unsafe void endian_writer<T>(ref T value) {
            //byte[] bytes = BitConverter.GetBytes(value);
            var bytes = new byte[Unsafe.SizeOf<T>()];
            Unsafe.As<byte, T>(ref bytes[0]) = value;
            //byte[] bytes = (byte[]) (object) value;
            if (!this.IsLittleEndian)
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
        public void RwObj<T>(T obj) where T : ISerializable { obj.ExbipHook(this); }
        public void RwObj<T>(ref T obj) where T : struct, ISerializable { obj.ExbipHook(this); }
        public void RwObjs<T>(ref T[] objs, int count) where T : ISerializable 
        {
            foreach (var obj in objs)
                this.RwObj(obj);
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

        public void Seek(long position, SeekOrigin origin)
        {
            this.bytestream.BaseStream.Seek(position, origin);
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

        public void AssertEOF() {
            // do nothing
        }
    }
}
