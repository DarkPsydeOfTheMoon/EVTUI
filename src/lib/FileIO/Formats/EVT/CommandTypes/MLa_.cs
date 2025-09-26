using System;
using System.Collections.Generic;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MLa_ : ISerializable
    {
        public const int DataSize = 32;

        public UInt16 ResetEyeWhenMoving;
        public Bitfield16 Flags = new Bitfield16();

        public UInt16 MotionType;
        public UInt16 SpeedType = 2;

        public UInt16 TargetType;
        public float[] Target = new float[3];
        public UInt32 TargetModelID;
        public UInt32 TargetHelperID;

        public ConstUInt16 UNUSED_UINT16 = new ConstUInt16();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt16(ref this.ResetEyeWhenMoving);
            rw.RwObj(ref this.Flags);

            rw.RwUInt16(ref this.MotionType);
            rw.RwUInt16(ref this.SpeedType);

            rw.RwObj(ref this.UNUSED_UINT16, args);
            
            rw.RwUInt16(ref this.TargetType);
            rw.RwFloat32s(ref this.Target, 3);
            rw.RwUInt32(ref this.TargetModelID);
            rw.RwUInt32(ref this.TargetHelperID);
        }
    }
}
