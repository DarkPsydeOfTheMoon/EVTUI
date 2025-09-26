using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class CAA_ : ISerializable
    {
        public const int DataSize = 48;

        public Bitfield32 Flags = new Bitfield32();
        public Int32 AssetId;
        public UInt32 AnimationId;
        public float PlaybackSpeed = 1.0F;

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 8).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.Flags);

            rw.RwInt32(ref this.AssetId);
            rw.RwUInt32(ref this.AnimationId);
            rw.RwFloat32(ref this.PlaybackSpeed);

            rw.RwObj(ref this.UNUSED_UINT32[0], args);
            rw.RwObj(ref this.UNUSED_UINT32[1], args);
            rw.RwObj(ref this.UNUSED_UINT32[2], args);
            rw.RwObj(ref this.UNUSED_UINT32[3], args);
            rw.RwObj(ref this.UNUSED_UINT32[4], args);
            rw.RwObj(ref this.UNUSED_UINT32[5], args);
            rw.RwObj(ref this.UNUSED_UINT32[6], args);
            rw.RwObj(ref this.UNUSED_UINT32[7], args);
        }
    }
}
