using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class EnFD : ISerializable
    {
        public const int DataSize = 32;

        public Bitfield32 Flags = new Bitfield32(65537);
        public UInt32 Unk = 4354;

        public UInt32 Mode;
        public float StartDistance = 5;
        public float EndDistance   = 2000;
        public UInt32 RGBA         = 0x7F7F7F00;

        public UInt32[] UNUSED_UINT32 = new UInt32[2];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.Flags);
            rw.RwUInt32(ref this.Unk);

            for (int i=0; i<this.UNUSED_UINT32.Length; i++)
            {
                rw.RwUInt32(ref this.UNUSED_UINT32[i]);
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
            }

            rw.RwUInt32(ref this.Mode);
            rw.RwFloat32(ref this.StartDistance);
            rw.RwFloat32(ref this.EndDistance);
            rw.RwUInt32(ref this.RGBA);
        }
    }
}
