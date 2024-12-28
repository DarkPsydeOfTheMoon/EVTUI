using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MLa_ : ISerializable
    {
        public Int16 UnkBool;
        public Int16 UnkIndex1;
        public Int16 UnkEnum1;
        public Int16 UnkEnum2;
        public Int16 UNUSED;
        public Int16 UnkEnum3;
        public float[] Target = new float[3];
        public Int32 UnkIndex2;
        public Int32 BoneId;

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwInt16(ref this.UnkBool);
            rw.RwInt16(ref this.UnkIndex1);
            rw.RwInt16(ref this.UnkEnum1);     // values: 0-4
            rw.RwInt16(ref this.UnkEnum2);     // values: 0-3

            rw.RwInt16(ref this.UNUSED);
            Trace.Assert(this.UNUSED == 0, $"Unexpected nonzero value ({this.UNUSED}) in reserve variable.");
            
            rw.RwInt16(ref this.UnkEnum3);     // values: 0-3
            rw.RwFloat32s(ref this.Target, 3);
            rw.RwInt32(ref this.UnkIndex2);
            rw.RwInt32(ref this.BoneId);
        }
    }
}
