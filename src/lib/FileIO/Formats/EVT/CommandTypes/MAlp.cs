using System;
using System.Collections.Generic;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MAlp : ISerializable
    {
        public const int DataSize = 16;

        public byte[] RGBA = new byte[] {0, 0, 0, 255};
        public UInt32 InterpolationParameters = 4354;
        public byte TranslucentMode;

        public ConstUInt8  UNUSED_UINT8  = new ConstUInt8();
        public ConstUInt16 UNUSED_UINT16 = new ConstUInt16();
        public ConstUInt32 UNUSED_UINT32 = new ConstUInt32();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.UNUSED_UINT32, args);

            rw.RwUInt8s(ref this.RGBA, 4);
            rw.RwUInt32(ref this.InterpolationParameters);
            rw.RwUInt8(ref this.TranslucentMode);

            rw.RwObj(ref this.UNUSED_UINT8, args);
            rw.RwObj(ref this.UNUSED_UINT16, args);
        }
    }
}
