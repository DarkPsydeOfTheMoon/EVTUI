using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class CMCn : ISerializable
    {
        public const int DataSize = 16;

        public UInt32 DirectionType;
        public float Distance;

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 2).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.UNUSED_UINT32[0], args);

            rw.RwUInt32(ref this.DirectionType);
            rw.RwFloat32(ref this.Distance);

            rw.RwObj(ref this.UNUSED_UINT32[1], args);
        }
    }
}
