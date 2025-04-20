using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class EnFH : ISerializable
    {
        public const int DataSize = 32;

        public UInt32 Unk1 = 1;
        public UInt32 Unk2 = 4354;

        public float StartHeight = 5;
        public float EndHeight   = 2000;
        public UInt32 RGBA       = 0x7F7F7F00;

        public UInt32[] UNUSED_UINT32 = new UInt32[3];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.Unk1);
            rw.RwUInt32(ref this.Unk2);

            rw.RwUInt32(ref this.UNUSED_UINT32[0]);
            rw.RwUInt32(ref this.UNUSED_UINT32[1]);

            rw.RwFloat32(ref this.StartHeight);
            rw.RwFloat32(ref this.EndHeight);
            rw.RwUInt32(ref this.RGBA);

            rw.RwUInt32(ref this.UNUSED_UINT32[2]);

            for (int i=0; i<this.UNUSED_UINT32.Length; i++)
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");

        }
    }
}
