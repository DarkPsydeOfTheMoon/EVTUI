using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MDt_ : ISerializable
    {
        public const int DataSize = 48;

        public Bitfield32 Flags = new Bitfield32(3);

        public UInt32 HelperId;
        public Int32 ChildObjectId;

        public float[] Position = new float[3];
        public float[] Rotation = new float[3];

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 3).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.Flags);

            rw.RwUInt32(ref this.HelperId);
            rw.RwInt32(ref this.ChildObjectId);

            rw.RwObj(ref this.UNUSED_UINT32[0], args);

            rw.RwFloat32s(ref this.Position, 3);
            rw.RwFloat32s(ref this.Rotation, 3);

            rw.RwObj(ref this.UNUSED_UINT32[1], args);
            rw.RwObj(ref this.UNUSED_UINT32[2], args);
        }
    }
}
