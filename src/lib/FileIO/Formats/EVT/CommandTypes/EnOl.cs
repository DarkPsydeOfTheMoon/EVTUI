using System;
using System.Collections.Generic;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class EnOl : ISerializable
    {
        public const int DataSize = 32;

        public ConstUInt32 Unk1 = new ConstUInt32(1);
        public UInt32      Unk2 = 4354;

        public float Strength;
        public float Width = 1.01F;
        public float Brightness = 1.0F;
        public float RangeMin = 100.0F;
        public float RangeMax = 150.0F;

        public ConstUInt32 UNUSED_UINT32 = new ConstUInt32();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.Unk1, args);
            rw.RwUInt32(ref this.Unk2);

            rw.RwFloat32(ref this.Strength);
            rw.RwFloat32(ref this.Width);
            rw.RwFloat32(ref this.Brightness);
            rw.RwFloat32(ref this.RangeMin);
            rw.RwFloat32(ref this.RangeMax);

            rw.RwObj(ref this.UNUSED_UINT32, args);
        }
    }
}
