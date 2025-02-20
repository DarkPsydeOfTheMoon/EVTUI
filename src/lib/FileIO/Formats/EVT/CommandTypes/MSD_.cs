using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MSD_ : ISerializable
    {
        public const int DataSize = 64;

        public float[] Position = new float[3];
        public float[] Rotation = new float[3];

        public AnimationStruct WaitingAnimation = new AnimationStruct(loopBool:1);

        public UInt32 _bitfield;
        public Bitfield Flags = new Bitfield(0);

        public UInt32[] UNUSED_UINT32 = new UInt32[4];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            Trace.Assert((int)args["dataSize"] == 48 || (int)args["dataSize"] == 64);

            rw.RwFloat32s(ref this.Position, 3);
            rw.RwFloat32s(ref this.Rotation, 3);

            rw.RwUInt32(ref this.WaitingAnimation.Index);
            rw.RwUInt32(ref this.WaitingAnimation.InterpolatedFrames);
            rw.RwUInt32(ref this.WaitingAnimation.LoopBool);
            rw.RwFloat32(ref this.WaitingAnimation.PlaybackSpeed);

            if (rw.IsParselike())
                this._bitfield = this.Flags.Compose();
            rw.RwUInt32(ref this._bitfield);
            this.Flags = new Bitfield(this._bitfield);

            rw.RwUInt32(ref this.WaitingAnimation.StartingFrame);

            if ((int)args["dataSize"] == 64)
                for (var i=0; i<4; i++)
                {
                    rw.RwUInt32(ref this.UNUSED_UINT32[i]); // observed values: 0
                    Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in MSD_ reserve variable.");
                }
        }
    }
}
