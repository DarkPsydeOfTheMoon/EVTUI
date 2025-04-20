using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class EnHd : ISerializable
    {
        public const int DataSize = 80;

        public Bitfield32 Flags = new Bitfield32();
        public UInt32 Unk1 = 4354;

        public float ToneMapMediumBrightness;
        public float ToneMapBloomStrength = 0.3F;
        public float ToneMapAdaptiveBrightness;
        public float ToneMapAdaptiveBloom = 0.016666667F;

        public UInt32 StarFilterNumberOfLines           = 4;
        public float StarFilterLength                   = 1.0F;
        public float StarFilterStrength                 = 0.5F;
        public float StarFilterGlareChromaticAberration = 1.5F;
        public float StarFilterGlareTilt                = 25.0F;

        public float UnkFloat1 = 3.8F;
        public float UnkFloat2 = 0.35F;
        public float UnkFloat3 = 0.03F;

        public UInt32 UnkEnum = 1;
        public UInt32 RGBA1;
        public UInt32 RGBA2;
        public UInt32 RGBA3;

        public UInt32[] UNUSED_UINT32 = new UInt32[2];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.Flags);
            rw.RwUInt32(ref this.Unk1);

            for (int i=0; i<this.UNUSED_UINT32.Length; i++)
            {
                rw.RwUInt32(ref this.UNUSED_UINT32[i]);
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
            }

            rw.RwFloat32(ref this.ToneMapMediumBrightness);
            rw.RwFloat32(ref this.ToneMapBloomStrength);
            rw.RwFloat32(ref this.ToneMapAdaptiveBrightness);
            rw.RwFloat32(ref this.ToneMapAdaptiveBloom);

            rw.RwUInt32(ref this.StarFilterNumberOfLines);
            rw.RwFloat32(ref this.StarFilterLength);
            rw.RwFloat32(ref this.StarFilterStrength);
            rw.RwFloat32(ref this.StarFilterGlareChromaticAberration);
            rw.RwFloat32(ref this.StarFilterGlareTilt);

            rw.RwFloat32(ref this.UnkFloat1);
            rw.RwFloat32(ref this.UnkFloat2);
            rw.RwFloat32(ref this.UnkFloat3);

            if ((int)args["dataSize"] > 64)
            {
                rw.RwUInt32(ref this.UnkEnum);
                rw.RwUInt32(ref this.RGBA1);
                rw.RwUInt32(ref this.RGBA2);
                rw.RwUInt32(ref this.RGBA3);
            }
        }
    }
}
