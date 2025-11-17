using System;
using System.Collections.Generic;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class CSA_ : Generic
{
    public CSA_(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Camera: Base Animation";

        // animation source
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.CommandData.AssetId, config.EventManager.AssetIDsOfType(0x00000007));
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.CommandData.AssetId = this.AssetID.Choice);
        this.AnimationID = new NumEntryField("Animation ID", this.Editable, this.CommandData.AnimationId, 0, 59, 1);
        this.WhenAnyValue(_ => _.AnimationID.Value).Subscribe(_ => this.CommandData.AnimationId = (uint)this.AnimationID.Value);
        this.PlaybackSpeed = new NumEntryField("Playback Speed", this.Editable, this.CommandData.PlaybackSpeed, 0, 10, 0.1);
        this.WhenAnyValue(_ => _.PlaybackSpeed.Value).Subscribe(_ => this.CommandData.PlaybackSpeed = (float)this.PlaybackSpeed.Value);
        this.StartingFrame = new NumEntryField("Starting Frame", this.Editable, this.CommandData.StartingFrame, 0, 99999, 1);
        this.WhenAnyValue(_ => _.StartingFrame.Value).Subscribe(_ => this.CommandData.StartingFrame = (uint)this.StartingFrame.Value);
        this.LoopBool = new BoolChoiceField("Loop Playback?", this.Editable, this.CommandData.Flags[0]);
        this.WhenAnyValue(_ => _.LoopBool.Value).Subscribe(_ => this.CommandData.Flags[0] = this.LoopBool.Value);
        this.EnableCorrectionParameters = new BoolChoiceField("Enable Viewport Updates?", this.Editable, this.CommandData.Flags[1]);
        this.WhenAnyValue(_ => _.EnableCorrectionParameters.Value).Subscribe(_ => this.CommandData.Flags[1] = this.EnableCorrectionParameters.Value);

        // viewport target position
        this.ViewportX = new NumRangeField("X", this.Editable, this.CommandData.ViewportCoordinates[0], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.ViewportX.Value).Subscribe(_ => this.CommandData.ViewportCoordinates[0] = (float)this.ViewportX.Value);
        this.ViewportY = new NumRangeField("Y", this.Editable, this.CommandData.ViewportCoordinates[1], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.ViewportY.Value).Subscribe(_ => this.CommandData.ViewportCoordinates[1] = (float)this.ViewportY.Value);
        this.ViewportZ = new NumRangeField("Z", this.Editable, this.CommandData.ViewportCoordinates[2], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.ViewportZ.Value).Subscribe(_ => this.CommandData.ViewportCoordinates[2] = (float)this.ViewportZ.Value);

        // viewport target rotation
        // i may have yaw and pitch switched around here, dunno
        this.ViewportYaw = new NumRangeField("Yaw", this.Editable, this.CommandData.ViewportRotation[0], -180, 180, 1);
        this.WhenAnyValue(_ => _.ViewportYaw.Value).Subscribe(_ => this.CommandData.ViewportRotation[0] = (float)this.ViewportYaw.Value);
        this.ViewportPitch = new NumRangeField("Pitch", this.Editable, this.CommandData.ViewportRotation[1], -180, 180, 1);
        this.WhenAnyValue(_ => _.ViewportPitch.Value).Subscribe(_ => this.CommandData.ViewportRotation[1] = (float)this.ViewportPitch.Value);
        this.ViewportRoll = new NumRangeField("Roll", this.Editable, this.CommandData.ViewportRotation[2], -180, 180, 1);
        this.WhenAnyValue(_ => _.ViewportRoll.Value).Subscribe(_ => this.CommandData.ViewportRotation[2] = (float)this.ViewportRoll.Value);

        // focus/blur
        this.EnableDOF = new BoolChoiceField("Enable Depth-Of-Field?", this.Editable, this.CommandData.Flags[2]);
        this.WhenAnyValue(_ => _.EnableDOF.Value).Subscribe(_ => this.CommandData.Flags[2] = this.EnableDOF.Value);
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
    }

    // animation source
    public IntSelectionField AssetID                    { get; set; }
    public NumEntryField     AnimationID                { get; set; }
    public NumEntryField     PlaybackSpeed              { get; set; }
    public NumEntryField     StartingFrame              { get; set; }
    public BoolChoiceField   LoopBool                   { get; set; }
    public BoolChoiceField   EnableCorrectionParameters { get; set; }

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
    public BoolChoiceField UnkBool { get; set; }
    public NumEntryField   UnkEnum { get; set; }
    public NumEntryField   UnkInd  { get; set; }
}
