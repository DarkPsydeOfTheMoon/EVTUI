using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MMD_ : ISerializable
    {
        public Int32 UnkBool1;
        public Int32 UnkEnum;
        public float[][] Targets = new float[24][];
        public Int32 UnkIndex1;
        public float UnkFloat1;
        public Int32 UnkIndex2;
        public Int32 UnkBitflag;
        public Int32 UnkIndex3;
        public Int32 UnkIndex4;
        public Int32 UnkBool2;
        public float UnkFloat2;
        public Int32 UnkIndex5;

        public Int32 UnkIndex6;
        public Int32 UnkIndex7;
        public Int32 UnkBool3;
        public float UnkFloat3;
        public Int32 UnkIndex8;

        public Int32[] UNUSED = new Int32[8];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwInt32(ref this.UnkBool1);
            rw.RwInt32(ref this.UnkEnum);      // values: 1-6

            for (int i=0; i<24; i++)
                rw.RwFloat32s(ref this.Targets[i], 3);  // uh... do i need to initialize the second dimension arrays

            rw.RwInt32(ref this.UnkIndex1);
            rw.RwFloat32(ref this.UnkFloat1);  // mode: ~4.5
            rw.RwInt32(ref this.UnkIndex2);
            rw.RwInt32(ref this.UnkBitflag);
            rw.RwInt32(ref this.UnkIndex3);
            rw.RwInt32(ref this.UnkIndex4);
            rw.RwInt32(ref this.UnkBool2);
            rw.RwFloat32(ref this.UnkFloat2);  // mode: 1.0
            rw.RwInt32(ref this.UnkIndex5);

            rw.RwInt32(ref this.UNUSED[0]);
            rw.RwInt32(ref this.UNUSED[1]);
            rw.RwInt32(ref this.UNUSED[2]);

            rw.RwInt32(ref this.UnkIndex6);
            rw.RwInt32(ref this.UnkIndex7);
            rw.RwInt32(ref this.UnkBool3);
            rw.RwFloat32(ref this.UnkFloat3);  // mode: 1.0
            rw.RwInt32(ref this.UnkIndex8);

            rw.RwInt32(ref this.UNUSED[3]);
            rw.RwInt32(ref this.UNUSED[4]);
            rw.RwInt32(ref this.UNUSED[5]);
            rw.RwInt32(ref this.UNUSED[6]);
            rw.RwInt32(ref this.UNUSED[7]);

            for (int i=0; i<this.UNUSED.Length; i++)
                Trace.Assert(this.UNUSED[i] == 0, $"Unexpected nonzero value ({this.UNUSED[i]}) in reserve variable.");
        }
    }
}
