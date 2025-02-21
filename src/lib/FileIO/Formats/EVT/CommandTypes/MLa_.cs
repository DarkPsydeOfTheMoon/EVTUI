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

        private UInt16 _bitfield;
        public Bitfield Flags = new Bitfield(0);

        public UInt16 MotionType;
        public UInt16 SpeedType = 2;
        public UInt16 UNUSED;
        public UInt16 TargetType;
        public float[] Target = new float[3];
        public UInt32 TargetModelID;
        public UInt32 TargetHelperID;

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt16(ref this.ResetEyeWhenMoving);

            if (rw.IsParselike())
                this._bitfield = (UInt16)this.Flags.Compose();
            rw.RwUInt16(ref this._bitfield);
            this.Flags = new Bitfield((UInt32)this._bitfield);

            rw.RwUInt16(ref this.MotionType);
            rw.RwUInt16(ref this.SpeedType);

            rw.RwUInt16(ref this.UNUSED);
            Trace.Assert(this.UNUSED == 0, $"Unexpected nonzero value ({this.UNUSED}) in reserve variable.");
            
            rw.RwUInt16(ref this.TargetType);
            rw.RwFloat32s(ref this.Target, 3);
            rw.RwUInt32(ref this.TargetModelID);
            rw.RwUInt32(ref this.TargetHelperID);
        }
    }
}
