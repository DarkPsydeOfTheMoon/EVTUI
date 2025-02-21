using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MFts : ISerializable
    {
        public const int DataSize = 16;

        private UInt32 _bitfield;
        public Bitfield Flags = new Bitfield(0);

        public float Strength;

        public UInt32[] UNUSED_UINT32 = new UInt32[2];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            if (rw.IsParselike())
                this._bitfield = this.Flags.Compose();
            rw.RwUInt32(ref this._bitfield);
            this.Flags = new Bitfield(this._bitfield);

            rw.RwFloat32(ref this.Strength);

            rw.RwUInt32(ref this.UNUSED_UINT32[0]);
            rw.RwUInt32(ref this.UNUSED_UINT32[1]);

            for (int i=0; i<this.UNUSED_UINT32.Length; i++)
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
        }
    }
}
