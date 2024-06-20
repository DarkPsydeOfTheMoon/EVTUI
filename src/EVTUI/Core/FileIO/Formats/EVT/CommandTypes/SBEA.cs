using System;
using System.Collections.Generic;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class SBEA : ISerializable
    {
        public UInt32 UnkEnum;

        public UInt32[] UNUSED_UINT32 = new UInt32[3];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.UNUSED_UINT32[0]); // observed values: 0
            rw.RwUInt32(ref this.UnkEnum);          // observed values: 1, 2
            rw.RwUInt32(ref this.UNUSED_UINT32[1]); // observed values: 0
            rw.RwUInt32(ref this.UNUSED_UINT32[2]); // observed values: 0
        }
    }
}
