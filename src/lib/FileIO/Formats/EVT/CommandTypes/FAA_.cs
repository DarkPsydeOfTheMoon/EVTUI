using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class FAA_ : ISerializable
    {
        public const int DataSize = 80;

        public UInt32 Unk = 174;
        public UInt32 ObjectIndex;

        public Bitfield32 Flags = new Bitfield32();

        public AnimationStruct FirstAnimation = new AnimationStruct(loopBool:0, endingFrame:0, weight:1.0F);
        public AnimationStruct SecondAnimation = new AnimationStruct(loopBool:0, endingFrame:0, weight:1.0F);

        public UInt32[] UNUSED_UINT32 = new UInt32[3];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.Unk);
            rw.RwUInt32(ref this.ObjectIndex);

            rw.RwObj(ref this.Flags);

            rw.RwUInt32(ref this.UNUSED_UINT32[0]);

            rw.RwUInt32(ref this.FirstAnimation.Index);
            rw.RwUInt32(ref this.FirstAnimation.StartingFrame);
            rw.RwUInt32(ref this.FirstAnimation.EndingFrame);
            rw.RwUInt32(ref this.FirstAnimation.InterpolatedFrames);
            rw.RwUInt32(ref this.FirstAnimation.LoopBool);
            rw.RwFloat32(ref this.FirstAnimation.Weight);
            rw.RwFloat32(ref this.FirstAnimation.PlaybackSpeed);

            rw.RwUInt32(ref this.UNUSED_UINT32[1]);

            rw.RwUInt32(ref this.SecondAnimation.Index);
            rw.RwUInt32(ref this.SecondAnimation.StartingFrame);
            rw.RwUInt32(ref this.SecondAnimation.EndingFrame);
            rw.RwUInt32(ref this.SecondAnimation.InterpolatedFrames);
            rw.RwUInt32(ref this.SecondAnimation.LoopBool);
            rw.RwFloat32(ref this.SecondAnimation.Weight);
            rw.RwFloat32(ref this.SecondAnimation.PlaybackSpeed);

            rw.RwUInt32(ref this.UNUSED_UINT32[2]);

            for (int i=1; i<this.UNUSED_UINT32.Length; i++)
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
        }
    }
}
