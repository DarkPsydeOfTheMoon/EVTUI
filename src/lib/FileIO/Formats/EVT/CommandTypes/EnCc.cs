using System;
using System.Collections.Generic;
using System.Linq;

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

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 5).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.Enable);
            rw.RwUInt32(ref this.Unk);

            rw.RwObj(ref this.UNUSED_UINT32[0], args);
            rw.RwObj(ref this.UNUSED_UINT32[1], args);

            rw.RwFloat32(ref this.Cyan);
            rw.RwFloat32(ref this.Magenta);
            rw.RwFloat32(ref this.Yellow);
            rw.RwFloat32(ref this.Dodge);
            rw.RwFloat32(ref this.Burn);

            rw.RwObj(ref this.UNUSED_UINT32[2], args);
            rw.RwObj(ref this.UNUSED_UINT32[3], args);
            rw.RwObj(ref this.UNUSED_UINT32[4], args);
        }
    }
}
