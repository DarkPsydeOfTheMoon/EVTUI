using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class EnBc : ISerializable
    {
        public const int DataSize = 16;

        public UInt32 Unk = 4354;
        public UInt32 RGBA = 0x202020FF;

        public UInt32[] UNUSED_UINT32 = new UInt32[2];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.UNUSED_UINT32[0]);
            Trace.Assert(this.UNUSED_UINT32[0] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[0]}) in reserve variable.");

            rw.RwUInt32(ref this.Unk);

            rw.RwUInt32(ref this.UNUSED_UINT32[1]);
            Trace.Assert(this.UNUSED_UINT32[1] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[1]}) in reserve variable.");

            rw.RwUInt32(ref this.RGBA);
        }
    }
}
