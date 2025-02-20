using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MRot : ISerializable
    {
        public const int DataSize = 96;

        private UInt32 _bitfield;
        public Bitfield Flags = new Bitfield(1);

        public float[] Rotation = new float[3];
        public UInt32  InterpolationType;
        public UInt32  UNK;

        public AnimationStruct RotatingAnimation = new AnimationStruct(loopBool:0);
        public AnimationStruct WaitingAnimation = new AnimationStruct(loopBool:1);

        public UInt32[] UNUSED_UINT32 = new UInt32[8];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            if (rw.IsParselike())
                this._bitfield = this.Flags.Compose();
            rw.RwUInt32(ref this._bitfield);
            this.Flags = new Bitfield(this._bitfield);
            rw.RwFloat32s(ref this.Rotation, 3);

            rw.RwUInt32(ref this.InterpolationType);
            rw.RwUInt32(ref this.UNK);

            rw.RwUInt32(ref this.UNUSED_UINT32[0]);
            rw.RwUInt32(ref this.UNUSED_UINT32[1]);

            rw.RwUInt32(ref this.RotatingAnimation.Index);
            rw.RwUInt32(ref this.RotatingAnimation.InterpolatedFrames);
            rw.RwUInt32(ref this.RotatingAnimation.LoopBool);
            rw.RwFloat32(ref this.RotatingAnimation.PlaybackSpeed);
            rw.RwUInt32(ref this.RotatingAnimation.StartingFrame);

            rw.RwUInt32(ref this.UNUSED_UINT32[2]);
            rw.RwUInt32(ref this.UNUSED_UINT32[3]);
            rw.RwUInt32(ref this.UNUSED_UINT32[4]);

            rw.RwUInt32(ref this.WaitingAnimation.Index);
            rw.RwUInt32(ref this.WaitingAnimation.InterpolatedFrames);
            rw.RwUInt32(ref this.WaitingAnimation.LoopBool);
            rw.RwFloat32(ref this.WaitingAnimation.PlaybackSpeed);
            rw.RwUInt32(ref this.WaitingAnimation.StartingFrame);

            rw.RwUInt32(ref this.UNUSED_UINT32[5]);
            rw.RwUInt32(ref this.UNUSED_UINT32[6]);
            rw.RwUInt32(ref this.UNUSED_UINT32[7]);

            for (int i=0; i<this.UNUSED_UINT32.Length; i++)
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
        }
    }
}
