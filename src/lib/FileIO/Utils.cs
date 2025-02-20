using System;

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

    //public AnimationStruct(bool loopBool = true, bool startingFrame = true, bool endingFrame = true, bool interpolatedFrames = true, bool playbackSpeed = true, bool weight = false)
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
        /*this.IncludeLoopBool = loopBool;
        this.IncludeStartingFrame = startingFrame;
        this.IncludeEndingFrame = endingFrame;
        this.IncludeInterpolatedFrames = interpolatedFrames;
        this.IncludePlaybackSpeed = playbackSpeed;
        this.IncludeWeight = weight;*/

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

public class Bitfield
{
    private bool[] Bits = new bool[32];

    public Bitfield(uint field)
    {
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

}
