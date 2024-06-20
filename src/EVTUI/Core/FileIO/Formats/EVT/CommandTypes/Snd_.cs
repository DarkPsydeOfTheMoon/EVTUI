using System;
using System.Collections.Generic;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class Snd_ : ISerializable
    {
        public UInt32 Source;
        public UInt32 Action;
        public UInt32 Channel;
        public UInt32 CueId;
        public UInt32 FadeDuration;

        public UInt32[] UNUSED_UINT32 = new UInt32[3];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwUInt32(ref this.UNUSED_UINT32[0]); // observed values: 0
            rw.RwUInt32(ref this.Source);           // observed values: 0, 1, 2, 3 (1 = bgm, 2 = system, 3 = event, 0 = ...skip)
            rw.RwUInt32(ref this.Action);           // observed values: 0, 1, 2 (1 = play, 2 = stop, 0 = ...skip)
            rw.RwUInt32(ref this.Channel);          // observed values: 0, 1, 2, 3 (some combo of mono/stereo/left/right...? or "respect ADX" vs. "no do this actually"?)
            rw.RwUInt32(ref this.CueId);
            rw.RwUInt32(ref this.UNUSED_UINT32[1]); // observed values: 0
            rw.RwUInt32(ref this.FadeDuration);     // in milliseconds, I'm guessing... fadeout only, or ever fadein?
            rw.RwUInt32(ref this.UNUSED_UINT32[2]); // observed values: 0
        }
    }
}
