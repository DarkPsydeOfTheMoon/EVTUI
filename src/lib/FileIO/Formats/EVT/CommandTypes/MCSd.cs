using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MCSd : ISerializable
    {
        public const int DataSize = 32;

        public UInt32 UnkEnum = 3;
        public byte[] RGBA1 = new byte[] {0, 0, 0, 128}; 
        public byte[] RGBA2 = new byte[] {0, 0, 0, 96}; 
        public UInt16 UnkInd1 = 20;
        public UInt16 UnkInd2 = 40;

        public UInt32[] UNUSED_UINT32 = new UInt32[4];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.UnkEnum);
            rw.RwUInt8s(ref this.RGBA1, 4);
            rw.RwUInt8s(ref this.RGBA2, 4);

            rw.RwUInt32(ref this.UNUSED_UINT32[0]);

            rw.RwUInt16(ref this.UnkInd1);
            rw.RwUInt16(ref this.UnkInd2);

            rw.RwUInt32(ref this.UNUSED_UINT32[1]);
            rw.RwUInt32(ref this.UNUSED_UINT32[2]);
            rw.RwUInt32(ref this.UNUSED_UINT32[3]);

            for (int i=0; i<this.UNUSED_UINT32.Length; i++)
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
        }
    }
}
