using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class CSD_ : ISerializable
    {
        public const int DataSize = 80;

        public Bitfield32 Flags = new Bitfield32(16);

        public float[] ViewportCoordinates = new float[3];
        public float[] ViewportRotation = new float[3];
        public float AngleOfView = 45.0F;

        public float FocalPlaneDistance;
        public float NearBlurSurface;
        public float FarBlurSurface;

        public float BlurStrength = 1.0F;
        public UInt32 BlurType;

        public UInt32 MessageCoordinateType = 4;
        public float[] MessageCoordinates = new[] { 375.0F, 528.0F };

        public byte UnkEnum;
        public byte UnkInd = 2;
        public byte SuperUnk1;
        public byte SuperUnk2;
        public float[] UnkCoordinates = new float[3];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.Flags);

            rw.RwFloat32s(ref this.ViewportCoordinates, 3);
            rw.RwFloat32s(ref this.ViewportRotation, 3);
            rw.RwFloat32(ref this.AngleOfView);

            if ((int)args["dataSize"] > 32)
            {
                rw.RwFloat32(ref this.FocalPlaneDistance);
                rw.RwFloat32(ref this.NearBlurSurface);
                rw.RwFloat32(ref this.FarBlurSurface);
                rw.RwFloat32(ref this.BlurStrength);

                if ((int)args["dataSize"] > 48)
                {
                    rw.RwUInt32(ref this.BlurType);
                    rw.RwUInt32(ref this.MessageCoordinateType);
                    rw.RwFloat32s(ref this.MessageCoordinates, 2);

                    if ((int)args["dataSize"] > 64)
                    {
                        rw.RwUInt8(ref this.UnkEnum);
                        rw.RwUInt8(ref this.UnkInd);
                        rw.RwUInt8(ref this.SuperUnk1);
                        rw.RwUInt8(ref this.SuperUnk2);
                        rw.RwFloat32s(ref this.UnkCoordinates, 3);
                    }
                }
            }
        }
    }
}
