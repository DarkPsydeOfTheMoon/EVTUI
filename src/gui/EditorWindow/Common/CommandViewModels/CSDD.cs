using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class CSDD : Generic
{
    public CSDD(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Camera: Placement (Old)";

        this.AngleOfView = new NumRangeField("Angle of View", this.Editable, this.CommandData.AngleOfView, 1, 180, 1);
        this.WhenAnyValue(_ => _.AngleOfView.Value).Subscribe(_ => this.CommandData.AngleOfView = (float)this.AngleOfView.Value);

        // viewport
        this.Position = new Position3D("Viewport Coordinates", this.Editable, this.CommandData.ViewportCoordinates);
        // i may have yaw and pitch switched around here, dunno
        this.Rotation = new RotationWidget(config, this.CommandData.ViewportRotation, this.CommandData.Flags, yawInd: 0, pitchInd: 1, name: "Viewport Rotation (Degrees)");

        // focus/blur
        this.EnableDOF = new BoolChoiceField("Enable Depth-Of-Field?", this.Editable, this.CommandData.Flags[0]);
        this.WhenAnyValue(_ => _.EnableDOF.Value).Subscribe(_ => this.CommandData.Flags[0] = this.EnableDOF.Value);
        this.FocalDistance = new NumRangeField("Focal Distance", this.Editable, this.CommandData.FocalPlaneDistance, 0, 999999, 1);
        this.WhenAnyValue(_ => _.FocalDistance.Value).Subscribe(_ => this.CommandData.FocalPlaneDistance = (float)this.FocalDistance.Value);
        this.NearBlurDistance = new NumRangeField("Near Blur Distance", this.Editable, this.CommandData.NearBlurSurface, 0, 999999, 1);
        this.WhenAnyValue(_ => _.NearBlurDistance.Value).Subscribe(_ => this.CommandData.NearBlurSurface = (float)this.NearBlurDistance.Value);
        this.FarBlurDistance = new NumRangeField("Far Blur Distance", this.Editable, this.CommandData.FarBlurSurface, 0, 999999, 1);
        this.WhenAnyValue(_ => _.FarBlurDistance.Value).Subscribe(_ => this.CommandData.FarBlurSurface = (float)this.FarBlurDistance.Value);
        this.BlurStrength = new NumRangeField("Blur Strength", this.Editable, this.CommandData.BlurStrength, 0.5, 1, 0.01);
        this.WhenAnyValue(_ => _.BlurStrength.Value).Subscribe(_ => this.CommandData.BlurStrength = (float)this.BlurStrength.Value);

        // unknown
        this.UnkBool = new BoolChoiceField("Unknown #1", this.Editable, this.CommandData.Flags[1]);
        this.WhenAnyValue(_ => _.UnkBool.Value).Subscribe(_ => this.CommandData.Flags[1] = this.UnkBool.Value);
    }

    public NumRangeField AngleOfView { get; set; }

    // viewport
    public Position3D     Position { get; set; }
    public RotationWidget Rotation { get; set; }

    // focus/blur
    public BoolChoiceField      EnableDOF        { get; set; }
    public NumRangeField        FocalDistance    { get; set; }
    public NumRangeField        NearBlurDistance { get; set; }
    public NumRangeField        FarBlurDistance  { get; set; }
    public NumRangeField        BlurStrength     { get; set; }

    // unknown
    public BoolChoiceField UnkBool   { get; set; }
}
