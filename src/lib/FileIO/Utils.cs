using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serialization;

namespace EVTUI;

public class AnimationStruct
{

    // can't be nullable because they gotta be ref'able
    public UInt32 Index;
    public UInt32 LoopBool; // = 1;
    public UInt32 StartingFrame;
    public UInt32 EndingFrame;
    public UInt32 InterpolatedFrames;
    public float  PlaybackSpeed; // = 1.0F;
    public float  Weight; // = 1.0F;

    // ...so these are the compromise. sigh. it's fine
    public bool IncludeIndex = true;
    public bool IncludeLoopBool = true;
    public bool IncludeStartingFrame = true;
    public bool IncludeEndingFrame = true;
    public bool IncludeInterpolatedFrames = true;
    public bool IncludePlaybackSpeed = true;
    public bool IncludeWeight = true;

    public AnimationStruct
    (
        UInt32? index              = 0,
        UInt32? loopBool           = null,
        UInt32? startingFrame      = 0,
        UInt32? endingFrame        = null,
        UInt32? interpolatedFrames = 0,
        float? playbackSpeed       = 1.0F,
        float? weight              = null
    )
    {
        if (index is null)
            this.IncludeIndex = false;
        else
            this.Index = (UInt32)index;

        if (loopBool is null)
            this.IncludeLoopBool = false;
        else
            this.LoopBool = (UInt32)loopBool;

        if (startingFrame is null)
            this.IncludeStartingFrame = false;
        else
            this.StartingFrame = (UInt32)startingFrame;

        if (endingFrame is null)
            this.IncludeEndingFrame = false;
        else
            this.EndingFrame = (UInt32)endingFrame;

        if (interpolatedFrames is null)
            this.IncludeInterpolatedFrames = false;
        else
            this.InterpolatedFrames = (UInt32)interpolatedFrames;

        if (playbackSpeed is null)
            this.IncludePlaybackSpeed = false;
        else
            this.PlaybackSpeed = (float)playbackSpeed;

        if (weight is null)
            this.IncludeWeight = false;
        else
            this.Weight = (float)weight;
    }
}

public abstract class BitfieldBase : ISerializable
{
    private bool[] Bits;

    protected void Init(int size, uint field)
    {
        Trace.Assert(size == 8 || size == 16 || size == 32, $"Only bitfields of size 8, 16, and 32 are supported (input size: {size})");
        this.Bits = new bool[size];
        for (int i=0; i<this.Bits.Length; i++)
            this.Bits[i] = (((field >> i) & 0x1) != 0);
    }

    public bool this[int i]
    {
        get => this.Bits[i];
        set => this.Bits[i] = value;
    }

    public uint Compose()
    {
        uint field = 0;
        for (int i=0; i<this.Bits.Length; i++)
            field |= (uint)(((this.Bits[i]) ? 1 : 0) << i);
            return field;
    }

    public abstract void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget;
}

public class Bitfield32 : BitfieldBase
{
    //private UInt32 Field;
    public UInt32 Field;

    public Bitfield32(uint field = 0)
    {
        this.Field = field;
        this.Init(32, this.Field);
    }

    //public override void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    public override void ExbipHook<T>(T rw, Dictionary<string, object> args)
    {
        if (rw.IsParselike())
            this.Field = this.Compose();
        rw.RwUInt32(ref this.Field);
        this.Init(32, this.Field);
    }
}

public class Bitfield16 : BitfieldBase
{
    private UInt16 Field;

    public Bitfield16(ushort field = 0)
    {
        this.Field = field;
        this.Init(16, (uint)this.Field);
    }

    //public override void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    public override void ExbipHook<T>(T rw, Dictionary<string, object> args)
    {
        if (rw.IsParselike())
            this.Field = (ushort)this.Compose();
        rw.RwUInt16(ref this.Field);
        this.Init(16, (uint)this.Field);
    }
}

public class Bitfield8 : BitfieldBase
{
    private byte Field;

    public Bitfield8(byte field = 0)
    {
        this.Field = field;
        this.Init(8, (uint)this.Field);
    }

    //public override void ExbipHook<T>(T rw, Dictionary<string, object> args) where T : struct, IBaseBinaryTarget
    public override void ExbipHook<T>(T rw, Dictionary<string, object> args)
    {
        if (rw.IsParselike())
            this.Field = (byte)this.Compose();
        rw.RwUInt8(ref this.Field);
        this.Init(8, (uint)this.Field);
    }
}
