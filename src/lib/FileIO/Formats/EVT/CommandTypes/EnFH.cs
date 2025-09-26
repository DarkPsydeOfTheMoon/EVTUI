using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class EnFH : ISerializable
    {
        public const int DataSize = 32;

        public UInt32 Unk1 = 1;
        public UInt32 Unk2 = 4354;

        public float StartHeight = 5;
        public float EndHeight   = 2000;
        public UInt32 RGBA       = 0x7F7F7F00;

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 3).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.Unk1);
            rw.RwUInt32(ref this.Unk2);

            rw.RwObj(ref this.UNUSED_UINT32[0], args);
            rw.RwObj(ref this.UNUSED_UINT32[1], args);

            rw.RwFloat32(ref this.StartHeight);
            rw.RwFloat32(ref this.EndHeight);
            rw.RwUInt32(ref this.RGBA);

            rw.RwObj(ref this.UNUSED_UINT32[2], args);

        }
    }
}
