namespace EVTUI.ViewModels.TimelineCommands;

public class CSDD : Generic
{
    public CSDD(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Camera: Placement (Old)";

        this.AngleOfView = new NumRangeField("Angle of View", this.Editable, this.CommandData.AngleOfView, 1, 180, 1);

        // viewport target position
        this.ViewportX = new NumRangeField("X", this.Editable, this.CommandData.ViewportCoordinates[0], -99999, 99999, 1);
        this.ViewportY = new NumRangeField("Y", this.Editable, this.CommandData.ViewportCoordinates[1], -99999, 99999, 1);
        this.ViewportZ = new NumRangeField("Z", this.Editable, this.CommandData.ViewportCoordinates[2], -99999, 99999, 1);

        // viewport target rotation
        // i may have yaw and pitch switched around here, dunno
        this.ViewportYaw = new NumRangeField("Yaw", this.Editable, this.CommandData.ViewportRotation[0], -180, 180, 1);
        this.ViewportPitch = new NumRangeField("Pitch", this.Editable, this.CommandData.ViewportRotation[1], -180, 180, 1);
        this.ViewportRoll = new NumRangeField("Roll", this.Editable, this.CommandData.ViewportRotation[2], -180, 180, 1);

        // focus/blur
        this.EnableDOF = new BoolChoiceField("Enable Depth-Of-Field?", this.Editable, this.CommandData.Flags[0]);
        this.FocalDistance = new NumRangeField("Focal Distance", this.Editable, this.CommandData.FocalPlaneDistance, 0, 999999, 1);
        this.NearBlurDistance = new NumRangeField("Near Blur Distance", this.Editable, this.CommandData.NearBlurSurface, 0, 999999, 1);
        this.FarBlurDistance = new NumRangeField("Far Blur Distance", this.Editable, this.CommandData.FarBlurSurface, 0, 999999, 1);
        this.BlurStrength = new NumRangeField("Blur Strength", this.Editable, this.CommandData.BlurStrength, 0.5, 1, 0.01);

        // unknown
        this.UnkBool = new BoolChoiceField("Unknown #1", this.Editable, this.CommandData.Flags[1]);
    }

    public NumRangeField AngleOfView { get; set; }

    // viewport target position
    public NumRangeField ViewportX { get; set; }
    public NumRangeField ViewportY { get; set; }
    public NumRangeField ViewportZ { get; set; }

    // viewport target rotation
    public NumRangeField ViewportYaw   { get; set; }
    public NumRangeField ViewportPitch { get; set; }
    public NumRangeField ViewportRoll  { get; set; }

    // focus/blur
    public BoolChoiceField      EnableDOF        { get; set; }
    public NumRangeField        FocalDistance    { get; set; }
    public NumRangeField        NearBlurDistance { get; set; }
    public NumRangeField        FarBlurDistance  { get; set; }
    public NumRangeField        BlurStrength     { get; set; }

    // unknown
    public BoolChoiceField UnkBool   { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();

        this.CommandData.Flags[0] = this.EnableDOF.Value;
        this.CommandData.Flags[1] = this.UnkBool.Value;

        this.CommandData.ViewportCoordinates[0] = (float)this.ViewportX.Value;
        this.CommandData.ViewportCoordinates[1] = (float)this.ViewportY.Value;
        this.CommandData.ViewportCoordinates[2] = (float)this.ViewportZ.Value;

        this.CommandData.ViewportRotation[0] = (float)this.ViewportYaw.Value;
        this.CommandData.ViewportRotation[1] = (float)this.ViewportPitch.Value;
        this.CommandData.ViewportRotation[2] = (float)this.ViewportRoll.Value;

        this.CommandData.AngleOfView = (float)this.AngleOfView.Value;

        this.CommandData.FocalPlaneDistance = (float)this.FocalDistance.Value;
        this.CommandData.NearBlurSurface = (float)this.NearBlurDistance.Value;
        this.CommandData.FarBlurSurface = (float)this.FarBlurDistance.Value;
        this.CommandData.BlurStrength = (float)this.BlurStrength.Value;
    }
}
