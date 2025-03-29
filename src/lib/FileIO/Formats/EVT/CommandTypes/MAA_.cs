using System;
using System.Collections.Generic;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MAA_ : ISerializable
    {
        public const int DataSize = 32;

        public Int32 TrackNumber;

        public AnimationStruct AddAnimation = new AnimationStruct(loopBool:0, weight:1.0F);

        public Bitfield32 Flags = new Bitfield32();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwInt32(ref this.TrackNumber);

            rw.RwUInt32(ref this.AddAnimation.Index);
            rw.RwUInt32(ref this.AddAnimation.InterpolatedFrames);
            rw.RwFloat32(ref this.AddAnimation.Weight);
            rw.RwUInt32(ref this.AddAnimation.LoopBool);
            rw.RwFloat32(ref this.AddAnimation.PlaybackSpeed);
            rw.RwUInt32(ref this.AddAnimation.StartingFrame);

            rw.RwObj(ref this.Flags);
        }
    }
}
