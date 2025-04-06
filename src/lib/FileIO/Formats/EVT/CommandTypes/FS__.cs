using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class FS__ : ISerializable
    {
        public const int DataSize = 32;

        public UInt32 UnkBool;

        public float[] Coordinates = new float[3];
        public float[] Rotation = new float[3];

        public float UnkFloat;

        public UInt32[] UNUSED_UINT32 = new UInt32[4];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.UNUSED_UINT32[0]);

            rw.RwUInt32(ref this.UnkBool);

            rw.RwUInt32(ref this.UNUSED_UINT32[1]);
            rw.RwUInt32(ref this.UNUSED_UINT32[2]);

            rw.RwFloat32s(ref this.Coordinates, 3);
            rw.RwFloat32s(ref this.Rotation, 3);

            rw.RwFloat32(ref this.UnkFloat);

            rw.RwUInt32(ref this.UNUSED_UINT32[3]);

            for (int i=0; i<this.UNUSED_UINT32.Length; i++)
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
        }
    }
}
