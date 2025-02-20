using System;
using System.Collections.Generic;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class MRot : Generic
{
    public MRot(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Rotation";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);

        // rotation
        this.InterpolationType = new StringSelectionField("Interpolation Type", this.Editable, this.InterpolationTypes.Backward[this.CommandData.InterpolationType], this.InterpolationTypes.Keys);
        this.YawEnabled = new BoolChoiceField("Enable Yaw?", this.Editable, this.CommandData.Flags[0]);
        this.YawDegrees = new NumRangeField("Yaw", this.Editable, this.CommandData.Rotation[0], -180, 180, 1);
        this.PitchEnabled = new BoolChoiceField("Enable Pitch?", this.Editable, this.CommandData.Flags[1]);
        this.PitchDegrees = new NumRangeField("Pitch", this.Editable, this.CommandData.Rotation[1], -180, 180, 1);
        this.RollEnabled = new BoolChoiceField("Enable Roll?", this.Editable, this.CommandData.Flags[2]);
        this.RollDegrees = new NumRangeField("Roll", this.Editable, this.CommandData.Rotation[2], -180, 180, 1);
        this.CustomRotationAnimationsEnabled = new BoolChoiceField("Customize Rotation Animations?", this.Editable, this.CommandData.Flags[12]);

        // animations
        this.RotatingAnimation = new AnimationWidget(config, this.AssetID, this.CommandData.RotatingAnimation, this.CommandData.Flags, $"Rotating Animation", enabledInd:4, extInd:6);
        this.WaitingAnimation = new AnimationWidget(config, this.AssetID, this.CommandData.WaitingAnimation, this.CommandData.Flags, $"Idle Animation", enabledInd:5, extInd:7);

        // unknown
        this.Unk = new NumEntryField("Unknown Int", this.Editable, this.CommandData.UNK, 0, null, 1);
    }

    public IntSelectionField AssetID { get; set; }

    // rotation
    public StringSelectionField InterpolationType               { get; set; }
    public BoolChoiceField      YawEnabled                      { get; set; }
    public NumRangeField        YawDegrees                      { get; set; }
    public BoolChoiceField      PitchEnabled                    { get; set; }
    public NumRangeField        PitchDegrees                    { get; set; }
    public BoolChoiceField      RollEnabled                     { get; set; }
    public NumRangeField        RollDegrees                     { get; set; }
    public BoolChoiceField      CustomRotationAnimationsEnabled { get; set; }

    // animations
    public AnimationWidget RotatingAnimation { get; set; }
    public AnimationWidget WaitingAnimation { get; set; }

    // unknown
    public NumEntryField Unk { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        this.CommandData.Flags[0] = this.YawEnabled.Value;
        this.CommandData.Flags[1] = this.PitchEnabled.Value;
        this.CommandData.Flags[2] = this.RollEnabled.Value;
        this.CommandData.Flags[12] = this.CustomRotationAnimationsEnabled.Value;

        this.CommandData.Rotation[0] = (float)this.YawDegrees.Value;
        this.CommandData.Rotation[1] = (float)this.PitchDegrees.Value;
        this.CommandData.Rotation[2] = (float)this.RollDegrees.Value;

        this.CommandData.InterpolationType = this.InterpolationTypes.Forward[this.InterpolationType.Choice];
        this.CommandData.UNK               = (uint)this.Unk.Value;

        this.RotatingAnimation.SaveChanges();
        this.WaitingAnimation.SaveChanges();
    }

    public BiDict<string, uint> InterpolationTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Linear",                    0},
            {"Hermite (Slow-to-Slow)", 4354},
            {"Hermite (Fast-to-Slow)", 4610},
            {"Hermite (Slow-to-Fast)", 8450},
        }
    );
}
