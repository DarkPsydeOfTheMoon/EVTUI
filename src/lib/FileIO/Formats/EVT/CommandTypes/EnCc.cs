using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class EnCc : ISerializable
    {
        public const int DataSize = 48;

        public UInt32 Enable = 1;
        public UInt32 Unk = 4354;

        public float Cyan;
        public float Magenta;
        public float Yellow;
        public float Dodge;
        public float Burn;

        public UInt32[] UNUSED_UINT32 = new UInt32[5];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.Enable);
            rw.RwUInt32(ref this.Unk);

            rw.RwUInt32(ref this.UNUSED_UINT32[0]);
            rw.RwUInt32(ref this.UNUSED_UINT32[1]);

            rw.RwFloat32(ref this.Cyan);
            rw.RwFloat32(ref this.Magenta);
            rw.RwFloat32(ref this.Yellow);
            rw.RwFloat32(ref this.Dodge);
            rw.RwFloat32(ref this.Burn);

            rw.RwUInt32(ref this.UNUSED_UINT32[2]);
            rw.RwUInt32(ref this.UNUSED_UINT32[3]);
            rw.RwUInt32(ref this.UNUSED_UINT32[4]);

            for (int i=0; i<this.UNUSED_UINT32.Length; i++)
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
        }
    }
}
