using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class CAR_ : ISerializable
    {
        public const int DataSize = 16;

        public Bitfield32 Flags = new Bitfield32(3);

        public UInt32[] UNUSED_UINT32 = new UInt32[3];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.Flags);

            for (int i=0; i<this.UNUSED_UINT32.Length; i++)
            {
                rw.RwUInt32(ref this.UNUSED_UINT32[i]);
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
            }
        }
    }
}
