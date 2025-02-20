using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MAB_ : ISerializable
    {
        public const int DataSize = 64;

        public AnimationStruct FirstAnimation = new AnimationStruct(loopBool:0, endingFrame:0);
        public UInt16 FirstAnimationUnkFrames;
        public UInt16 FirstAnimationInterpolatedFrames;

        public AnimationStruct SecondAnimation = new AnimationStruct(loopBool:1, endingFrame:0);
        public UInt16 SecondAnimationUnkFrames;
        public UInt16 SecondAnimationInterpolatedFrames;

        private UInt32 _bitfield;
        public Bitfield Flags = new Bitfield(0);

        public UInt32 StartWaitingFrames;

        public UInt32[] UNUSED_UINT32 = new UInt32[2];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.FirstAnimation.Index);

            // horrible shenanigans bc idk what the fuck is going on with this part
            if (rw.IsParselike())
                this.FirstAnimationInterpolatedFrames = (UInt16)this.FirstAnimation.InterpolatedFrames;
            rw.RwUInt16(ref this.FirstAnimationUnkFrames);
            rw.RwUInt16(ref this.FirstAnimationInterpolatedFrames);
            this.FirstAnimation.InterpolatedFrames = (UInt32)this.FirstAnimationInterpolatedFrames;

            rw.RwUInt32(ref this.FirstAnimation.LoopBool);
            rw.RwFloat32(ref this.FirstAnimation.PlaybackSpeed);

            rw.RwUInt32(ref this.SecondAnimation.Index);

            // horrible shenanigans bc idk what the fuck is going on with this part
            if (rw.IsParselike())
                this.SecondAnimationInterpolatedFrames = (UInt16)this.SecondAnimation.InterpolatedFrames;
            rw.RwUInt16(ref this.SecondAnimationUnkFrames);
            rw.RwUInt16(ref this.SecondAnimationInterpolatedFrames);
            this.SecondAnimation.InterpolatedFrames = (UInt32)this.SecondAnimationInterpolatedFrames;

            rw.RwUInt32(ref this.SecondAnimation.LoopBool);
            rw.RwFloat32(ref this.SecondAnimation.PlaybackSpeed);

            if (rw.IsParselike())
                this._bitfield = this.Flags.Compose();
            rw.RwUInt32(ref this._bitfield);
            this.Flags = new Bitfield(this._bitfield);

            rw.RwUInt32(ref this.FirstAnimation.StartingFrame);
            rw.RwUInt32(ref this.FirstAnimation.EndingFrame);

            rw.RwUInt32(ref this.SecondAnimation.StartingFrame);
            rw.RwUInt32(ref this.SecondAnimation.EndingFrame);

            rw.RwUInt32(ref this.StartWaitingFrames);

            for (int i=0; i<2; i++)
            {
                rw.RwUInt32(ref this.UNUSED_UINT32[i]);
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
            }
        }
    }
}
