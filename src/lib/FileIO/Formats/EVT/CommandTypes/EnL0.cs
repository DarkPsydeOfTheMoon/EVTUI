using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class EnL0 : ISerializable
    {
        public const int DataSize = 48;

        public UInt32 Unk1 = 1;
        public UInt32 Unk2 = 4354;

        public UInt32 AmbientRGBA = 0xBFBFBFFF;
        public UInt32 DiffuseRGBA = 0xE5E5E5FF;
        public UInt32 SpecularRGBA = 0xFFFFFFFF;

        public float[] Direction = new float[] {0.0F, 0.5299989581108093F, 0.847998321056366F};

        public UInt32[] UNUSED_UINT32 = new UInt32[4];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.Unk1);
            Trace.Assert(this.Unk1 == 1, $"Unexpected value ({this.Unk2}) in constant field (expected value: 4354).");

            rw.RwUInt32(ref this.Unk2);
            Trace.Assert(this.Unk2 == 4354, $"Unexpected value ({this.Unk2}) in constant field (expected value: 4354).");

            rw.RwUInt32(ref this.UNUSED_UINT32[0]);
            rw.RwUInt32(ref this.UNUSED_UINT32[1]);

            rw.RwUInt32(ref this.AmbientRGBA);
            rw.RwUInt32(ref this.DiffuseRGBA);
            rw.RwUInt32(ref this.SpecularRGBA);

            rw.RwFloat32s(ref this.Direction, 3);

            rw.RwUInt32(ref this.UNUSED_UINT32[2]);
            rw.RwUInt32(ref this.UNUSED_UINT32[3]);

            for (int i=0; i<this.UNUSED_UINT32.Length; i++)
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
        }
    }
}
