using System;
using System.Collections.Generic;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MRgs : ISerializable
    {
        public Int32 ActionType;

        public Int32[] UNUSED_INT32 = new Int32[3];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwInt32(ref this.UNUSED_INT32[0]); // observed values: 0
            rw.RwInt32(ref this.ActionType);      // observed values: 0, 1, 2; 1 spawns model, 2 despawns... 0 presumably also despawns?
            rw.RwInt32(ref this.UNUSED_INT32[1]); // observed values: 0
            rw.RwInt32(ref this.UNUSED_INT32[2]); // observed values: 0
        }
    }
}
