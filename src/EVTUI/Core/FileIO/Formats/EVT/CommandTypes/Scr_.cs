using System;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class Scr_ : ISerializable
    {
        public Int32 ProcedureIndex;

        public Int32[] UNUSED_INT32 = new Int32[3];

        public void ExbipHook<T>(T rw) where T : struct, IBaseBinaryTarget
        {
            rw.RwInt32(ref this.UNUSED_INT32[0]); // observed values: 0
            rw.RwInt32(ref this.ProcedureIndex);  // (should be shown as 0 through the BF file's number of procedures)
            rw.RwInt32(ref this.UNUSED_INT32[1]); // observed values: 0
            rw.RwInt32(ref this.UNUSED_INT32[2]); // observed values: 0
        }

        public void Write(string filepath) { TraitMethods.Write(this, filepath); }
        public void Read (string filepath) { TraitMethods.Read (this, filepath); }
    }
}
