using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MSD_ : ISerializable
    {
        public const int DataSize = 64;

        public float[] Position = new float[3];
        public float[] Rotation = new float[3];

        public AnimationStruct WaitingAnimation = new AnimationStruct(loopBool:1);

        public Bitfield32 Flags = new Bitfield32();

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 4).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            if ((int)args["dataSize"] != 48 && (int)args["dataSize"] != 64)
                throw new Exception($"MSD_ command should have dataSize 48 or 64; instead has {(int)args["dataSize"]}");

            rw.RwFloat32s(ref this.Position, 3);
            rw.RwFloat32s(ref this.Rotation, 3);

            rw.RwUInt32(ref this.WaitingAnimation.Index);
            rw.RwUInt32(ref this.WaitingAnimation.InterpolatedFrames);
            rw.RwUInt32(ref this.WaitingAnimation.LoopBool);
            rw.RwFloat32(ref this.WaitingAnimation.PlaybackSpeed);

            rw.RwObj(ref this.Flags);

            rw.RwUInt32(ref this.WaitingAnimation.StartingFrame);

            if ((int)args["dataSize"] == 64)
                rw.RwObjs(ref this.UNUSED_UINT32, 4, args);
        }
    }
}
