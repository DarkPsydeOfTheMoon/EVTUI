using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
	public class CShk : ISerializable
	{
		public const int DataSize = 32;

        public UInt32 Action;
        public UInt32 ShakingType;

        public float Magnitude = 1.0F;
        public float Speed     = 1.0F;

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 4).Select(i => new ConstUInt32()).ToArray();

		public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
		{
            rw.RwObj(ref this.UNUSED_UINT32[0], args);

			rw.RwUInt32(ref this.Action);
			rw.RwUInt32(ref this.ShakingType);

			rw.RwFloat32(ref this.Magnitude);
			rw.RwFloat32(ref this.Speed);

            rw.RwObj(ref this.UNUSED_UINT32[1], args);
            rw.RwObj(ref this.UNUSED_UINT32[2], args);
            rw.RwObj(ref this.UNUSED_UINT32[3], args);
		}
	}
}

