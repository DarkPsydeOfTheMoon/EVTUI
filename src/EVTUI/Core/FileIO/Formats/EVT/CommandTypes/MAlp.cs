using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MAlp : ISerializable
    {
        public byte AlphaLevel;

        public byte[] UNK_BYTE = new byte[11];

        public UInt32[] UNUSED_UINT32 = new UInt32[1];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.UNUSED_UINT32[0]);
            Trace.Assert(this.UNUSED_UINT32[0] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[0]}) in MAlp reserve variable.");

            for (int i=0; i<3; i++)
                rw.RwUInt8(ref this.UNK_BYTE[0]);

            rw.RwUInt8(ref this.AlphaLevel);

            // TODO: seems like a bunch of bitflags but i have nooooo clue what they're doing
            for (int i=3; i<11; i++)
                rw.RwUInt8(ref this.UNK_BYTE[i]);
        }
    }
}
