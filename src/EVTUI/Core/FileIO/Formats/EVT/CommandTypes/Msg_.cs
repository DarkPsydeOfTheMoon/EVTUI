using System;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class Msg_ : ISerializable
    {
        public Int32 MessageMode;
        public Int16 MessageMajorId;
        public byte MessageMinorId;
        public byte MessageSubId;
        public Int16 SelectMajorId;
        public byte SelectMinorId;
        public byte SelectSubId;
        public Int32 EvtLocalDataIdSelStorage;

        public int NumOfEntries;
        public struct Entry
        {
            public Int16 UNK_INT16_1;
            public Int16 UNK_INT16_2;
            public float UNK_FLOAT;
        }
        public Entry[] Entries = new Entry[16];

        public float[] UNK_FLOAT = new float[4];
        public Int16[] UNK_INT16 = new Int16[2];
        public Int32[] UNK_INT32 = new Int32[3];

        public void ExbipHook<T>(T rw) where T : struct, IBaseBinaryTarget
        {
            rw.RwInt32(ref this.MessageMode);    // a bit field! that'll be handled by the manager, not here
            rw.RwInt16(ref this.MessageMajorId);
            rw.RwUInt8(ref this.MessageMinorId);
            rw.RwUInt8(ref this.MessageSubId);
            rw.RwInt16(ref this.SelectMajorId);
            rw.RwUInt8(ref this.SelectMinorId);
            rw.RwUInt8(ref this.SelectSubId);
            rw.RwInt32(ref this.EvtLocalDataIdSelStorage);
            rw.RwInt32(ref this.UNK_INT32[0]);   // observed values: 0, 1, 2, 3, 4, 5 ... number of choices to display...?
            rw.RwFloat32(ref this.UNK_FLOAT[0]); // a very limited set
            rw.RwFloat32(ref this.UNK_FLOAT[1]); // also a very limited set
            rw.RwFloat32(ref this.UNK_FLOAT[2]); // huge range
            rw.RwInt16(ref this.UNK_INT16[0]);   // seems like a set of flags (bit field?), same as next
            rw.RwInt16(ref this.UNK_INT16[1]);   // seems like a set of flags (bit field?), same as prev
            rw.RwInt32(ref this.UNK_INT32[1]);   // observed values: -65526, 0, 3, 7
            rw.RwFloat32(ref this.UNK_FLOAT[3]); // observed values: 0.0, 1.0
            rw.RwInt32(ref this.UNK_INT32[2]);   // observed values: -65526, 0
            this.NumOfEntries = 14;
            if (Globals.LAST_DATA_SIZE == 0xB0)
                this.NumOfEntries = 16;
            for (var i=0; i<this.NumOfEntries; i++) { // no idea what these are. some kind of positioning thing?
                rw.RwInt16(ref this.Entries[i].UNK_INT16_1);
                rw.RwInt16(ref this.Entries[i].UNK_INT16_2);
                rw.RwFloat32(ref this.Entries[i].UNK_FLOAT);
            }
        }

        public void Write(string filepath) { TraitMethods.Write(this, filepath); }
        public void Read (string filepath) { TraitMethods.Read (this, filepath); }
    }
}
