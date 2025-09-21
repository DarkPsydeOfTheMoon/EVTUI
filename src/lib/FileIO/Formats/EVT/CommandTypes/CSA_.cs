using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class CSA_ : ISerializable
    {
        public const int DataSize = 96;

        public Bitfield32 Flags = new Bitfield32(48);

        public Int32 AssetId;
        public UInt32 AnimationId;
        public float PlaybackSpeed = 1.0F;
        public UInt32 StartingFrame;

        public float[] ViewportCoordinates = new float[3];
        public float[] ViewportRotation = new float[3];

        public float FocalPlaneDistance;
        public float NearBlurSurface;
        public float FarBlurSurface;

        public float BlurStrength = 1.0F;
        public UInt32 BlurType;

        public UInt32 MessageCoordinateType = 4;
        public float[] MessageCoordinates = new[] { 375.0F, 565.0F };

        public byte UnkEnum;
        public byte UnkInd = 2;

        public ConstUInt16 UNUSED_UINT16 = new ConstUInt16();
        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 4).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.Flags);

            rw.RwInt32(ref this.AssetId);
            rw.RwUInt32(ref this.AnimationId);
            rw.RwFloat32(ref this.PlaybackSpeed);
            rw.RwUInt32(ref this.StartingFrame);

            rw.RwFloat32s(ref this.ViewportCoordinates, 3);
            rw.RwFloat32s(ref this.ViewportRotation, 3);

            rw.RwObj(ref this.UNUSED_UINT32[0], args);

            if ((int)args["dataSize"] > 48)
            {
                rw.RwFloat32(ref this.FocalPlaneDistance);
                rw.RwFloat32(ref this.NearBlurSurface);
                rw.RwFloat32(ref this.FarBlurSurface);

                rw.RwFloat32(ref this.BlurStrength);
                rw.RwUInt32(ref this.BlurType);

                rw.RwUInt32(ref this.MessageCoordinateType);
                rw.RwFloat32s(ref this.MessageCoordinates, 2);

                if ((int)args["dataSize"] > 80)
                {
                    rw.RwUInt8(ref this.UnkEnum);
                    rw.RwUInt8(ref this.UnkInd);

                    rw.RwObj(ref this.UNUSED_UINT16, args);
                    rw.RwObj(ref this.UNUSED_UINT32[1], args);
                    rw.RwObj(ref this.UNUSED_UINT32[2], args);
                    rw.RwObj(ref this.UNUSED_UINT32[3], args);
                }
            }
        }
    }
}
