using System;
using System.Collections.Generic;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MRgs : ISerializable
    {
        public UInt32 ActionType;

        public UInt32[] UNUSED_UINT32 = new UInt32[3];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.UNUSED_UINT32[0]); // observed values: 0
            rw.RwUInt32(ref this.ActionType);       // observed values: 0, 1, 2; 1 spawns model, 2 despawns... 0 presumably also despawns?
            rw.RwUInt32(ref this.UNUSED_UINT32[1]); // observed values: 0
            rw.RwUInt32(ref this.UNUSED_UINT32[2]); // observed values: 0
        }
    }
}
