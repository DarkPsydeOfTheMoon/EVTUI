using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class CSDD : ISerializable
    {
        public const int DataSize = 48;

        public Bitfield32 Flags = new Bitfield32(2);

        public float[] ViewportCoordinates = new float[3];
        public float[] ViewportRotation = new float[3];
        public float AngleOfView = 45.0F;

        public float FocalPlaneDistance;
        public float NearBlurSurface;
        public float FarBlurSurface;
        public float BlurStrength = 1.0F;

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.Flags);

            rw.RwFloat32s(ref this.ViewportCoordinates, 3);
            rw.RwFloat32s(ref this.ViewportRotation, 3);
            rw.RwFloat32(ref this.AngleOfView);

            rw.RwFloat32(ref this.FocalPlaneDistance);
            rw.RwFloat32(ref this.NearBlurSurface);
            rw.RwFloat32(ref this.FarBlurSurface);
            rw.RwFloat32(ref this.BlurStrength);
        }
    }
}
