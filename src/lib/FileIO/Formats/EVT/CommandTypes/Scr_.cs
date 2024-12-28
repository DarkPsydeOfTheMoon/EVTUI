using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class Scr_ : ISerializable
    {
        public const int DataSize = 16;

        public Int32 ProcedureIndex;

        public Int32[] UNUSED_INT32 = new Int32[3];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwInt32(ref this.UNUSED_INT32[0]);
            rw.RwInt32(ref this.ProcedureIndex); // (should be shown as 0 through the BF file's number of procedures)
            rw.RwInt32(ref this.UNUSED_INT32[1]);
            rw.RwInt32(ref this.UNUSED_INT32[2]);
            for (int i=0; i<this.UNUSED_INT32.Length; i++)
                Trace.Assert(this.UNUSED_INT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_INT32[i]}) in reserve variable.");
        }
    }
}
