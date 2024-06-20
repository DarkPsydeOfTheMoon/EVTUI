using System;
using System.Collections.Generic;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class SBE_ : ISerializable
    {
        public UInt32 Enable;
        public UInt32 UnkEnum;
        public UInt32 CueId;

        public UInt32 UNUSED_UINT32;

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.Enable);        // observed values: 0, 1 (a boolean)
            rw.RwUInt32(ref this.UnkEnum);       // observed values: 1, 2
            rw.RwUInt32(ref this.CueId);         // I'm currently hypothesizing that these are field noise sfx cues....
            rw.RwUInt32(ref this.UNUSED_UINT32); // observed values: 0
        }
    }
}
