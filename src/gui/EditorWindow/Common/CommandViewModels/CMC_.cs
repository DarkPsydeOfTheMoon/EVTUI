using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.Utils;

namespace EVTUI.ViewModels.TimelineCommands;

public class CMC_ : Generic
{
    public CMC_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Camera: Character-Centric Movement";

        // asset/angle/whatever
        // this is technically 0-999, and also model assets only, so... TODO either way
        this.AssetID = new IntSelectionField("Model Asset ID", this.Editable, this.CommandData.AssetId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.CommandData.AssetId = this.AssetID.Choice);
        this.ShotType = new StringSelectionField("Shot Type", this.Editable, CMC_.ShotTypes.Backward[this.CommandData.ShotType], CMC_.ShotTypes.Keys);
        this.WhenAnyValue(_ => _.ShotType.Choice).Subscribe(_ => this.CommandData.ShotType = CMC_.ShotTypes.Forward[this.ShotType.Choice]);
        this.AngleType = new StringSelectionField("Angle Type", this.Editable, CMC_.AngleTypes.Backward[this.CommandData.AngleType], CMC_.AngleTypes.Keys);
        this.WhenAnyValue(_ => _.AngleType.Choice).Subscribe(_ => this.CommandData.AngleType = CMC_.AngleTypes.Forward[this.AngleType.Choice]);
        this.StartCorrectionFrame = new NumRangeField("Start Correction At Frame", this.Editable, this.CommandData.StartCorrectionFrameNumber, 0, 60, 1);
        this.WhenAnyValue(_ => _.StartCorrectionFrame.Value).Subscribe(_ => this.CommandData.StartCorrectionFrameNumber = (ushort)this.StartCorrectionFrame.Value);

        // interpolation
        this.InterpolationSettings = new InterpolationParameters(this.CommandData.InterpolationParameters, this.Editable);
        this.WhenAnyValue(_ => _.InterpolationSettings.InterpolationType.Choice, _ => _.InterpolationSettings.SlopeInType.Choice, _ => _.InterpolationSettings.SlopeOutType.Choice).Subscribe(_ => this.CommandData.InterpolationParameters = this.InterpolationSettings.Compose());

        // focus/blur
        this.EnableDOF = new BoolChoiceField("Enable Depth-Of-Field?", this.Editable, this.CommandData.Flags[0]);
        this.WhenAnyValue(_ => _.EnableDOF.Value).Subscribe(_ => this.CommandData.Flags[0] = this.EnableDOF.Value);
        this.EnableCustomDOF = new BoolChoiceField("Customize Depth-Of-Field?", this.Editable, this.CommandData.Flags[1]);
        this.WhenAnyValue(_ => _.EnableCustomDOF.Value).Subscribe(_ => this.CommandData.Flags[1] = this.EnableCustomDOF.Value);
        this.FocalDistance = new NumRangeField("Focal Distance", this.Editable, this.CommandData.FocalPlaneDistance, 0, 999999, 1);
        this.WhenAnyValue(_ => _.FocalDistance.Value).Subscribe(_ => this.CommandData.FocalPlaneDistance = (float)this.FocalDistance.Value);
        this.NearBlurDistance = new NumRangeField("Near Blur Distance", this.Editable, this.CommandData.NearBlurSurface, 0, 999999, 1);
        this.WhenAnyValue(_ => _.NearBlurDistance.Value).Subscribe(_ => this.CommandData.NearBlurSurface = (float)this.NearBlurDistance.Value);
        this.FarBlurDistance = new NumRangeField("Far Blur Distance", this.Editable, this.CommandData.FarBlurSurface, 0, 999999, 1);
        this.WhenAnyValue(_ => _.FarBlurDistance.Value).Subscribe(_ => this.CommandData.FarBlurSurface = (float)this.FarBlurDistance.Value);
        this.BlurStrength = new NumRangeField("Blur Strength", this.Editable, this.CommandData.BlurStrength, 0.5, 1, 0.01);
        this.WhenAnyValue(_ => _.BlurStrength.Value).Subscribe(_ => this.CommandData.BlurStrength = (float)this.BlurStrength.Value);
        this.BlurType = new StringSelectionField("Blur Type", this.Editable, Generic.BlurTypes.Backward[this.CommandData.BlurType], Generic.BlurTypes.Keys);
        this.WhenAnyValue(_ => _.BlurType).Subscribe(_ => this.CommandData.BlurType = Generic.BlurTypes.Forward[this.BlurType.Choice]);

        // message
        this.EnableMessageCoordinates = new BoolChoiceField("Directly Specify Message Coordinates?", this.Editable, this.CommandData.Flags[5]);
        this.WhenAnyValue(_ => _.EnableMessageCoordinates.Value).Subscribe(_ => this.CommandData.Flags[5] = this.EnableMessageCoordinates.Value);
        this.MessageCoordinateType = new StringSelectionField("Coordinate Type", this.Editable, Generic.MessageCoordinateTypes.Backward[this.CommandData.MessageCoordinateType], Generic.MessageCoordinateTypes.Keys);
        this.WhenAnyValue(_ => _.MessageCoordinateType.Choice).Subscribe(_ => this.CommandData.MessageCoordinateType = Generic.MessageCoordinateTypes.Forward[this.MessageCoordinateType.Choice]);
        this.MessageX = new NumRangeField("X Coordinate", this.Editable, this.CommandData.MessageCoordinates[0], -9999, 9999, 1);
        this.WhenAnyValue(_ => _.MessageX.Value).Subscribe(_ => this.CommandData.MessageCoordinates[0] = (float)this.MessageX.Value);
        this.MessageY = new NumRangeField("Y Coordinate", this.Editable, this.CommandData.MessageCoordinates[1], -9999, 9999, 1);
        this.WhenAnyValue(_ => _.MessageY.Value).Subscribe(_ => this.CommandData.MessageCoordinates[1] = (float)this.MessageY.Value);
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

    public static BiDict<string, uint> ShotTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Close-Up (Face)",       0},
            {"Bust-Up",               1},
            {"Whole Body",            2},
            {"Overhead / Bird's Eye", 3},
        }
    );

    public static BiDict<string, uint> AngleTypes = new BiDict<string, uint>
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
}
