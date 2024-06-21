using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;


namespace Serialization
{
    struct Reader : IBaseBinaryTarget
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

        public bool IsConstructlike() { return true; }
        public bool IsParselike() { return false; }

        private BinaryReader bytestream;

        public Reader(BinaryReader reader)
        {
            this.bytestream = reader;
        }

        // Basic types
        public void RwBytestring(ref byte[] value, int length) 
        {
            value = this.bytestream.ReadBytes(length);
        }
        public void RwString(ref string value, int length, System.Text.Encoding encoding)
        {
            byte[] buf = this.bytestream.ReadBytes(length);
            value = encoding.GetString(buf);
        }
        public void RwCBytestring(ref byte[] value)
        {
            // Bad implementation
            byte v  = this.bytestream.ReadByte();
            List<byte> buf = new List<byte> {};
            while (v != 0x00)
            {
                buf.Add(v);
                v = this.bytestream.ReadByte();
            }
            value = buf.ToArray();
        }
        public void RwCString(ref string value, System.Text.Encoding encoding)
        {
            byte[] buf = Array.Empty<byte>();
            this.RwCBytestring(ref buf);
            value = encoding.GetString(buf);
        }

        public void RwInt8   (ref sbyte  value) { this.endian_reader(ref value, (x => (sbyte) Convert.ChangeType(x[0], typeof(sbyte)) )); }
        public void RwUInt8  (ref byte   value) { this.endian_reader(ref value, (x => (byte) Convert.ChangeType(x[0], typeof(byte)))); }
        public void RwInt16  (ref Int16  value) { this.endian_reader(ref value, (x => BitConverter.ToInt16(x))); }
        public void RwUInt16 (ref UInt16 value) { this.endian_reader(ref value, (x => BitConverter.ToUInt16(x))); }
        public void RwInt32  (ref Int32  value) { this.endian_reader(ref value, (x => BitConverter.ToInt32(x))); }
        public void RwUInt32 (ref UInt32 value) { this.endian_reader(ref value, (x => BitConverter.ToUInt32(x))); }
        public void RwInt64  (ref Int64  value) { this.endian_reader(ref value, (x => BitConverter.ToInt64(x))); }
        public void RwUInt64 (ref UInt64 value) { this.endian_reader(ref value, (x => BitConverter.ToUInt64(x))); }
        public void RwFloat16(ref Half   value) { this.endian_reader(ref value, (x => BitConverter.ToHalf(x))); }
        public void RwFloat32(ref float  value) { this.endian_reader(ref value, (x => BitConverter.ToSingle(x))); }
        public void RwFloat64(ref double value) { this.endian_reader(ref value, (x => BitConverter.ToDouble(x))); }

        private unsafe void endian_reader<T>(ref T value, Func<byte[], T> ConvData) {
            byte[] bytes = new byte[sizeof(T)];
            this.bytestream.Read(bytes, 0, sizeof(T));
            if (!this.IsLittleEndian.Peek())
                bytes = bytes.Reverse().ToArray();
            value = ConvData(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private void rw_typeds<T>(ref T[] value, int count, Func<byte[], T> ConvData) // Func *should* be CTO'd as an inline func in each specialisation..?!
        {
            value = new T[count];
            for (int i=0; i<count; ++i)
                this.endian_reader(ref value[i], ConvData);
        }

        public void RwInt8s   (ref sbyte [] value, int count) { rw_typeds(ref value, count, (x => (sbyte) Convert.ChangeType(x[0], typeof(sbyte)))); }
        public void RwUInt8s  (ref byte  [] value, int count) { rw_typeds(ref value, count, (x => (byte) Convert.ChangeType(x[0], typeof(byte)))); }
        public void RwInt16s  (ref Int16 [] value, int count) { rw_typeds(ref value, count, (x => BitConverter.ToInt16(x))); }
        public void RwUInt16s (ref UInt16[] value, int count) { rw_typeds(ref value, count, (x => BitConverter.ToUInt16(x))); }
        public void RwInt32s  (ref Int32 [] value, int count) { rw_typeds(ref value, count, (x => BitConverter.ToInt32(x))); }
        public void RwUInt32s (ref UInt32[] value, int count) { rw_typeds(ref value, count, (x => BitConverter.ToUInt32(x))); }
        public void RwInt64s  (ref Int64 [] value, int count) { rw_typeds(ref value, count, (x => BitConverter.ToInt64(x)));  }
        public void RwUInt64s (ref UInt64[] value, int count) { rw_typeds(ref value, count, (x => BitConverter.ToUInt64(x))); }
        public void RwFloat16s(ref Half  [] value, int count) { rw_typeds(ref value, count, (x => BitConverter.ToHalf(x)));   }
        public void RwFloat32s(ref Single[] value, int count) { rw_typeds(ref value, count, (x => BitConverter.ToSingle(x))); }
        public void RwFloat64s(ref Double[] value, int count) { rw_typeds(ref value, count, (x => BitConverter.ToDouble(x))); }

        // Struct read/write
        public void RwObj<T>(T obj, Dictionary<string, object> args = null)     where T : ISerializable { obj.ExbipHook(this, args); }
        public void RwObj<T>(ref T obj, Dictionary<string, object> args = null) where T : ISerializable
        {
            if (obj is null)
                obj = (T) typeof(T).GetConstructors().First().Invoke([]);
            obj.ExbipHook(this, args);
        }
        public void RwObjs<T>(ref T[] objs, int count, Dictionary<string, object> args = null) where T : ISerializable 
        {
             objs = new T[count];
             for (int i=0; i < count; ++i)
             {
                objs[i] = (T) typeof(T).GetConstructors().First().Invoke([]);
                this.RwObj(objs[i], args);
             }
        }

        // Stream Manipulation
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
            this.Seek(skiplength, SeekOrigin.Current);
        }

        public void ValidatedAlign(long position, long alignment)
        {
            int skiplength = (int)IBaseBinaryTarget.GetAlignment(position, alignment);
            var buf = this.bytestream.ReadBytes(skiplength);
            foreach (var v in buf)
                Trace.Assert(v == 0x00, "Expected alignment buffer to be 0x00");
        }

        public bool IsEOF()
        {
            return (this.bytestream.PeekChar() < 0);
        }

        public void AssertEOF()
        {
            Trace.Assert(this.IsEOF(), "Finished reading the stream before EOF was reached");
        }
    }
}
