using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public partial class CommandTypes
{
    public class MSD_ : ISerializable
    {
        public float[] Position = new float[3];
        public float[] Rotation = new float[3];
        public Int32 AnimationIndex;
        public Int32 LoopBool;
        public float AnimationSpeed;
        public Int32 FirstFrameInd;
        public Int32 LastFrameInd;

        public UInt32 UNK_UINT32;

        public UInt32[] UNUSED_UINT32 = new UInt32[4];

        public void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
        {
            rw.RwFloat32s(ref this.Position, 3);
            rw.RwFloat32s(ref this.Rotation, 3);        // in degrees, according to the 010 template
            rw.RwInt32(ref this.AnimationIndex);        // from BE_... / BaseMotionNo ... I think?
            rw.RwUInt32(ref this.UNK_UINT32);           // is a float in EvtTool but that makes no sense tbqh, also inconsistent with MAB_
            rw.RwInt32(ref this.LoopBool);              // it's only ever 0, 1, or -1020883212 (??), so it may be a bool... LoopBool?
            rw.RwFloat32(ref this.AnimationSpeed);      // most common is 1.0
            rw.RwInt32(ref this.FirstFrameInd);         // (an educated guess)
            rw.RwInt32(ref this.LastFrameInd);          // (an educated guess)
            for (var i=0; i<4; i++)
            {
                rw.RwUInt32(ref this.UNUSED_UINT32[i]); // observed values: 0
                Trace.Assert(this.UNUSED_UINT32[i] == 0, $"Unexpected nonzero value ({this.UNUSED_UINT32[i]}) in MSD_ reserve variable.");
            }
        }
    }
}
