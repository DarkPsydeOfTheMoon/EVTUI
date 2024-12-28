using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MAt_ : ISerializable
    {
        public const int DataSize = 48;

        public Int32 FrameDelay;
        public Int32 BoneId;
        public Int32 ChildObjectId;

        public float[] RelativePosition = new float[3];
        public float[] Rotation = new float[3];

        public UInt32[] UNUSED_UINT32 = new UInt32[3];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwInt32(ref this.FrameDelay);
            rw.RwInt32(ref this.BoneId);
            rw.RwInt32(ref this.ChildObjectId);

            rw.RwUInt32(ref this.UNUSED_UINT32[0]);

            rw.RwFloat32s(ref this.RelativePosition, 3);
            rw.RwFloat32s(ref this.Rotation, 3);

            rw.RwUInt32(ref this.UNUSED_UINT32[1]);
            rw.RwUInt32(ref this.UNUSED_UINT32[2]);

            for (int i=0; i<this.UNUSED_UINT32.Length; i++)
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in MAt_ reserve variable.");
        }
    }
}
