using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class EAlp : ISerializable
    {
        public const int DataSize = 16;

        public byte[] RGBA = new byte[] {0, 0, 0, 255};
        public UInt32 InterpolationParameters = 4354;

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 2).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.UNUSED_UINT32[0], args);

            rw.RwUInt8s(ref this.RGBA, 4);
            rw.RwUInt32(ref this.InterpolationParameters);

            rw.RwObj(ref this.UNUSED_UINT32[1], args);
        }
    }
}
