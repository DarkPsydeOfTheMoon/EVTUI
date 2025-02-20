using System;
using System.Collections.Generic;
using System.Diagnostics;

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

        public byte UNUSED_UINT8;
        public UInt16 UNUSED_UINT16;
        public UInt32 UNUSED_UINT32;

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.UNUSED_UINT32);
            Trace.Assert(this.UNUSED_UINT32 == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32}) in reserve variable.");

            rw.RwUInt8s(ref this.RGBA, 4);
            rw.RwUInt32(ref this.InterpolationParameters);
            rw.RwUInt8(ref this.TranslucentMode);

            rw.RwUInt8(ref this.UNUSED_UINT8);
            Trace.Assert(this.UNUSED_UINT8 == 0, $"Unexpected nonzero value ({this.UNUSED_UINT8}) in reserve variable.");
            rw.RwUInt16(ref this.UNUSED_UINT16);
            Trace.Assert(this.UNUSED_UINT16 == 0, $"Unexpected nonzero value ({this.UNUSED_UINT16}) in reserve variable.");

        }
    }
}
