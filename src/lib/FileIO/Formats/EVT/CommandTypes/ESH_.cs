using System;
using System.Collections.Generic;
using System.Diagnostics;

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

        public UInt32[] UNUSED_UINT32 = new UInt32[2];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwObj(ref this.Flags);

            rw.RwUInt16(ref this.StartCorrectionFrameNumber);
            rw.RwUInt16(ref this.EndCorrectionFrameNumber);

            rw.RwUInt32(ref this.StartInterpolationParameters);
            rw.RwUInt32(ref this.EndInterpolationParameters);

            rw.RwInt32(ref this.ModelObjectId);
            rw.RwUInt32(ref this.HelperId);

            for (int i=0; i<this.UNUSED_UINT32.Length; i++)
            {
                rw.RwUInt32(ref this.UNUSED_UINT32[i]);
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[1]}) in reserve variable.");
            }
        }
    }
}
