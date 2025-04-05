using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class CClp : ISerializable
    {
        public const int DataSize = 16;

        public float NearClip = 1.0F;
        public float FarClip  = 60000.0F;

        public UInt32[] UNUSED_UINT32 = new UInt32[2];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            for (int i=0; i<this.UNUSED_UINT32.Length; i++)
            {
                rw.RwUInt32(ref this.UNUSED_UINT32[i]);
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
            }

            rw.RwFloat32(ref this.NearClip);
            rw.RwFloat32(ref this.FarClip);
        }
    }
}
