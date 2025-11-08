using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class Msg_ : ISerializable
    {
        public const int DataSize = 176;

        public Bitfield32 Flags = new Bitfield32(1);

        public UInt16 MessageMajorId;
        public byte MessageMinorId;
        public byte MessageSubId;

        public UInt16 SelectMajorId;
        public byte SelectMinorId;
        public byte SelectSubId;

        public UInt32 EvtLocalDataIdSelStorage;

        public UInt32 MessageCoordinateType = 4;
        public float[] MessageCoordinates = new[] { 375.0F, 528.0F };
        public float UnkFloat;

        public Bitfield32 EntryFlags = new Bitfield32();
        public UInt32 UnkEnum;
        //public SerialMsgEntry[] Entries = new SerialMsgEntry[14];
        public SerialMsgEntry[] Entries = Enumerable.Range(0, 14).Select(i => new SerialMsgEntry()).ToArray();

        public float[] RoyalUnkFloats = new float[2];

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 4).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.Flags);

            rw.RwUInt16(ref this.MessageMajorId);
            rw.RwUInt8(ref this.MessageMinorId);
            rw.RwUInt8(ref this.MessageSubId);

            rw.RwUInt16(ref this.SelectMajorId);
            rw.RwUInt8(ref this.SelectMinorId);
            rw.RwUInt8(ref this.SelectSubId);

            rw.RwUInt32(ref this.EvtLocalDataIdSelStorage);

            if ((int)args["dataSize"] > 16)
            {
                rw.RwUInt32(ref this.MessageCoordinateType);
                rw.RwFloat32s(ref this.MessageCoordinates, 2);
                rw.RwFloat32(ref this.UnkFloat);

                if ((int)args["dataSize"] > 32)
                {
                    rw.RwObj(ref this.EntryFlags);
                    rw.RwUInt32(ref this.UnkEnum);

                    rw.RwObj(ref this.UNUSED_UINT32[0], args);
                    rw.RwObj(ref this.UNUSED_UINT32[1], args);

                    rw.RwObjs(ref this.Entries, 14);

                    if ((int)args["dataSize"] > 160)
                    {
                        rw.RwFloat32s(ref this.RoyalUnkFloats, 2);

                        rw.RwObj(ref this.UNUSED_UINT32[2], args);
                        rw.RwObj(ref this.UNUSED_UINT32[3], args);
                    }
                }
            }
        }
    }
}
