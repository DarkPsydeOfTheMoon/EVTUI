using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class EnSs : ISerializable
    {
        public const int DataSize = 48;

        public UInt32 Enable;
        public UInt32 Unk = 4354;

        public float Range = 545.0F;
        public float Radius = 1.15F;
        public float Attenuation = 0.45F;
        public float Concentration = 1.0F;
        public float Blur = 1.5F;

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 5).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.Enable);
            rw.RwUInt32(ref this.Unk);

            rw.RwObj(ref this.UNUSED_UINT32[0], args);
            rw.RwObj(ref this.UNUSED_UINT32[1], args);

            rw.RwFloat32(ref this.Range);
            rw.RwFloat32(ref this.Radius);
            rw.RwFloat32(ref this.Attenuation);
            rw.RwFloat32(ref this.Concentration);
            rw.RwFloat32(ref this.Blur);

            rw.RwObj(ref this.UNUSED_UINT32[2], args);
            rw.RwObj(ref this.UNUSED_UINT32[3], args);
            rw.RwObj(ref this.UNUSED_UINT32[4], args);
        }
    }
}
