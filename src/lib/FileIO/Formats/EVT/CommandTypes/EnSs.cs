using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class EnSs : ISerializable
    {
        public const int DataSize = 48;

        public UInt32 Enable;
        public UInt32 Unk = 4354;

        public float Range = 545.0F;
        public float Radius = 1.15F;
        public float Attenuation = 0.45F;
        public float Concentration = 1.0F;
        public float Blur = 1.5F;

        public UInt32[] UNUSED_UINT32 = new UInt32[5];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.Enable);
            rw.RwUInt32(ref this.Unk);
            Trace.Assert(this.Unk == 4354, $"Unexpected value ({this.Unk}) in constant field (expected value: 4354).");

            rw.RwUInt32(ref this.UNUSED_UINT32[0]);
            rw.RwUInt32(ref this.UNUSED_UINT32[1]);

            rw.RwFloat32(ref this.Range);
            rw.RwFloat32(ref this.Radius);
            rw.RwFloat32(ref this.Attenuation);
            rw.RwFloat32(ref this.Concentration);
            rw.RwFloat32(ref this.Blur);

            rw.RwUInt32(ref this.UNUSED_UINT32[2]);
            rw.RwUInt32(ref this.UNUSED_UINT32[3]);
            rw.RwUInt32(ref this.UNUSED_UINT32[4]);

            for (int i=0; i<this.UNUSED_UINT32.Length; i++)
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
        }
    }
}
