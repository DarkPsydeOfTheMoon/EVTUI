using System;
using System.Collections.Generic;
using System.Linq;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class ESH_ : ISerializable
    {
        public const int DataSize = 32;

        public Bitfield32 Flags = new Bitfield32();

        public UInt16 StartCorrectionFrameNumber;
        public UInt16 EndCorrectionFrameNumber;

        public UInt32 StartInterpolationParameters = 4354;
        public UInt32 EndInterpolationParameters = 4354;

        public Int32 ModelObjectId;
        public UInt32 HelperId;

        public ConstUInt32[] UNUSED_UINT32 = Enumerable.Range(0, 2).Select(i => new ConstUInt32()).ToArray();

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.Flags);

            rw.RwUInt16(ref this.StartCorrectionFrameNumber);
            rw.RwUInt16(ref this.EndCorrectionFrameNumber);

            rw.RwUInt32(ref this.StartInterpolationParameters);
            rw.RwUInt32(ref this.EndInterpolationParameters);

            rw.RwInt32(ref this.ModelObjectId);
            rw.RwUInt32(ref this.HelperId);

            rw.RwObj(ref this.UNUSED_UINT32[0], args);
            rw.RwObj(ref this.UNUSED_UINT32[1], args);
        }
    }
}
