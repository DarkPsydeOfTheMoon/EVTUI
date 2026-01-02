using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MRot : ISerializable
    {
        public const int DataSize = 96;

        public Bitfield32 Flags = new Bitfield32(1);
        public float[] Rotation = new float[3];
        public UInt32  InterpolationParameters;
        public UInt32  UNK;

        public AnimationStruct RotatingAnimation = new AnimationStruct(loopBool:0);
        public AnimationStruct WaitingAnimation = new AnimationStruct(loopBool:1);

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 8).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.Flags);
            rw.RwFloat32s(ref this.Rotation, 3);
            rw.RwUInt32(ref this.InterpolationParameters);
            rw.RwUInt32(ref this.UNK);

            rw.RwObj(ref this.UNUSED_UINT32[0], args);
            rw.RwObj(ref this.UNUSED_UINT32[1], args);

            rw.RwUInt32(ref this.RotatingAnimation.Index);
            rw.RwUInt32(ref this.RotatingAnimation.InterpolatedFrames);
            rw.RwUInt32(ref this.RotatingAnimation.LoopBool);
            rw.RwFloat32(ref this.RotatingAnimation.PlaybackSpeed);
            rw.RwUInt32(ref this.RotatingAnimation.StartingFrame);

            rw.RwObj(ref this.UNUSED_UINT32[2], args);
            rw.RwObj(ref this.UNUSED_UINT32[3], args);
            rw.RwObj(ref this.UNUSED_UINT32[4], args);

            rw.RwUInt32(ref this.WaitingAnimation.Index);
            rw.RwUInt32(ref this.WaitingAnimation.InterpolatedFrames);
            rw.RwUInt32(ref this.WaitingAnimation.LoopBool);
            rw.RwFloat32(ref this.WaitingAnimation.PlaybackSpeed);
            rw.RwUInt32(ref this.WaitingAnimation.StartingFrame);

            rw.RwObj(ref this.UNUSED_UINT32[5], args);
            rw.RwObj(ref this.UNUSED_UINT32[6], args);
            rw.RwObj(ref this.UNUSED_UINT32[7], args);
        }
    }
}
