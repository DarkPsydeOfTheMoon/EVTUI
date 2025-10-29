using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class CQuk : Generic
{
    public CQuk(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Camera: Quake Effect";

        this.StrengthOfShaking = new NumRangeField("Strength of Shaking", this.Editable, this.CommandData.StrengthOfShaking, 0, 100, 1);
        this.WhenAnyValue(_ => _.StrengthOfShaking.Value).Subscribe(_ => this.CommandData.StrengthOfShaking = (float)this.StrengthOfShaking.Value);
        this.DegreeOfPitch = new NumRangeField("Degree of Pitch", this.Editable, this.CommandData.DegreeOfPitch, 0, 1, 0.01);
        this.WhenAnyValue(_ => _.DegreeOfPitch.Value).Subscribe(_ => this.CommandData.DegreeOfPitch = (float)this.DegreeOfPitch.Value);
        this.FadeInFrames = new NumRangeField("Fade-In Frames", this.Editable, this.CommandData.FadeInFrames, 0, 10000, 1);
        this.WhenAnyValue(_ => _.FadeInFrames.Value).Subscribe(_ => this.CommandData.FadeInFrames = (uint)this.FadeInFrames.Value);
        this.FadeOutFrames = new NumRangeField("Fade-Out Frames", this.Editable, this.CommandData.FadeOutFrames, 0, 10000, 1);
        this.WhenAnyValue(_ => _.FadeOutFrames.Value).Subscribe(_ => this.CommandData.FadeOutFrames = (uint)this.FadeOutFrames.Value);
    }

    public NumRangeField StrengthOfShaking { get; set; }
    public NumRangeField DegreeOfPitch     { get; set; }
    public NumRangeField FadeInFrames      { get; set; }
    public NumRangeField FadeOutFrames     { get; set; }
}
