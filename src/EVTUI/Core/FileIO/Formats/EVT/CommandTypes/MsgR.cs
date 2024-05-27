using System;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MsgR : ISerializable
    {
        public Int32 MessageMode;
        public Int32 MessageIndex;
        public Int32 SelIndex;
        public Int32 EvtLocalDataIdSelStorage;

        public float[] UNK_FLOAT = new float[3];
        public Int32[] UNK_INT32 = new Int32[1];

        public void ExbipHook<T>(T rw) where T : struct, IBaseBinaryTarget
        {
            rw.RwInt32(ref this.MessageMode);              // same bit field as Msg_
            rw.RwInt32(ref this.MessageIndex);             // should be shown as 0 through the BMD file's number of messages
            rw.RwInt32(ref this.SelIndex);                 // should be shown as 0 through the BMD file's number of messages... iirc
            rw.RwInt32(ref this.EvtLocalDataIdSelStorage); // same as in Msg_ but only ever seems to be 0 or 1 since MsgR is rarer
            rw.RwInt32(ref this.UNK_INT32[0]);             // same as Msg_
            rw.RwFloat32(ref this.UNK_FLOAT[0]);           // same as Msg_
            rw.RwFloat32(ref this.UNK_FLOAT[1]);           // same as Msg_
            rw.RwFloat32(ref this.UNK_FLOAT[2]);           // same as Msg_
        }

        public void Write(string filepath) { TraitMethods.Write(this, filepath); }
        public void Read (string filepath) { TraitMethods.Read (this, filepath); }
    }
}
