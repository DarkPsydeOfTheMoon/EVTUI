using System.Collections.Generic;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class CSD_ : Generic
{
    public CSD_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Camera: Placement";

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
        this.BlurType = new StringSelectionField("Blur Type", this.Editable, this.BlurTypes.Backward[this.CommandData.BlurType], this.BlurTypes.Keys);

        // message
        this.EnableMessageCoordinates = new BoolChoiceField("Directly Specify Message Coordinates?", this.Editable, this.CommandData.Flags[5]);
        this.MessageCoordinateType = new StringSelectionField("Coordinate Type", this.Editable, this.MessageCoordinateTypes.Backward[this.CommandData.MessageCoordinateType], this.MessageCoordinateTypes.Keys);
        this.MessageX = new NumRangeField("X Coordinate", this.Editable, this.CommandData.MessageCoordinates[0], -9999, 9999, 1);
        this.MessageY = new NumRangeField("Y Coordinate", this.Editable, this.CommandData.MessageCoordinates[1], -9999, 9999, 1);

        // unknown
        this.UnkBool = new BoolChoiceField("Unknown #1", this.Editable, this.CommandData.Flags[8]);
        this.UnkEnum = new NumEntryField("Unknown #2", this.Editable, this.CommandData.UnkEnum, 0, 2, 1);
        this.UnkInd = new NumEntryField("Unknown #3", this.Editable, this.CommandData.UnkInd, 0, 31, 1);
        this.SuperUnk1 = new NumEntryField("Unknown #4", this.Editable, this.CommandData.SuperUnk1, 0, 255, 1);
        this.SuperUnk2 = new NumEntryField("Unknown #5", this.Editable, this.CommandData.SuperUnk2, 0, 255, 1);
        this.UnkCoord1 = new NumRangeField("Unknown #6", this.Editable, this.CommandData.UnkCoordinates[0], -99999, 99999, 1);
        this.UnkCoord2 = new NumRangeField("Unknown #7", this.Editable, this.CommandData.UnkCoordinates[1], -99999, 99999, 1);
        this.UnkCoord3 = new NumRangeField("Unknown #8", this.Editable, this.CommandData.UnkCoordinates[2], -99999, 99999, 1);
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

    public new void SaveChanges()
    {
        base.SaveChanges();

        this.CommandData.Flags[0] = this.EnableDOF.Value;
        this.CommandData.Flags[5] = this.EnableMessageCoordinates.Value;
        this.CommandData.Flags[8] = this.UnkBool.Value;

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
        this.CommandData.BlurType = this.BlurTypes.Forward[this.BlurType.Choice];

        this.CommandData.MessageCoordinateType = this.MessageCoordinateTypes.Forward[this.MessageCoordinateType.Choice];
        this.CommandData.MessageCoordinates[0] = (float)this.MessageX.Value;
        this.CommandData.MessageCoordinates[1] = (float)this.MessageY.Value;

        this.CommandData.UnkEnum           = (byte)this.UnkEnum.Value;
        this.CommandData.UnkInd            = (byte)this.UnkInd.Value;
        this.CommandData.SuperUnk1         = (byte)this.SuperUnk1.Value;
        this.CommandData.SuperUnk2         = (byte)this.SuperUnk2.Value;
        this.CommandData.UnkCoordinates[0] = (float)this.UnkCoord1.Value;
        this.CommandData.UnkCoordinates[1] = (float)this.UnkCoord2.Value;
        this.CommandData.UnkCoordinates[2] = (float)this.UnkCoord3.Value;
    }

    public BiDict<string, uint> BlurTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"5x5 Gaussian Filter",  0},
            {"2-Iteration Gaussian", 1},
            {"3-Iteration Gaussian", 2},
            {"5-Iteration Gaussian", 3},
            {"7-Iteration Gaussian", 4},
        }
    );

    public BiDict<string, uint> MessageCoordinateTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Top Left",      0},
            {"Top Center",    1},
            {"Top Right",     2},
            {"Bottom Left",   3},
            {"Bottom Center", 4},
            {"Bottom Right",  5},
        }
    );
}
