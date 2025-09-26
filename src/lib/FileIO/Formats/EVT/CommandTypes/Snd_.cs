using System;
using System.Collections.Generic;
using System.Linq;

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

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 3).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.UNUSED_UINT32[0], args);

            rw.RwInt32(ref this.Source);           // observed values: 0, 1, 2, 3 (1 = bgm, 2 = system, 3 = event, 0 = ...skip)
            rw.RwInt32(ref this.Action);           // observed values: 0, 1, 2 (1 = play, 2 = stop, 0 = ...skip)
            rw.RwInt32(ref this.Channel);          // observed values: 0, 1, 2, 3 (some combo of mono/stereo/left/right...? or "respect ADX" vs. "no do this actually"?)
            rw.RwInt32(ref this.CueId);

            rw.RwObj(ref this.UNUSED_UINT32[1], args);

            rw.RwInt32(ref this.FadeDuration);     // in milliseconds, I'm guessing... fadeout only, or ever fadein?

            rw.RwObj(ref this.UNUSED_UINT32[2], args);
        }
    }
}
