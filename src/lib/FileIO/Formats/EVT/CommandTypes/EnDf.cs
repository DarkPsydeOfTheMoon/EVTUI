using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class EnDf : ISerializable
    {
        public const int DataSize = 48;

        public UInt32 Unk1;
        public UInt32 Unk2 = 4354;
        public UInt32 Unk3;

        public float FocalPlaneDistance;
        public float NearBlurSurface;
        public float FarBlurSurface;

        public float DistanceBlurLimit;

        public float BlurStrength = 1.0F;
        public UInt32 BlurType;

        public UInt32[] UNUSED_UINT32 = new UInt32[3];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.Unk1);
            rw.RwUInt32(ref this.Unk2);
            rw.RwUInt32(ref this.Unk3);

            rw.RwUInt32(ref this.UNUSED_UINT32[0]);
            Trace.Assert(this.UNUSED_UINT32[0] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[0]}) in reserve variable.");

            rw.RwFloat32(ref this.FocalPlaneDistance);
            rw.RwFloat32(ref this.NearBlurSurface);
            rw.RwFloat32(ref this.FarBlurSurface);

            rw.RwFloat32(ref this.DistanceBlurLimit);

            rw.RwFloat32(ref this.BlurStrength);
            rw.RwUInt32(ref this.BlurType);

            for (int i=1; i<this.UNUSED_UINT32.Length; i++)
            {
                rw.RwUInt32(ref this.UNUSED_UINT32[i]);
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
            }
        }
    }
}
