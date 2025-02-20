using System.Collections.Generic;

namespace EVTUI.ViewModels.TimelineCommands;

public class MAB_ : Generic
{
    public MAB_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Base Animation";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);

        this.StartWaitingFrames = new NumEntryField("Frame Delay", this.Editable, this.CommandData.StartWaitingFrames, 0, 20, 1);

        // animations
        this.FirstAnimation = new AnimationWidget(config, this.AssetID, this.CommandData.FirstAnimation, this.CommandData.Flags, $"First Animation", extInd:4);
        this.SecondAnimation = new AnimationWidget(config, this.AssetID, this.CommandData.SecondAnimation, this.CommandData.Flags, $"Second Animation", enabledInd:0, extInd:5, frameBlendingInd:1, enabledFlip:true);

        // debug
        this.DebugUnkBool = new BoolChoiceField("Unknown (Debug)", this.Editable, this.CommandData.Flags[30]);
        this.DebugFrameForward = new BoolChoiceField("Frame Forward", this.Editable, this.CommandData.Flags[31]);

        // unknown
        this.FirstAnimationUnk = new NumEntryField("Unknown (First Animation)", this.Editable, this.CommandData.FirstAnimationUnkFrames, 0, 100, 1);
        this.SecondAnimationUnk = new NumEntryField("Unknown (Second Animation)", this.Editable, this.CommandData.SecondAnimationUnkFrames, 0, 100, 1);
        this.UnkBool1 = new BoolChoiceField("Unknown #1", this.Editable, this.CommandData.Flags[6]);
        this.UnkBool2 = new BoolChoiceField("Unknown #2", this.Editable, this.CommandData.Flags[7]);
        this.UnkBool3 = new BoolChoiceField("Unknown #3", this.Editable, this.CommandData.Flags[8]);
        this.UnkBool4 = new BoolChoiceField("Unknown #4", this.Editable, this.CommandData.Flags[9]);
        this.UnkBool5 = new BoolChoiceField("Unknown #5", this.Editable, this.CommandData.Flags[10]);
        this.UnkBool6 = new BoolChoiceField("Unknown #6", this.Editable, this.CommandData.Flags[11]);
        this.UnkBool7 = new BoolChoiceField("Unknown #7", this.Editable, this.CommandData.Flags[12]);
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

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        this.FirstAnimation.SaveChanges();
        this.CommandData.FirstAnimationUnkFrames = (ushort)this.FirstAnimationUnk.Value;
        this.SecondAnimation.SaveChanges();
        this.CommandData.SecondAnimationUnkFrames = (ushort)this.SecondAnimationUnk.Value;

        this.CommandData.Flags[6]  = this.UnkBool1.Value;
        this.CommandData.Flags[7]  = this.UnkBool2.Value;
        this.CommandData.Flags[8]  = this.UnkBool3.Value;
        this.CommandData.Flags[9]  = this.UnkBool4.Value;
        this.CommandData.Flags[10] = this.UnkBool5.Value;
        this.CommandData.Flags[11] = this.UnkBool6.Value;
        this.CommandData.Flags[12] = this.UnkBool7.Value;
        this.CommandData.Flags[30] = this.DebugUnkBool.Value;
        this.CommandData.Flags[31] = this.DebugFrameForward.Value;

        this.CommandData.StartWaitingFrames = (uint)this.StartWaitingFrames.Value;
    }
}
