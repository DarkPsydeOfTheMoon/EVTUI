using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MMD_ : ISerializable
    {
        public const int DataSize = 384;

        public Int32 InterpolationType;
        public Int32 NumControlGroups;
        public float[][] Targets = new float[24][];
        public Int32 Bitflag;
        public float MovementSpeed;
        public Int32 UNK;

        public byte StartSpeedType;
        public byte FinalSpeedType;

        public Int32 MovingAnimationID;
        public Int32 MovingAnimationInterpolatedFrames;
        public Int32 MovingAnimationLoopBool;
        public float MovingAnimationPlaybackSpeed;
        public Int32 MovingAnimationStartingFrame;

        public Int32 WaitingAnimationID;
        public Int32 WaitingAnimationInterpolatedFrames;
        public Int32 WaitingAnimationLoopBool;
        public float WaitingAnimationPlaybackSpeed;
        public Int32 WaitingAnimationStartingFrame;

        public Int16 UNUSED_INT16;
        public Int32[] UNUSED_INT32 = new Int32[8];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwInt32(ref this.InterpolationType);
            rw.RwInt32(ref this.NumControlGroups);

            for (int i=0; i<24; i++)
                rw.RwFloat32s(ref this.Targets[i], 3);

            rw.RwInt32(ref this.Bitflag);
            rw.RwFloat32(ref this.MovementSpeed);
            rw.RwInt32(ref this.UNK);

            rw.RwUInt8(ref this.StartSpeedType);
            rw.RwUInt8(ref this.FinalSpeedType);

            rw.RwInt16(ref this.UNUSED_INT16);
            Trace.Assert(this.UNUSED_INT16 == 0, $"Unexpected nonzero value ({this.UNUSED_INT16}) in reserve variable.");

            rw.RwInt32(ref this.MovingAnimationID);
            rw.RwInt32(ref this.MovingAnimationInterpolatedFrames);
            rw.RwInt32(ref this.MovingAnimationLoopBool);
            rw.RwFloat32(ref this.MovingAnimationPlaybackSpeed);
            rw.RwInt32(ref this.MovingAnimationStartingFrame);

            rw.RwInt32(ref this.UNUSED_INT32[0]);
            rw.RwInt32(ref this.UNUSED_INT32[1]);
            rw.RwInt32(ref this.UNUSED_INT32[2]);

            rw.RwInt32(ref this.WaitingAnimationID);
            rw.RwInt32(ref this.WaitingAnimationInterpolatedFrames);
            rw.RwInt32(ref this.WaitingAnimationLoopBool);
            rw.RwFloat32(ref this.WaitingAnimationPlaybackSpeed);
            rw.RwInt32(ref this.WaitingAnimationStartingFrame);

            rw.RwInt32(ref this.UNUSED_INT32[3]);
            rw.RwInt32(ref this.UNUSED_INT32[4]);
            rw.RwInt32(ref this.UNUSED_INT32[5]);
            rw.RwInt32(ref this.UNUSED_INT32[6]);
            rw.RwInt32(ref this.UNUSED_INT32[7]);

            for (int i=0; i<this.UNUSED_INT32.Length; i++)
                Trace.Assert(this.UNUSED_INT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_INT32[i]}) in reserve variable.");
        }
    }
}
