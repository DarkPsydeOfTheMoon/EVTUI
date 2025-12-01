using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class CSD_ : Generic
{
    public CSD_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Camera: Placement";

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
        this.BlurType = new StringSelectionField("Blur Type", this.Editable, this.BlurTypes.Backward[this.CommandData.BlurType], this.BlurTypes.Keys);
        this.WhenAnyValue(_ => _.BlurType.Choice).Subscribe(_ => this.CommandData.BlurType = this.BlurTypes.Forward[this.BlurType.Choice]);

        // message
        this.EnableMessageCoordinates = new BoolChoiceField("Directly Specify Message Coordinates?", this.Editable, this.CommandData.Flags[5]);
        this.WhenAnyValue(_ => _.EnableMessageCoordinates.Value).Subscribe(_ => this.CommandData.Flags[5] = this.EnableMessageCoordinates.Value);
        this.MessageCoordinateType = new StringSelectionField("Coordinate Type", this.Editable, this.MessageCoordinateTypes.Backward[this.CommandData.MessageCoordinateType], this.MessageCoordinateTypes.Keys);
        this.WhenAnyValue(_ => _.MessageCoordinateType.Choice).Subscribe(_ => this.CommandData.MessageCoordinateType = this.MessageCoordinateTypes.Forward[this.MessageCoordinateType.Choice]);
        this.MessageX = new NumRangeField("X Coordinate", this.Editable, this.CommandData.MessageCoordinates[0], -9999, 9999, 1);
        this.WhenAnyValue(_ => _.MessageX.Value).Subscribe(_ => this.CommandData.MessageCoordinates[0] = (float)this.MessageX.Value);
        this.MessageY = new NumRangeField("Y Coordinate", this.Editable, this.CommandData.MessageCoordinates[1], -9999, 9999, 1);
        this.WhenAnyValue(_ => _.MessageY.Value).Subscribe(_ => this.CommandData.MessageCoordinates[1] = (float)this.MessageY.Value);

        // unknown
        this.UnkBool = new BoolChoiceField("Unknown #1", this.Editable, this.CommandData.Flags[8]);
        this.WhenAnyValue(_ => _.UnkBool.Value).Subscribe(_ => this.CommandData.Flags[8] = this.UnkBool.Value);
        this.UnkEnum = new NumEntryField("Unknown #2", this.Editable, this.CommandData.UnkEnum, 0, 2, 1);
        this.WhenAnyValue(_ => _.UnkEnum.Value).Subscribe(_ => this.CommandData.UnkEnum = (byte)this.UnkEnum.Value);
        this.UnkInd = new NumEntryField("Unknown #3", this.Editable, this.CommandData.UnkInd, 0, 31, 1);
        this.WhenAnyValue(_ => _.UnkInd.Value).Subscribe(_ => this.CommandData.UnkInd = (byte)this.UnkInd.Value);
        this.SuperUnk1 = new NumEntryField("Unknown #4", this.Editable, this.CommandData.SuperUnk1, 0, 255, 1);
        this.WhenAnyValue(_ => _.SuperUnk1.Value).Subscribe(_ => this.CommandData.SuperUnk1 = (byte)this.SuperUnk1.Value);
        this.SuperUnk2 = new NumEntryField("Unknown #5", this.Editable, this.CommandData.SuperUnk2, 0, 255, 1);
        this.WhenAnyValue(_ => _.SuperUnk2.Value).Subscribe(_ => this.CommandData.SuperUnk2 = (byte)this.SuperUnk2.Value);
        this.UnkCoord1 = new NumRangeField("Unknown #6", this.Editable, this.CommandData.UnkCoordinates[0], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.UnkCoord1.Value).Subscribe(_ => this.CommandData.UnkCoordinates[0] = (float)this.UnkCoord1.Value);
        this.UnkCoord2 = new NumRangeField("Unknown #7", this.Editable, this.CommandData.UnkCoordinates[1], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.UnkCoord2.Value).Subscribe(_ => this.CommandData.UnkCoordinates[1] = (float)this.UnkCoord2.Value);
        this.UnkCoord3 = new NumRangeField("Unknown #8", this.Editable, this.CommandData.UnkCoordinates[2], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.UnkCoord3.Value).Subscribe(_ => this.CommandData.UnkCoordinates[2] = (float)this.UnkCoord3.Value);
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
    public StringSelectionField BlurType         { get; set; }

    // message
    public BoolChoiceField      EnableMessageCoordinates { get; set; }
    public StringSelectionField MessageCoordinateType    { get; set; }
    public NumRangeField        MessageX                 { get; set; }
    public NumRangeField        MessageY                 { get; set; }

    // unknown
    public BoolChoiceField UnkBool   { get; set; }
    public NumEntryField   UnkEnum   { get; set; }
    public NumEntryField   UnkInd    { get; set; }
    public NumEntryField   SuperUnk1 { get; set; }
    public NumEntryField   SuperUnk2 { get; set; }
    public NumRangeField   UnkCoord1 { get; set; }
    public NumRangeField   UnkCoord2 { get; set; }
    public NumRangeField   UnkCoord3 { get; set; }
}
