using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class CMC_ : ISerializable
    {
        public const int DataSize = 64;

        public Bitfield32 Flags = new Bitfield32();

        public UInt32 InterpolationParameters = 4354;
        public UInt16 StartCorrectionFrameNumber;
        public Int32 AssetId;
        public UInt32 ShotType;
        public UInt32 AngleType;

        public float FocalPlaneDistance;
        public float NearBlurSurface;
        public float FarBlurSurface;
        public float BlurStrength = 1.0F;

        public UInt32 BlurType;
        public UInt32 MessageCoordinateType = 4;
        public float[] MessageCoordinates = new[] { 375.0F, 528.0F };

        public UInt16[] UNUSED_UINT16 = new UInt16[1];
        public UInt32[] UNUSED_UINT32 = new UInt32[2];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.Flags);

            rw.RwUInt32(ref this.InterpolationParameters);
            rw.RwUInt16(ref this.StartCorrectionFrameNumber);

            rw.RwUInt16(ref this.UNUSED_UINT16[0]);
            Trace.Assert(this.UNUSED_UINT16[0] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT16[0]}) in reserve variable.");

            rw.RwUInt32(ref this.UNUSED_UINT32[0]);

            rw.RwInt32(ref this.AssetId);
            rw.RwUInt32(ref this.ShotType);

            rw.RwUInt32(ref this.UNUSED_UINT32[0]);

            rw.RwUInt32(ref this.AngleType);

            rw.RwFloat32(ref this.FocalPlaneDistance);
            rw.RwFloat32(ref this.NearBlurSurface);
            rw.RwFloat32(ref this.FarBlurSurface);
            rw.RwFloat32(ref this.BlurStrength);

            if ((int)args["dataSize"] > 48)
            {
                rw.RwUInt32(ref this.BlurType);
                rw.RwUInt32(ref this.MessageCoordinateType);
                rw.RwFloat32s(ref this.MessageCoordinates, 2);
            }

            for (int i=0; i<this.UNUSED_UINT32.Length; i++)
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
        }
    }
}
