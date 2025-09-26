using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class EMD_ : ISerializable
    {
        public const int DataSize = 320;

        public UInt32 InterpolationType;
        public UInt32 NumControlGroups = 1;
        public float[,] Targets = new float[24,3];

        public float MovementSpeed = 1.0F;
        public UInt32 UNK;

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 4).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.InterpolationType);
            rw.RwUInt32(ref this.NumControlGroups);

            for (int i=0; i<24; i++)
                for (int j=0; j<3; j++)
                    rw.RwFloat32(ref this.Targets[i,j]);

            rw.RwObj(ref this.UNUSED_UINT32[0], args);

            rw.RwFloat32(ref this.MovementSpeed);
            rw.RwUInt32(ref this.UNK);

            rw.RwObj(ref this.UNUSED_UINT32[1], args);
            rw.RwObj(ref this.UNUSED_UINT32[2], args);
            rw.RwObj(ref this.UNUSED_UINT32[3], args);
        }
    }
}
