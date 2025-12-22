using System;
using System.Collections.Generic;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class MAB_ : Generic
{
    public MAB_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Model: Base Animation";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.StartWaitingFrames = new NumEntryField("Frame Delay", this.Editable, this.CommandData.StartWaitingFrames, 0, 20, 1);
        this.WhenAnyValue(_ => _.StartWaitingFrames.Value).Subscribe(_ => this.CommandData.StartWaitingFrames = (uint)this.StartWaitingFrames.Value);

        // animations
        this.FirstAnimation = new AnimationWidget(config, commonVMs, this.AssetID, this.CommandData.FirstAnimation, this.CommandData.Flags, $"First Animation", extInd:4);
        this.SecondAnimation = new AnimationWidget(config, commonVMs, this.AssetID, this.CommandData.SecondAnimation, this.CommandData.Flags, $"Second Animation", enabledInd:0, extInd:5, frameBlendingInd:1, enabledFlip:true);

        // debug
        this.DebugUnkBool = new BoolChoiceField("Unknown (Debug)", this.Editable, this.CommandData.Flags[30]);
        this.WhenAnyValue(_ => _.DebugUnkBool.Value).Subscribe(_ => this.CommandData.Flags[30] = this.DebugUnkBool.Value);
        this.DebugFrameForward = new BoolChoiceField("Frame Forward", this.Editable, this.CommandData.Flags[31]);
        this.WhenAnyValue(_ => _.DebugFrameForward.Value).Subscribe(_ => this.CommandData.Flags[31] = this.DebugFrameForward.Value);

        // unknown
        this.FirstAnimationUnk = new NumEntryField("Unknown (First Animation)", this.Editable, this.CommandData.FirstAnimationUnkFrames, 0, 100, 1);
        this.WhenAnyValue(_ => _.FirstAnimationUnk.Value).Subscribe(_ => this.CommandData.FirstAnimationUnkFrames = (ushort)this.FirstAnimationUnk.Value);
        this.SecondAnimationUnk = new NumEntryField("Unknown (Second Animation)", this.Editable, this.CommandData.SecondAnimationUnkFrames, 0, 100, 1);
        this.WhenAnyValue(_ => _.SecondAnimationUnk.Value).Subscribe(_ => this.CommandData.SecondAnimationUnkFrames = (ushort)this.SecondAnimationUnk.Value);
        this.UnkBool1 = new BoolChoiceField("Unknown #1", this.Editable, this.CommandData.Flags[6]);
        this.WhenAnyValue(_ => _.UnkBool1.Value).Subscribe(_ => this.CommandData.Flags[6]  = this.UnkBool1.Value);
        this.UnkBool2 = new BoolChoiceField("Unknown #2", this.Editable, this.CommandData.Flags[7]);
        this.WhenAnyValue(_ => _.UnkBool2.Value).Subscribe(_ => this.CommandData.Flags[7]  = this.UnkBool2.Value);
        this.UnkBool3 = new BoolChoiceField("Unknown #3", this.Editable, this.CommandData.Flags[8]);
        this.WhenAnyValue(_ => _.UnkBool3.Value).Subscribe(_ => this.CommandData.Flags[8]  = this.UnkBool3.Value);
        this.UnkBool4 = new BoolChoiceField("Unknown #4", this.Editable, this.CommandData.Flags[9]);
        this.WhenAnyValue(_ => _.UnkBool4.Value).Subscribe(_ => this.CommandData.Flags[9]  = this.UnkBool4.Value);
        this.UnkBool5 = new BoolChoiceField("Unknown #5", this.Editable, this.CommandData.Flags[10]);
        this.WhenAnyValue(_ => _.UnkBool5.Value).Subscribe(_ => this.CommandData.Flags[10]  = this.UnkBool5.Value);
        this.UnkBool6 = new BoolChoiceField("Unknown #6", this.Editable, this.CommandData.Flags[11]);
        this.WhenAnyValue(_ => _.UnkBool6.Value).Subscribe(_ => this.CommandData.Flags[11]  = this.UnkBool6.Value);
        this.UnkBool7 = new BoolChoiceField("Unknown #7", this.Editable, this.CommandData.Flags[12]);
        this.WhenAnyValue(_ => _.UnkBool7.Value).Subscribe(_ => this.CommandData.Flags[12]  = this.UnkBool7.Value);
    }

    public IntSelectionField AssetID { get; set; }

    public NumEntryField StartWaitingFrames { get; set; }

    // animations
    public AnimationWidget FirstAnimation  { get; set; }
    public AnimationWidget SecondAnimation { get; set; }

    // debug
    public BoolChoiceField DebugUnkBool      { get; set; }
    public BoolChoiceField DebugFrameForward { get; set; }

    // unknown
    public NumEntryField FirstAnimationUnk  { get; set; }
    public NumEntryField SecondAnimationUnk { get; set; }
    public BoolChoiceField UnkBool1 { get; set; }
    public BoolChoiceField UnkBool2 { get; set; }
    public BoolChoiceField UnkBool3 { get; set; }
    public BoolChoiceField UnkBool4 { get; set; }
    public BoolChoiceField UnkBool5 { get; set; }
    public BoolChoiceField UnkBool6 { get; set; }
    public BoolChoiceField UnkBool7 { get; set; }
}
