using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MMD_ : ISerializable
    {
        public const int DataSize = 384;

        public UInt32 InterpolationType;
        public UInt32 NumControlGroups = 1;
        public float[,] Targets = new float[24,3];

        public Bitfield32 Flags = new Bitfield32();

        public float MovementSpeed = 1.0F;
        public UInt32 UNK;

        public byte StartSpeedType;
        public byte FinalSpeedType;

        public AnimationStruct MovingAnimation = new AnimationStruct(loopBool:1);
        public AnimationStruct WaitingAnimation = new AnimationStruct(loopBool:1);

        public ConstUInt16   UNUSED_UINT16 = new ConstUInt16();
        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 8).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            if ((int)args["dataSize"] != 96 && (int)args["dataSize"] != 384)
                throw new Exception($"MMD_ command should have dataSize 96 or 384; instead has {(int)args["dataSize"]}");

            rw.RwUInt32(ref this.InterpolationType);
            rw.RwUInt32(ref this.NumControlGroups);

            // where does the position get stored if not here, you ask? lmao idk. ancient evts...
            if ((int)args["dataSize"] == 384)
                for (int i=0; i<24; i++)
                    for (int j=0; j<3; j++)
                        rw.RwFloat32(ref this.Targets[i,j]);

            rw.RwObj(ref this.Flags);
            rw.RwFloat32(ref this.MovementSpeed);
            rw.RwUInt32(ref this.UNK);
            rw.RwUInt8(ref this.StartSpeedType);
            rw.RwUInt8(ref this.FinalSpeedType);

            rw.RwObj(ref this.UNUSED_UINT16, args);

            rw.RwUInt32(ref this.MovingAnimation.Index);
            rw.RwUInt32(ref this.MovingAnimation.InterpolatedFrames);
            rw.RwUInt32(ref this.MovingAnimation.LoopBool);
            rw.RwFloat32(ref this.MovingAnimation.PlaybackSpeed);
            rw.RwUInt32(ref this.MovingAnimation.StartingFrame);

            rw.RwObj(ref this.UNUSED_UINT32[0], args);
            rw.RwObj(ref this.UNUSED_UINT32[1], args);
            rw.RwObj(ref this.UNUSED_UINT32[2], args);

            rw.RwUInt32(ref this.WaitingAnimation.Index);
            rw.RwUInt32(ref this.WaitingAnimation.InterpolatedFrames);
            rw.RwUInt32(ref this.WaitingAnimation.LoopBool);
            rw.RwFloat32(ref this.WaitingAnimation.PlaybackSpeed);
            rw.RwUInt32(ref this.WaitingAnimation.StartingFrame);

            rw.RwObj(ref this.UNUSED_UINT32[3], args);
            rw.RwObj(ref this.UNUSED_UINT32[4], args);
            rw.RwObj(ref this.UNUSED_UINT32[5], args);
            rw.RwObj(ref this.UNUSED_UINT32[6], args);
            rw.RwObj(ref this.UNUSED_UINT32[7], args);
        }
    }
}
