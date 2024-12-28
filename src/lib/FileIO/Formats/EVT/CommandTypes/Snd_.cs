using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class Snd_ : ISerializable
    {
        public const int DataSize = 32;

        public Int32 Source;
        public Int32 Action;
        public Int32 Channel;
        public Int32 CueId;
        public Int32 FadeDuration;

        public Int32[] UNUSED_INT32 = new Int32[3];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwInt32(ref this.UNUSED_INT32[0]);
            rw.RwInt32(ref this.Source);           // observed values: 0, 1, 2, 3 (1 = bgm, 2 = system, 3 = event, 0 = ...skip)
            rw.RwInt32(ref this.Action);           // observed values: 0, 1, 2 (1 = play, 2 = stop, 0 = ...skip)
            rw.RwInt32(ref this.Channel);          // observed values: 0, 1, 2, 3 (some combo of mono/stereo/left/right...? or "respect ADX" vs. "no do this actually"?)
            rw.RwInt32(ref this.CueId);
            rw.RwInt32(ref this.UNUSED_INT32[1]);
            rw.RwInt32(ref this.FadeDuration);     // in milliseconds, I'm guessing... fadeout only, or ever fadein?
            rw.RwInt32(ref this.UNUSED_INT32[2]);
            for (int i=0; i<this.UNUSED_INT32.Length; i++)
                Trace.Assert(this.UNUSED_INT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_INT32[i]}) in reserve variable.");
        }
    }
}
