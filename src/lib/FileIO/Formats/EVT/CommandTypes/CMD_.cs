using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class CMD_ : ISerializable
    {
        public const int DataSize = 64;

        public Bitfield32 Flags = new Bitfield32(15);

        public float[] ViewportCoordinates = new float[3];
        public float[] ViewportRotation = new float[3];
        public float AngleOfView = 45.0F;

        public UInt32 InterpolationParameters = 4354;

        public float FocalPlaneDistance;
        public float NearBlurSurface;
        public float FarBlurSurface;

        public float BlurStrength = 1.0F;
        public UInt32 BlurType;

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 2).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.Flags);

            rw.RwFloat32s(ref this.ViewportCoordinates, 3);
            rw.RwFloat32s(ref this.ViewportRotation, 3);
            rw.RwFloat32(ref this.AngleOfView);

            rw.RwUInt32(ref this.InterpolationParameters);

            rw.RwFloat32(ref this.FocalPlaneDistance);
            rw.RwFloat32(ref this.NearBlurSurface);
            rw.RwFloat32(ref this.FarBlurSurface);

            if ((int)args["dataSize"] > 48)
            {
                rw.RwFloat32(ref this.BlurStrength);
                rw.RwUInt32(ref this.BlurType);

                rw.RwObj(ref this.UNUSED_UINT32[0], args);
                rw.RwObj(ref this.UNUSED_UINT32[1], args);
            }
        }
    }
}
