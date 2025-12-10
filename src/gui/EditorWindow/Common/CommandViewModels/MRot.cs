using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.Utils;

namespace EVTUI.ViewModels.TimelineCommands;

public class MRot : Generic
{
    public MRot(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Model: Rotation";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.InterpolationType = new StringSelectionField("Interpolation Type", this.Editable, this.InterpolationTypes.Backward[this.CommandData.InterpolationType], this.InterpolationTypes.Keys);
        this.WhenAnyValue(_ => _.InterpolationType.Choice).Subscribe(_ => this.CommandData.InterpolationType = this.InterpolationTypes.Forward[this.InterpolationType.Choice]);
        this.CustomRotationAnimationsEnabled = new BoolChoiceField("Customize Rotation Animations?", this.Editable, this.CommandData.Flags[12]);
        this.WhenAnyValue(_ => _.CustomRotationAnimationsEnabled.Value).Subscribe(_ => this.CommandData.Flags[12] = this.CustomRotationAnimationsEnabled.Value);

        // rotation
        this.Rotation = new RotationWidget(config, this.CommandData.Rotation, this.CommandData.Flags, yawInd: 0, pitchInd: 1, yawEnabledInd: 0, pitchEnabledInd: 1, rollEnabledInd: 2);

        // animations
        this.RotatingAnimation = new AnimationWidget(config, this.AssetID, this.CommandData.RotatingAnimation, this.CommandData.Flags, $"Rotating Animation", enabledInd:4, extInd:6);
        this.WaitingAnimation = new AnimationWidget(config, this.AssetID, this.CommandData.WaitingAnimation, this.CommandData.Flags, $"Idle Animation", enabledInd:5, extInd:7);

        // unknown
        this.Unk = new NumEntryField("Unknown Int", this.Editable, this.CommandData.UNK, 0, null, 1);
        this.WhenAnyValue(_ => _.Unk.Value).Subscribe(_ => this.CommandData.UNK = (uint)this.Unk.Value);
    }

    public IntSelectionField AssetID { get; set; }

    public StringSelectionField InterpolationType               { get; set; }
    public BoolChoiceField      CustomRotationAnimationsEnabled { get; set; }

    // rotation
    public RotationWidget Rotation { get; set; }

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
