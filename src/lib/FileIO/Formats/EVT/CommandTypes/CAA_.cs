using System;
using System.Collections.Generic;
using System.Diagnostics;

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

        public UInt32[] UNUSED_UINT32 = new UInt32[8];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.Flags);
            rw.RwInt32(ref this.AssetId);
            rw.RwUInt32(ref this.AnimationId);
            rw.RwFloat32(ref this.PlaybackSpeed);

            for (int i=0; i<this.UNUSED_UINT32.Length; i++)
            {
                rw.RwUInt32(ref this.UNUSED_UINT32[i]);
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
            }
        }
    }
}
