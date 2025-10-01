using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class Fd__ : ISerializable
    {
        public const int DataSize = 16;

        public UInt32 UnkBool;
        public byte FadeMode;
        public byte FadeType;

        public ConstUInt16 UNUSED_UINT16 = new ConstUInt16();
        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 2).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.UnkBool);
            rw.RwUInt8(ref this.FadeMode);
            rw.RwUInt8(ref this.FadeType);

            rw.RwObj(ref this.UNUSED_UINT16, args);
            rw.RwObj(ref this.UNUSED_UINT32[0], args);
            rw.RwObj(ref this.UNUSED_UINT32[1], args);
        }
    }
}
