using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MLa_ : ISerializable
    {
        public const int DataSize = 32;

        public UInt16 ResetEyeWhenMoving;
        public UInt16 Bitfield;
        public UInt16 MotionType;
        public UInt16 SpeedType = 2;
        public UInt16 UNUSED;
        public UInt16 TargetType;
        public float[] Target = new float[3];
        public UInt32 TargetModelID;
        public UInt32 TargetBoneID;

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt16(ref this.ResetEyeWhenMoving);
            rw.RwUInt16(ref this.Bitfield);
            rw.RwUInt16(ref this.MotionType);
            rw.RwUInt16(ref this.SpeedType);

            rw.RwUInt16(ref this.UNUSED);
            Trace.Assert(this.UNUSED == 0, $"Unexpected nonzero value ({this.UNUSED}) in reserve variable.");
            
            rw.RwUInt16(ref this.TargetType);
            rw.RwFloat32s(ref this.Target, 3);
            rw.RwUInt32(ref this.TargetModelID);
            rw.RwUInt32(ref this.TargetBoneID);
        }
    }
}
