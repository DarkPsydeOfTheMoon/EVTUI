using System;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MAB_ : ISerializable
    {
        public Int32 PrimaryAnimationIndex;
        public Int32 PrimaryLoopBool;
        public float PrimaryAnimationSpeed;
        public Int32 SecondaryAnimationIndex;
        public Int32 SecondaryLoopBool;
        public float SecondaryAnimationSpeed;
        public byte AnimationMode;
        public Int32 FirstFrameInd;
        public Int32 LastFrameInd;

        public byte[] UNK_UINT8 = new byte[1];
        public Int16[] UNK_INT16 = new Int16[1];
        public Int32[] UNK_INT32 = new Int32[5];

        public Int32[] UNUSED_INT32 = new Int32[2];

        public void ExbipHook<T>(T rw) where T : struct, IBaseBinaryTarget
        {
            rw.RwInt32(ref this.PrimaryAnimationIndex);
            rw.RwInt32(ref this.UNK_INT32[0]);
            rw.RwInt32(ref this.PrimaryLoopBool);   // observed values: 0, 1
            rw.RwFloat32(ref this.PrimaryAnimationSpeed);
            rw.RwInt32(ref this.SecondaryAnimationIndex);
            rw.RwInt32(ref this.UNK_INT32[1]);
            rw.RwInt32(ref this.SecondaryLoopBool); // observed values: 0, 1
            rw.RwFloat32(ref this.SecondaryAnimationSpeed);
            rw.RwUInt8(ref this.AnimationMode);     // oh this is for sure a bit field, right...
            rw.RwUInt8(ref this.UNK_UINT8[0]);
            rw.RwInt16(ref this.UNK_INT16[0]);
            rw.RwInt32(ref this.FirstFrameInd);
            rw.RwInt32(ref this.LastFrameInd);
            for (var i=2; i<5; i++)
                rw.RwInt32(ref this.UNK_INT32[i]);
            rw.RwInt32(ref this.UNUSED_INT32[0]); // observed values: 0
            rw.RwInt32(ref this.UNUSED_INT32[1]); // observed values: 0
        }

        public void Write(string filepath) { TraitMethods.Write(this, filepath); }
        public void Read (string filepath) { TraitMethods.Read (this, filepath); }
    }
}
