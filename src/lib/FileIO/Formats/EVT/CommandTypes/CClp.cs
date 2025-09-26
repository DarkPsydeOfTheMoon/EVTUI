using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class CClp : ISerializable
    {
        public const int DataSize = 16;

        public float NearClip = 1.0F;
        public float FarClip  = 60000.0F;

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 2).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.UNUSED_UINT32[0], args);
            rw.RwObj(ref this.UNUSED_UINT32[1], args);

            rw.RwFloat32(ref this.NearClip);
            rw.RwFloat32(ref this.FarClip);
        }
    }
}
