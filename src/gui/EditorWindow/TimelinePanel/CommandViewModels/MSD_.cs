using System;
using System.Collections.Generic;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class MSD_ : Generic
{
    public MSD_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Placement";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);

        this.ModelPreviewVM = new ModelPreviewWidget(config, this.AssetID);

        // position
        this.X = new NumRangeField("X", this.Editable, this.CommandData.Position[0], -99999, 99999, 1);
        this.Y = new NumRangeField("Y", this.Editable, this.CommandData.Position[1], -99999, 99999, 1);
        this.Z = new NumRangeField("Z", this.Editable, this.CommandData.Position[2], -99999, 99999, 1);

        // rotation
        this.RotationEnabled = new BoolChoiceField("Enabled?", this.Editable, !this.CommandData.Flags[1]);
        this.PitchDegrees = new NumRangeField("Pitch", this.Editable, this.CommandData.Rotation[0], -180, 180, 1);
        this.YawDegrees = new NumRangeField("Yaw", this.Editable, this.CommandData.Rotation[1], -180, 180, 1);
        this.RollDegrees = new NumRangeField("Roll", this.Editable, this.CommandData.Rotation[2], -180, 180, 1);

        // waiting animation
        this.WaitingAnimation = new AnimationWidget(config, this.AssetID, this.CommandData.WaitingAnimation, this.CommandData.Flags, $"Idle Animation", enabledInd:0, extInd:2, enabledFlip:true);
    }

    public IntSelectionField AssetID { get; set; }
    public ModelPreviewWidget ModelPreviewVM { get; set; }

    // position
    public NumRangeField X { get; set; }
    public NumRangeField Y { get; set; }
    public NumRangeField Z { get; set; }

    // rotation
    public BoolChoiceField RotationEnabled { get; set; }
    public NumRangeField   PitchDegrees    { get; set; }
    public NumRangeField   YawDegrees      { get; set; }
    public NumRangeField   RollDegrees     { get; set; }

    // waiting animation
    public AnimationWidget WaitingAnimation { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        this.CommandData.Position[0] = (float)this.X.Value;
        this.CommandData.Position[1] = (float)this.Y.Value;
        this.CommandData.Position[2] = (float)this.Z.Value;

        this.CommandData.Rotation[0] = (float)this.PitchDegrees.Value;
        this.CommandData.Rotation[1] = (float)this.YawDegrees.Value;
        this.CommandData.Rotation[2] = (float)this.RollDegrees.Value;

        this.WaitingAnimation.SaveChanges();

        this.CommandData.Flags[1] = !this.RotationEnabled.Value;
    }
}
