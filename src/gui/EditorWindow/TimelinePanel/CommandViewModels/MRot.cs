using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class MRot : Generic
{
    public MRot(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Model: Rotation";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        // rotation
        this.InterpolationType = new StringSelectionField("Interpolation Type", this.Editable, this.InterpolationTypes.Backward[this.CommandData.InterpolationType], this.InterpolationTypes.Keys);
        this.WhenAnyValue(_ => _.InterpolationType.Choice).Subscribe(_ => this.CommandData.InterpolationType = this.InterpolationTypes.Forward[this.InterpolationType.Choice]);
        this.YawEnabled = new BoolChoiceField("Enable Yaw?", this.Editable, this.CommandData.Flags[0]);
        this.WhenAnyValue(_ => _.YawEnabled.Value).Subscribe(_ => this.CommandData.Flags[0] = this.YawEnabled.Value);
        this.YawDegrees = new NumRangeField("Yaw", this.Editable, this.CommandData.Rotation[0], -180, 180, 1);
        this.WhenAnyValue(_ => _.YawDegrees).Subscribe(_ => this.CommandData.Rotation[0] = (float)this.YawDegrees.Value);
        this.PitchEnabled = new BoolChoiceField("Enable Pitch?", this.Editable, this.CommandData.Flags[1]);
        this.WhenAnyValue(_ => _.PitchEnabled.Value).Subscribe(_ => this.CommandData.Flags[1] = this.PitchEnabled.Value);
        this.PitchDegrees = new NumRangeField("Pitch", this.Editable, this.CommandData.Rotation[1], -180, 180, 1);
        this.WhenAnyValue(_ => _.PitchDegrees).Subscribe(_ => this.CommandData.Rotation[1] = (float)this.PitchDegrees.Value);
        this.RollEnabled = new BoolChoiceField("Enable Roll?", this.Editable, this.CommandData.Flags[2]);
        this.WhenAnyValue(_ => _.RollEnabled.Value).Subscribe(_ => this.CommandData.Flags[2] = this.RollEnabled.Value);
        this.RollDegrees = new NumRangeField("Roll", this.Editable, this.CommandData.Rotation[2], -180, 180, 1);
        this.WhenAnyValue(_ => _.RollDegrees).Subscribe(_ => this.CommandData.Rotation[2] = (float)this.RollDegrees.Value);
        this.CustomRotationAnimationsEnabled = new BoolChoiceField("Customize Rotation Animations?", this.Editable, this.CommandData.Flags[12]);
        this.WhenAnyValue(_ => _.CustomRotationAnimationsEnabled.Value).Subscribe(_ => this.CommandData.Flags[12] = this.CustomRotationAnimationsEnabled.Value);

        // animations
        this.RotatingAnimation = new AnimationWidget(config, this.AssetID, this.CommandData.RotatingAnimation, this.CommandData.Flags, $"Rotating Animation", enabledInd:4, extInd:6);
        this.WaitingAnimation = new AnimationWidget(config, this.AssetID, this.CommandData.WaitingAnimation, this.CommandData.Flags, $"Idle Animation", enabledInd:5, extInd:7);

        // unknown
        this.Unk = new NumEntryField("Unknown Int", this.Editable, this.CommandData.UNK, 0, null, 1);
        this.WhenAnyValue(_ => _.Unk.Value).Subscribe(_ => this.CommandData.UNK = (uint)this.Unk.Value);
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
