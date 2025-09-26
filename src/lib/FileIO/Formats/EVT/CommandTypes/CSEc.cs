using System;
using System.Collections.Generic;
using System.Linq;

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

        public ConstUInt16 UNUSED_UINT16 = new ConstUInt16();
        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 2).Select(i => new ConstUInt32()).ToArray();

		public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
		{
			rw.RwObj(ref this.Flags);

			rw.RwInt32(ref this.AssetId);

			rw.RwObj(ref this.UNUSED_UINT32[0], args);
			rw.RwObj(ref this.UNUSED_UINT32[1], args);

			rw.RwUInt32(ref this.MessageCoordinateType);
			rw.RwFloat32s(ref this.MessageCoordinates, 2);

			rw.RwUInt8(ref this.UnkBool);
			rw.RwUInt8(ref this.UnkEnum);

            rw.RwObj(ref this.UNUSED_UINT16, args);
		}
	}
}

