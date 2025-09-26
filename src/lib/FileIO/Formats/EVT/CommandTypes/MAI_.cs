using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MAI_ : ISerializable
    {
        public const int DataSize = 256;

        public Bitfield32[] IdleAnimationBitfields = Enumerable.Range(0, 10).Select(i => new Bitfield32((uint)((i == 0) ? 5 : 4))).ToArray();

        public AnimationStruct[] IdleAnimations = Enumerable.Range(0, 10).Select(_ => new AnimationStruct(endingFrame:0)).ToArray();

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 4).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.UNUSED_UINT32[0], args);

            for (int i=0; i<10; i++)
            {
                rw.RwObj(ref this.IdleAnimationBitfields[i]);
                rw.RwUInt32(ref this.IdleAnimations[i].Index);
                rw.RwUInt32(ref this.IdleAnimations[i].StartingFrame);
                rw.RwUInt32(ref this.IdleAnimations[i].EndingFrame);
                rw.RwUInt32(ref this.IdleAnimations[i].InterpolatedFrames);
                rw.RwFloat32(ref this.IdleAnimations[i].PlaybackSpeed);
            }

            rw.RwObj(ref this.UNUSED_UINT32[1], args);
            rw.RwObj(ref this.UNUSED_UINT32[2], args);
            rw.RwObj(ref this.UNUSED_UINT32[3], args);
        }
    }
}
