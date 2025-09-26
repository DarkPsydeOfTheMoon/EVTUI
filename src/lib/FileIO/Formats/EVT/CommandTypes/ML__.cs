using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class ML__ : ISerializable
    {
        public const int DataSize = 48;

        public UInt32 Enable;
        public UInt32 UNK = 4354;

        public UInt32 AmbientRGBA = 0xBFBFBFFF;
        public UInt32 DiffuseRGBA = 0xE5E5E5FF;
        public UInt32 SpecularRGBA = 0xFFFFFFFF;

        public float[] Direction = new float[] {0.0F, 0.5299989581108093F, 0.847998321056366F};

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 4).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.Enable);
            rw.RwUInt32(ref this.UNK);

            rw.RwObj(ref this.UNUSED_UINT32[0], args);
            rw.RwObj(ref this.UNUSED_UINT32[1], args);

            rw.RwUInt32(ref this.AmbientRGBA);
            rw.RwUInt32(ref this.DiffuseRGBA);
            rw.RwUInt32(ref this.SpecularRGBA);

            rw.RwFloat32s(ref this.Direction, 3);

            rw.RwObj(ref this.UNUSED_UINT32[2], args);
            rw.RwObj(ref this.UNUSED_UINT32[3], args);
        }
    }
}
