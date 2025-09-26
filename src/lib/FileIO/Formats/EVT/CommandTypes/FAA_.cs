using System;
using System.Collections.Generic;
using System.Linq;

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

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 3).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.Unk);
            rw.RwUInt32(ref this.ObjectIndex);

            rw.RwObj(ref this.Flags);

            rw.RwObj(ref this.UNUSED_UINT32[0], args);

            rw.RwUInt32(ref this.FirstAnimation.Index);
            rw.RwUInt32(ref this.FirstAnimation.StartingFrame);
            rw.RwUInt32(ref this.FirstAnimation.EndingFrame);
            rw.RwUInt32(ref this.FirstAnimation.InterpolatedFrames);
            rw.RwUInt32(ref this.FirstAnimation.LoopBool);
            rw.RwFloat32(ref this.FirstAnimation.Weight);
            rw.RwFloat32(ref this.FirstAnimation.PlaybackSpeed);

            rw.RwObj(ref this.UNUSED_UINT32[1], args);

            rw.RwUInt32(ref this.SecondAnimation.Index);
            rw.RwUInt32(ref this.SecondAnimation.StartingFrame);
            rw.RwUInt32(ref this.SecondAnimation.EndingFrame);
            rw.RwUInt32(ref this.SecondAnimation.InterpolatedFrames);
            rw.RwUInt32(ref this.SecondAnimation.LoopBool);
            rw.RwFloat32(ref this.SecondAnimation.Weight);
            rw.RwFloat32(ref this.SecondAnimation.PlaybackSpeed);

            rw.RwObj(ref this.UNUSED_UINT32[2], args);
        }
    }
}
