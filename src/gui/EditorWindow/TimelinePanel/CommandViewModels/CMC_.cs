using System.Collections.Generic;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class CMC_ : Generic
{
    public CMC_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Camera: Character-Centric Movement";

        // asset/angle/whatever
        // this is technically 0-999, and also model assets only, so... TODO either way
        this.AssetID = new IntSelectionField("Model Asset ID", this.Editable, this.CommandData.AssetId, config.EventManager. AssetIDs);
        this.ShotType = new StringSelectionField("Shot Type", this.Editable, this.ShotTypes.Backward[this.CommandData.ShotType], this.ShotTypes.Keys);
        this.AngleType = new StringSelectionField("Angle Type", this.Editable, this.AngleTypes.Backward[this.CommandData.AngleType], this.AngleTypes.Keys);
        this.StartCorrectionFrame = new NumRangeField("Start Correction At Frame", this.Editable, this.CommandData.StartCorrectionFrameNumber, 0, 60, 1);

        // interpolation
        this.InterpolationSettings = new InterpolationParameters(this.CommandData.InterpolationParameters, this.Editable);

        // focus/blur
        this.EnableDOF = new BoolChoiceField("Enable Depth-Of-Field?", this.Editable, this.CommandData.Flags[0]);
        this.EnableCustomDOF = new BoolChoiceField("Customize Depth-Of-Field?", this.Editable, this.CommandData.Flags[1]);
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
    }

    // asset/angle/whatever
    public IntSelectionField    AssetID              { get; set; }
    public StringSelectionField ShotType             { get; set; }
    public StringSelectionField AngleType            { get; set; }
    public NumRangeField        StartCorrectionFrame { get; set; }

    // interpolation
    public InterpolationParameters InterpolationSettings { get; set; }

    // focus/blur
    public BoolChoiceField      EnableDOF        { get; set; }
    public BoolChoiceField      EnableCustomDOF  { get; set; }
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

    public new void SaveChanges()
    {
        base.SaveChanges();

        this.CommandData.Flags[0] = this.EnableDOF.Value;
        this.CommandData.Flags[1] = this.EnableCustomDOF.Value;
        this.CommandData.Flags[5] = this.EnableMessageCoordinates.Value;

        this.CommandData.StartCorrectionFrameNumber = (ushort)this.StartCorrectionFrame.Value;
        this.CommandData.InterpolationParameters = this.InterpolationSettings.Compose();

        this.CommandData.AssetId = this.AssetID.Choice;
        this.CommandData.ShotType = this.ShotTypes.Forward[this.ShotType.Choice];
        this.CommandData.AngleType = this.AngleTypes.Forward[this.AngleType.Choice];

        this.CommandData.FocalPlaneDistance = (float)this.FocalDistance.Value;
        this.CommandData.NearBlurSurface = (float)this.NearBlurDistance.Value;
        this.CommandData.FarBlurSurface = (float)this.FarBlurDistance.Value;
        this.CommandData.BlurStrength = (float)this.BlurStrength.Value;

        this.CommandData.BlurType = this.BlurTypes.Forward[this.BlurType.Choice];
        this.CommandData.MessageCoordinateType = this.MessageCoordinateTypes.Forward[this.MessageCoordinateType.Choice];
        this.CommandData.MessageCoordinates[0] = (float)this.MessageX.Value;
        this.CommandData.MessageCoordinates[1] = (float)this.MessageY.Value;
    }

    public BiDict<string, uint> ShotTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Close-Up (Face)",       0},
            {"Bust-Up",               1},
            {"Whole Body",            2},
            {"Overhead / Bird's Eye", 3},
        }
    );

    public BiDict<string, uint> AngleTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Front Right Cheek", 0},
            {"Front Direct",      1},
            {"Front Left Cheek",  2},
            {"Back Right Cheek",  3},
            {"Back Direct",       4},
            {"Back Left Cheek",   5},
        }
    );

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
