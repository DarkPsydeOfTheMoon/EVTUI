using System;
using System.Collections.Generic;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MLw_ : ISerializable
    {
        public const int DataSize = 32;

        public UInt32 EnableUpperBodyRotation = 1;

        public float TopLimitAngle = 5.0F;
        public float BottomLimitAngle = 5.0F;
        public float LeftLimitAngle = 15.0F;
        public float RightLimitAngle = 15.0F;

        public UInt32 UpdateIntervalMinimumFrameValue = 150;
        public UInt32 UpdateIntervalRandomFrame = 60;

        public ConstUInt32 UNUSED_UINT32 = new ConstUInt32();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.EnableUpperBodyRotation);

            rw.RwFloat32(ref this.TopLimitAngle);
            rw.RwFloat32(ref this.BottomLimitAngle);
            rw.RwFloat32(ref this.LeftLimitAngle);
            rw.RwFloat32(ref this.RightLimitAngle);

            rw.RwUInt32(ref this.UpdateIntervalMinimumFrameValue);
            rw.RwUInt32(ref this.UpdateIntervalRandomFrame);

            rw.RwObj(ref this.UNUSED_UINT32, args);
        }
    }
}
