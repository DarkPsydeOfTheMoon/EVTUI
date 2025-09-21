using System;
using System.Collections.Generic;
using System.Linq;

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

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 5).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.UNUSED_UINT32[0], args);

            rw.RwUInt32(ref this.Unk);

            rw.RwObj(ref this.UNUSED_UINT32[1], args);
            rw.RwObj(ref this.UNUSED_UINT32[2], args);

            rw.RwObj(ref this.Flags);

            rw.RwFloat32(ref this.DepthRange);
            rw.RwFloat32(ref this.Bias);
            rw.RwFloat32(ref this.Ambient);
            rw.RwFloat32(ref this.Diffuse);
            rw.RwFloat32(ref this.CascadedShadowMapPartitionInterval);

            rw.RwObj(ref this.UNUSED_UINT32[3], args);
            rw.RwObj(ref this.UNUSED_UINT32[4], args);
        }
    }
}
