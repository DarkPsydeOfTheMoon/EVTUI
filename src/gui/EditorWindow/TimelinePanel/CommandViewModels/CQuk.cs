namespace EVTUI.ViewModels.TimelineCommands;

public class CQuk : Generic
{
    public CQuk(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Camera: Quake Effect";

        this.StrengthOfShaking = new NumRangeField("Strength of Shaking", this.Editable, this.CommandData.StrengthOfShaking, 0, 100, 1);
        this.DegreeOfPitch = new NumRangeField("Degree of Pitch", this.Editable, this.CommandData.DegreeOfPitch, 0, 1, 0.01);
        this.FadeInFrames = new NumRangeField("Fade-In Frames", this.Editable, this.CommandData.FadeInFrames, 0, 10000, 1);
        this.FadeOutFrames = new NumRangeField("Fade-Out Frames", this.Editable, this.CommandData.FadeOutFrames, 0, 10000, 1);
    }

    public NumRangeField StrengthOfShaking { get; set; }
    public NumRangeField DegreeOfPitch     { get; set; }
    public NumRangeField FadeInFrames      { get; set; }
    public NumRangeField FadeOutFrames     { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();

        this.CommandData.StrengthOfShaking = (float)this.StrengthOfShaking.Value;
        this.CommandData.DegreeOfPitch     = (float)this.DegreeOfPitch.Value;
        this.CommandData.FadeInFrames      = (uint)this.FadeInFrames.Value;
        this.CommandData.FadeOutFrames     = (uint)this.FadeOutFrames.Value;
    }
}
