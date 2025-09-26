using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MCSd : ISerializable
    {
        public const int DataSize = 32;

        public Bitfield32 Flags = new Bitfield32();

        public UInt32 InnerCircleRGBA = 0x00000080;
        public UInt32 OuterCircleRGBA = 0x00000060;

        public UInt16 InnerCircleDiameter = 20;
        public UInt16 OuterCircleDiameter = 40;

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 4).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.Flags);

            rw.RwUInt32(ref this.InnerCircleRGBA);
            rw.RwUInt32(ref this.OuterCircleRGBA);

            rw.RwObj(ref this.UNUSED_UINT32[0], args);

            rw.RwUInt16(ref this.InnerCircleDiameter);
            rw.RwUInt16(ref this.OuterCircleDiameter);

            rw.RwObj(ref this.UNUSED_UINT32[1], args);
            rw.RwObj(ref this.UNUSED_UINT32[2], args);
            rw.RwObj(ref this.UNUSED_UINT32[3], args);
        }
    }
}
