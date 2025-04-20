using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class EnSh : ISerializable
    {
        public const int DataSize = 48;

        public UInt32 Unk = 4354;

        public Bitfield32 Flags = new Bitfield32(6);

        public float DepthRange = 2000.0F;
        public float Bias;
        public float Ambient = 1.0F;
        public float Diffuse = 1.0F;
        public float CascadedShadowMapPartitionInterval = 0.12F;

        public UInt32[] UNUSED_UINT32 = new UInt32[5];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.UNUSED_UINT32[0]);

            rw.RwUInt32(ref this.Unk);
            Trace.Assert(this.Unk == 4354, $"Unexpected value ({this.Unk}) in constant field (expected value: 4354).");

            rw.RwUInt32(ref this.UNUSED_UINT32[1]);
            rw.RwUInt32(ref this.UNUSED_UINT32[2]);

            rw.RwObj(ref this.Flags);

            rw.RwFloat32(ref this.DepthRange);
            rw.RwFloat32(ref this.Bias);
            rw.RwFloat32(ref this.Ambient);
            rw.RwFloat32(ref this.Diffuse);
            rw.RwFloat32(ref this.CascadedShadowMapPartitionInterval);

            rw.RwUInt32(ref this.UNUSED_UINT32[3]);
            rw.RwUInt32(ref this.UNUSED_UINT32[4]);

            for (int i=0; i<this.UNUSED_UINT32.Length; i++)
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
        }
    }
}
