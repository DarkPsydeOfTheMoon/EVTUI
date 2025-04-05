using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
	public class CSEc : ISerializable
	{
		public const int DataSize = 32;

		public Bitfield32 Flags = new Bitfield32(38);

		public Int32 AssetId;

		public UInt32 MessageCoordinateType = 4;
		public float[] MessageCoordinates = new[] { 375.0F, 528.0F };

		public byte UnkBool;
		public byte UnkEnum;

        public UInt16[] UNUSED_UINT16 = new UInt16[1];
        public UInt32[] UNUSED_UINT32 = new UInt32[2];

		public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
		{
			rw.RwObj(ref this.Flags);

			rw.RwInt32(ref this.AssetId);

            for (int i=0; i<this.UNUSED_UINT32.Length; i++)
            {
                rw.RwUInt32(ref this.UNUSED_UINT32[i]);
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
            }

			rw.RwUInt32(ref this.MessageCoordinateType);
			rw.RwFloat32s(ref this.MessageCoordinates, 2);

			rw.RwUInt8(ref this.UnkBool);
			rw.RwUInt8(ref this.UnkEnum);

            rw.RwUInt16(ref this.UNUSED_UINT16[0]);
            Trace.Assert(this.UNUSED_UINT16[0] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT16[0]}) in reserve variable.");
		}
	}
}

