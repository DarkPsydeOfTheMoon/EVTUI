using System;
using System.Collections.Generic;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class SFts : ISerializable
    {
        public UInt32 Enable;
        public UInt32 ObjectId;

        public UInt32[] UNUSED_UINT32 = new UInt32[2];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.Enable);           // observed values: 0, 1 (a boolean)
            rw.RwUInt32(ref this.ObjectId);
            rw.RwUInt32(ref this.UNUSED_UINT32[0]); // observed values: 0
            rw.RwUInt32(ref this.UNUSED_UINT32[1]); // observed values: 0
        }
    }
}
