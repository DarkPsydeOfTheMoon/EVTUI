using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
	public class CSk_ : ISerializable
	{
		public const int DataSize = 16;

        public UInt32 VibrationMode;

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 3).Select(i => new ConstUInt32()).ToArray();

		public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
		{
            rw.RwObj(ref this.UNUSED_UINT32[0], args);

			rw.RwUInt32(ref this.VibrationMode);

            rw.RwObj(ref this.UNUSED_UINT32[1], args);
            rw.RwObj(ref this.UNUSED_UINT32[2], args);
		}
	}
}

