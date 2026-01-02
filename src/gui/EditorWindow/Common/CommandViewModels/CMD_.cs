using System;
using System.Collections.Generic;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class CMD_ : Generic
{
    public CMD_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Camera: Coordinate Movement";

        this.AngleOfView = new NumRangeField("Angle of View", this.Editable, this.CommandData.AngleOfView, 1, 180, 1);
        this.WhenAnyValue(_ => _.AngleOfView.Value).Subscribe(_ => this.CommandData.AngleOfView = (float)this.AngleOfView.Value);

        // viewport
        this.Position = new Position3D("Viewport Coordinates", this.Editable, this.CommandData.ViewportCoordinates);
        // i may have yaw and pitch switched around here, dunno
        this.Rotation = new RotationWidget(config, this.CommandData.ViewportRotation, this.CommandData.Flags, yawInd: 0, pitchInd: 1, name: "Viewport Rotation (Degrees)");

        // interpolation
        this.InterpolationSettings = new InterpolationParameters(this.CommandData.InterpolationParameters, this.Editable);
        this.WhenAnyValue(_ => _.InterpolationSettings.InterpolationType.Choice, _ => _.InterpolationSettings.SlopeInType.Choice, _ => _.InterpolationSettings.SlopeOutType.Choice).Subscribe(_ => this.CommandData.InterpolationParameters = this.InterpolationSettings.Compose());

        // focus/blur
        this.EnableDOF = new BoolChoiceField("Enable Depth-Of-Field?", this.Editable, this.CommandData.Flags[4]);
        this.WhenAnyValue(_ => _.EnableDOF.Value).Subscribe(_ => this.CommandData.Flags[4] = this.EnableDOF.Value);
        this.FocalDistance = new NumRangeField("Focal Distance", this.Editable, this.CommandData.FocalPlaneDistance, 0, 999999, 1);
        this.WhenAnyValue(_ => _.FocalDistance.Value).Subscribe(_ => this.CommandData.FocalPlaneDistance = (float)this.FocalDistance.Value);
        this.NearBlurDistance = new NumRangeField("Near Blur Distance", this.Editable, this.CommandData.NearBlurSurface, 0, 999999, 1);
        this.WhenAnyValue(_ => _.NearBlurDistance.Value).Subscribe(_ => this.CommandData.NearBlurSurface = (float)this.NearBlurDistance.Value);
        this.FarBlurDistance = new NumRangeField("Far Blur Distance", this.Editable, this.CommandData.FarBlurSurface, 0, 999999, 1);
        this.WhenAnyValue(_ => _.FarBlurDistance.Value).Subscribe(_ => this.CommandData.FarBlurSurface = (float)this.FarBlurDistance.Value);
        this.BlurStrength = new NumRangeField("Blur Strength", this.Editable, this.CommandData.BlurStrength, 0.5, 1, 0.01);
        this.WhenAnyValue(_ => _.BlurStrength.Value).Subscribe(_ => this.CommandData.BlurStrength = (float)this.BlurStrength.Value);
        this.BlurType = new StringSelectionField("Blur Type", this.Editable, Generic.BlurTypes.Backward[this.CommandData.BlurType], Generic.BlurTypes.Keys);
        this.WhenAnyValue(_ => _.BlurType.Choice).Subscribe(_ => this.CommandData.BlurType = Generic.BlurTypes.Forward[this.BlurType.Choice]);

    }

    public NumRangeField AngleOfView { get; set; }

    // viewport
    public Position3D     Position { get; set; }
    public RotationWidget Rotation { get; set; }

    // interpolation
    public InterpolationParameters InterpolationSettings { get; set; }

    // focus/blur
    public BoolChoiceField      EnableDOF        { get; set; }
    public NumRangeField        FocalDistance    { get; set; }
    public NumRangeField        NearBlurDistance { get; set; }
    public NumRangeField        FarBlurDistance  { get; set; }
    public NumRangeField        BlurStrength     { get; set; }
    public StringSelectionField BlurType         { get; set; }
}
