using System;
using System.Collections.Generic;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class FrJ_ : ISerializable
    {
        public Int32 JumpToFrame;

        public Int32[] UNUSED_INT32 = new Int32[3];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwInt32(ref this.JumpToFrame);     // (should be shown as 0 through the EVT's total number of frames -- can jump forward or backward)
            rw.RwInt32(ref this.UNUSED_INT32[0]); // observed values: 0
            rw.RwInt32(ref this.UNUSED_INT32[1]); // observed values: 0
            rw.RwInt32(ref this.UNUSED_INT32[2]); // observed values: 0
        }
    }
}
