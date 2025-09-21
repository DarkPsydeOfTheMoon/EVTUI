using System;
using System.Collections.Generic;
using System.Linq;

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

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 3).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.Unk1);
            rw.RwUInt32(ref this.Unk2);
            rw.RwUInt32(ref this.Unk3);

            rw.RwObj(ref this.UNUSED_UINT32[0], args);

            rw.RwFloat32(ref this.FocalPlaneDistance);
            rw.RwFloat32(ref this.NearBlurSurface);
            rw.RwFloat32(ref this.FarBlurSurface);

            rw.RwFloat32(ref this.DistanceBlurLimit);

            rw.RwFloat32(ref this.BlurStrength);
            rw.RwUInt32(ref this.BlurType);

            rw.RwObj(ref this.UNUSED_UINT32[1], args);
            rw.RwObj(ref this.UNUSED_UINT32[2], args);
        }
    }
}
