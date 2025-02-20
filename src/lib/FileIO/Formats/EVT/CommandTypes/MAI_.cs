using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MAI_ : ISerializable
    {
        public const int DataSize = 256;

        private UInt32[] _bitfield = new UInt32[] {5, 4, 4, 4, 4, 4, 4, 4, 4, 4};
        public Bitfield[] IdleAnimationBitfields = Enumerable.Range(0, 10).Select(i => new Bitfield((uint)((i == 0) ? 5 : 4))).ToArray();

        public AnimationStruct[] IdleAnimations = Enumerable.Range(0, 10).Select(_ => new AnimationStruct(endingFrame:0)).ToArray();

        public UInt32[] UNUSED_UINT32 = new UInt32[4];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.UNUSED_UINT32[0]);

            for (int i=0; i<10; i++)
            {
                if (rw.IsParselike())
                    this._bitfield[i] = this.IdleAnimationBitfields[i].Compose();
                rw.RwUInt32(ref this._bitfield[i]);
                this.IdleAnimationBitfields[i] = new Bitfield(this._bitfield[i]);
                rw.RwUInt32(ref this.IdleAnimations[i].Index);
                rw.RwUInt32(ref this.IdleAnimations[i].StartingFrame);
                rw.RwUInt32(ref this.IdleAnimations[i].EndingFrame);
                rw.RwUInt32(ref this.IdleAnimations[i].InterpolatedFrames);
                rw.RwFloat32(ref this.IdleAnimations[i].PlaybackSpeed);
            }

            rw.RwUInt32(ref this.UNUSED_UINT32[1]);
            rw.RwUInt32(ref this.UNUSED_UINT32[2]);
            rw.RwUInt32(ref this.UNUSED_UINT32[3]);

            for (int i=0; i<this.UNUSED_UINT32.Length; i++)
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
        }
    }
}
