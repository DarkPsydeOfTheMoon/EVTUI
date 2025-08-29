using System;
using System.Collections.Generic;
using System.Diagnostics;

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

        public UInt32[] UNUSED_UINT32 = new UInt32[4];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.InterpolationType);
            rw.RwUInt32(ref this.NumControlGroups);

            for (int i=0; i<24; i++)
                for (int j=0; j<3; j++)
                    rw.RwFloat32(ref this.Targets[i,j]);

            rw.RwUInt32(ref this.UNUSED_UINT32[0]);
            Trace.Assert(this.UNUSED_UINT32[0] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[0]}) in reserve variable.");

            rw.RwFloat32(ref this.MovementSpeed);
            rw.RwUInt32(ref this.UNK);

            for (int i=1; i<this.UNUSED_UINT32.Length; i++)
            {
                rw.RwUInt32(ref this.UNUSED_UINT32[i]);
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
            }
        }
    }
}
