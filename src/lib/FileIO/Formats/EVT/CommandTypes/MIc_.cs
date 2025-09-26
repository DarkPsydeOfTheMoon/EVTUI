using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MIc_ : ISerializable
    {
        public const int DataSize = 16;

        public UInt32 IconType;
        public UInt32 IconSize;

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 2).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.UNUSED_UINT32[0], args);

            rw.RwUInt32(ref this.IconType);
            rw.RwUInt32(ref this.IconSize);

            rw.RwObj(ref this.UNUSED_UINT32[1], args);
        }
    }
}
