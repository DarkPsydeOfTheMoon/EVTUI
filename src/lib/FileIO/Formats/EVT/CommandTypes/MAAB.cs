using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MAAB : ISerializable
    {
        public const int DataSize = 80;

        private UInt32 _bitfield;
        public Bitfield Flags = new Bitfield(0);

        public Int32 ChildObjectId;

        public AnimationStruct FirstAnimation = new AnimationStruct(loopBool:0, endingFrame:0);
        public AnimationStruct SecondAnimation = new AnimationStruct(loopBool:1, endingFrame:0);

        public UInt32[] UNUSED_UINT32 = new UInt32[6];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            if (rw.IsParselike())
                this._bitfield = this.Flags.Compose();
            rw.RwUInt32(ref this._bitfield);
            this.Flags = new Bitfield(this._bitfield);

            rw.RwInt32(ref this.ChildObjectId);

            rw.RwUInt32(ref this.UNUSED_UINT32[0]);
            rw.RwUInt32(ref this.UNUSED_UINT32[1]);

            rw.RwUInt32(ref this.FirstAnimation.Index);
            rw.RwUInt32(ref this.FirstAnimation.InterpolatedFrames);
            rw.RwUInt32(ref this.FirstAnimation.LoopBool);
            rw.RwFloat32(ref this.FirstAnimation.PlaybackSpeed);
            rw.RwUInt32(ref this.FirstAnimation.StartingFrame);
            rw.RwUInt32(ref this.FirstAnimation.EndingFrame);

            rw.RwUInt32(ref this.UNUSED_UINT32[2]);
            rw.RwUInt32(ref this.UNUSED_UINT32[3]);

            rw.RwUInt32(ref this.SecondAnimation.Index);
            rw.RwUInt32(ref this.SecondAnimation.InterpolatedFrames);
            rw.RwUInt32(ref this.SecondAnimation.LoopBool);
            rw.RwFloat32(ref this.SecondAnimation.PlaybackSpeed);
            rw.RwUInt32(ref this.SecondAnimation.StartingFrame);
            rw.RwUInt32(ref this.SecondAnimation.EndingFrame);

            rw.RwUInt32(ref this.UNUSED_UINT32[4]);
            rw.RwUInt32(ref this.UNUSED_UINT32[5]);

            for (int i=0; i<this.UNUSED_UINT32.Length; i++)
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
        }
    }
}
