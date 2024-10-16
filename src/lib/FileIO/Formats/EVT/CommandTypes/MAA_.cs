using System;
using System.Collections.Generic;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MAA_ : ISerializable
    {
        public const int DataSize = 32;

        public Int32 UnkEnum;
        public Int32 PrimaryAnimationIndex;
        public Int32 SecondaryAnimationIndex;
        public float PrimaryAnimationSpeed;
        public Int32 LoopBool;
        public float SecondaryAnimationSpeed;
        public Int32 UnkIndex;
        public Int32 UnkBitfield;

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwInt32(ref this.UnkEnum);                   // observed values: 0-7
            rw.RwInt32(ref this.PrimaryAnimationIndex);
            rw.RwInt32(ref this.SecondaryAnimationIndex);
            rw.RwFloat32(ref this.PrimaryAnimationSpeed);
            rw.RwInt32(ref this.LoopBool);                  // observed values: 0, 1
            rw.RwFloat32(ref this.SecondaryAnimationSpeed);
            rw.RwInt32(ref this.UnkIndex);
            rw.RwInt32(ref this.UnkBitfield);
        }
    }
}
