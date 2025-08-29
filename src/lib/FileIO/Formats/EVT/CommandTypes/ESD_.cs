using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class ESD_ : ISerializable
    {
        public const int DataSize = 32;

        public float[] Position = new float[3];
        public float[] Rotation = new float[3];

        public UInt32[] UNUSED_UINT32 = new UInt32[2];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.UNUSED_UINT32[0]);
            Trace.Assert(this.UNUSED_UINT32[0] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[0]}) in reserve variable.");

            rw.RwFloat32s(ref this.Position, 3);
            rw.RwFloat32s(ref this.Rotation, 3);

            rw.RwUInt32(ref this.UNUSED_UINT32[1]);
            Trace.Assert(this.UNUSED_UINT32[1] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[1]}) in reserve variable.");
        }
    }
}
