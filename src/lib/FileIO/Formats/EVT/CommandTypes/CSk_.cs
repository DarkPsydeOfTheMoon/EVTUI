using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
	public class CSk_ : ISerializable
	{
		public const int DataSize = 16;

        public UInt32 VibrationMode;

        public UInt32[] UNUSED_UINT32 = new UInt32[4];

		public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
		{
            rw.RwUInt32(ref this.UNUSED_UINT32[0]);
            Trace.Assert(this.UNUSED_UINT32[0] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[0]}) in reserve variable.");

			rw.RwUInt32(ref this.VibrationMode);

            for (int i=1; i<this.UNUSED_UINT32.Length; i++)
            {
                rw.RwUInt32(ref this.UNUSED_UINT32[i]);
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in reserve variable.");
            }
		}
	}
}

