using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MRot : ISerializable
    {
        public UInt16  FrameDelay;
        public float[] Rotation = new float[3];
        public UInt32  FrameDuration;

        public UInt16[] Animation1Unks = new UInt16[4];
        public UInt32   Animation1LoopBool;
        public float    Animation1Speed;
        public Int32    Animation1Ind;

        public UInt16[] Animation2Unks = new UInt16[4];
        public UInt32   Animation2LoopBool;
        public float    Animation2Speed;
        public Int32    Animation2Ind;

        public byte[] UNK_UINT8 = new byte[2];
        public UInt32[] UNK_UINT32 = new UInt32[1];

        public UInt32[] UNUSED_UINT32 = new UInt32[8];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt8(ref this.UNK_UINT8[0]);
            rw.RwUInt16(ref this.FrameDelay);
            rw.RwUInt8(ref this.UNK_UINT8[1]);
            rw.RwFloat32s(ref this.Rotation, 3);
            rw.RwUInt32(ref this.UNK_UINT32[0]);
            rw.RwUInt32(ref this.FrameDuration);

            rw.RwUInt32(ref this.UNUSED_UINT32[0]);
            rw.RwUInt32(ref this.UNUSED_UINT32[1]);

            rw.RwUInt16s(ref this.Animation1Unks, 4);
            rw.RwUInt32(ref this.Animation1LoopBool);
            rw.RwFloat32(ref this.Animation1Speed);
            rw.RwInt32(ref this.Animation1Ind);

            rw.RwUInt32(ref this.UNUSED_UINT32[2]);
            rw.RwUInt32(ref this.UNUSED_UINT32[3]);
            rw.RwUInt32(ref this.UNUSED_UINT32[4]);

            rw.RwUInt16s(ref this.Animation2Unks, 4);
            rw.RwUInt32(ref this.Animation2LoopBool);
            rw.RwFloat32(ref this.Animation2Speed);
            rw.RwInt32(ref this.Animation2Ind);

            rw.RwUInt32(ref this.UNUSED_UINT32[5]);
            rw.RwUInt32(ref this.UNUSED_UINT32[6]);
            rw.RwUInt32(ref this.UNUSED_UINT32[7]);

            for (int i=0; i<this.UNUSED_UINT32.Length; i++)
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
        }
    }
}
