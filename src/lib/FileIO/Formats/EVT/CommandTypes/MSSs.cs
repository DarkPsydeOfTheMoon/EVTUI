using System;
using System.Collections.Generic;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MSSs : ISerializable
    {
        public const int DataSize = 16;

        public Int32 ShoeLayerIndex;

        public Int32[] UNUSED_INT32 = new Int32[3];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwInt32(ref this.UNUSED_INT32[0]); // observed values: 0
            rw.RwInt32(ref this.ShoeLayerIndex);  // observed values: 1, 2; also known to work with 3, 4 (should be shown based on the object model's shoe_ layer values)
            rw.RwInt32(ref this.UNUSED_INT32[1]); // observed values: 0
            rw.RwInt32(ref this.UNUSED_INT32[2]); // observed values: 0
        }
    }
}
