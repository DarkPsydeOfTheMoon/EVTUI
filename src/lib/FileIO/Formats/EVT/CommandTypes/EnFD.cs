using System;
using System.Collections.Generic;
using System.Linq;

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

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 2).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.Flags);
            rw.RwUInt32(ref this.Unk);

            rw.RwObj(ref this.UNUSED_UINT32[0], args);
            rw.RwObj(ref this.UNUSED_UINT32[1], args);

            rw.RwUInt32(ref this.Mode);
            rw.RwFloat32(ref this.StartDistance);
            rw.RwFloat32(ref this.EndDistance);
            rw.RwUInt32(ref this.RGBA);
        }
    }
}
