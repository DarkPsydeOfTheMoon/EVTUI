using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class EnOl : ISerializable
    {
        public const int DataSize = 32;

        public UInt32 Unk1 = 1;
        public UInt32 Unk2 = 4354;

        public float Strength;
        public float Width = 1.01F;
        public float Brightness = 1.0F;
        public float RangeMin = 100.0F;
        public float RangeMax = 150.0F;

        public UInt32[] UNUSED_UINT32 = new UInt32[1];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.Unk1);
            Trace.Assert(this.Unk1 == 1, $"Unexpected value ({this.Unk2}) in constant field (expected value: 4354).");

            rw.RwUInt32(ref this.Unk2);
            Trace.Assert(this.Unk2 == 4354, $"Unexpected value ({this.Unk2}) in constant field (expected value: 4354).");

            rw.RwFloat32(ref this.Strength);
            rw.RwFloat32(ref this.Width);
            rw.RwFloat32(ref this.Brightness);
            rw.RwFloat32(ref this.RangeMin);
            rw.RwFloat32(ref this.RangeMax);

            rw.RwUInt32(ref this.UNUSED_UINT32[0]);
            Trace.Assert(this.UNUSED_UINT32[0] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[0]}) in reserve variable.");
        }
    }
}
