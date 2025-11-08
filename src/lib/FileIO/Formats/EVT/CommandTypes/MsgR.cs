using System;
using System.Collections.Generic;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MsgR : ISerializable
    {
        public const int DataSize = 32;

        public Bitfield32 Flags = new Bitfield32(1);

        public UInt32 MessageIndex;
        public UInt32 SelectIndex;

        public UInt32 EvtLocalDataIdSelStorage;

        public UInt32 MessageCoordinateType = 4;
        public float[] MessageCoordinates = new[] { 375.0F, 528.0F };
        public float UnkFloat;

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.Flags);

            rw.RwUInt32(ref this.MessageIndex);
            rw.RwUInt32(ref this.SelectIndex);

            rw.RwUInt32(ref this.EvtLocalDataIdSelStorage);

            if ((int)args["dataSize"] > 16)
            {
                rw.RwUInt32(ref this.MessageCoordinateType);
                rw.RwFloat32s(ref this.MessageCoordinates, 2);
                rw.RwFloat32(ref this.UnkFloat);
            }
        }
    }
}
