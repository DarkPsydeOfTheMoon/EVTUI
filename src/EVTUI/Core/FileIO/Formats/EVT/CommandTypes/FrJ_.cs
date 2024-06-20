using System;
using System.Collections.Generic;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class FrJ_ : ISerializable
    {
        public UInt32 JumpToFrame;

        public UInt32[] UNUSED_UINT32 = new UInt32[3];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.JumpToFrame);      // (should be shown as 0 through the EVT's total number of frames -- can jump forward or backward)
            rw.RwUInt32(ref this.UNUSED_UINT32[0]); // observed values: 0
            rw.RwUInt32(ref this.UNUSED_UINT32[1]); // observed values: 0
            rw.RwUInt32(ref this.UNUSED_UINT32[2]); // observed values: 0
        }
    }
}
