using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MCSd : ISerializable
    {
        public const int DataSize = 32;

        private UInt32 _bitfield;
        public Bitfield Flags = new Bitfield(0);

        public UInt32 InnerCircleRGBA = 0x00000080;
        public UInt32 OuterCircleRGBA = 0x00000060;

        public UInt16 InnerCircleDiameter = 20;
        public UInt16 OuterCircleDiameter = 40;

        public UInt32[] UNUSED_UINT32 = new UInt32[4];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            if (rw.IsParselike())
                this._bitfield = this.Flags.Compose();
            rw.RwUInt32(ref this._bitfield);
            this.Flags = new Bitfield(this._bitfield);

            rw.RwUInt32(ref this.InnerCircleRGBA);
            rw.RwUInt32(ref this.OuterCircleRGBA);

            rw.RwUInt32(ref this.UNUSED_UINT32[0]);

            rw.RwUInt16(ref this.InnerCircleDiameter);
            rw.RwUInt16(ref this.OuterCircleDiameter);

            rw.RwUInt32(ref this.UNUSED_UINT32[1]);
            rw.RwUInt32(ref this.UNUSED_UINT32[2]);
            rw.RwUInt32(ref this.UNUSED_UINT32[3]);

            for (int i=0; i<this.UNUSED_UINT32.Length; i++)
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
        }
    }
}
