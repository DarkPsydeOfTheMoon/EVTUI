using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
	public class CQuk : ISerializable
	{
		public const int DataSize = 32;

		public float StrengthOfShaking;
		public float DegreeOfPitch;
		public UInt32 FadeInFrames;
		public UInt32 FadeOutFrames;

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 4).Select(i => new ConstUInt32()).ToArray();

		public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
		{
            rw.RwObj(ref this.UNUSED_UINT32[0], args);

			rw.RwFloat32(ref this.StrengthOfShaking);
			rw.RwFloat32(ref this.DegreeOfPitch);
			rw.RwUInt32(ref this.FadeInFrames);
			rw.RwUInt32(ref this.FadeOutFrames);

            rw.RwObj(ref this.UNUSED_UINT32[1], args);
            rw.RwObj(ref this.UNUSED_UINT32[2], args);
            rw.RwObj(ref this.UNUSED_UINT32[3], args);
		}
	}
}

