using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MSSs : ISerializable
    {
        public const int DataSize = 16;

        public UInt32 ShoeLayerIndex = 1;

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 3).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.UNUSED_UINT32[0], args);

            rw.RwUInt32(ref this.ShoeLayerIndex);  // observed values: 1, 2; also known to work with 3, 4 (should be shown based on the object model's shoe_ layer values)

            rw.RwObj(ref this.UNUSED_UINT32[1], args);
            rw.RwObj(ref this.UNUSED_UINT32[2], args);
        }
    }
}
