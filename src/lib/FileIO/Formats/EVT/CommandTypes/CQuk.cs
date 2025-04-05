using System;
using System.Collections.Generic;
using System.Diagnostics;

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

        public UInt32[] UNUSED_UINT32 = new UInt32[4];

		public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
		{
            rw.RwUInt32(ref this.UNUSED_UINT32[0]);
            Trace.Assert(this.UNUSED_UINT32[0] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[0]}) in reserve variable.");

			rw.RwFloat32(ref this.StrengthOfShaking);
			rw.RwFloat32(ref this.DegreeOfPitch);
			rw.RwUInt32(ref this.FadeInFrames);
			rw.RwUInt32(ref this.FadeOutFrames);

            for (int i=1; i<this.UNUSED_UINT32.Length; i++)
            {
                rw.RwUInt32(ref this.UNUSED_UINT32[i]);
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
            }
		}
	}
}

