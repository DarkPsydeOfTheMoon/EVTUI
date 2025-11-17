using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class MLw_ : Generic
{
    public MLw_(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Model: Look Around";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.EnableUpperBodyRotation = new BoolChoiceField("Enable Upper Body Rotation?", this.Editable, this.CommandData.EnableUpperBodyRotation != 0);
        this.WhenAnyValue(_ => _.EnableUpperBodyRotation.Value).Subscribe(_ => this.CommandData.EnableUpperBodyRotation = Convert.ToUInt32(this.EnableUpperBodyRotation.Value));

        // limit angles
        this.TopLimitDegrees = new NumRangeField("Top", this.Editable, this.CommandData.TopLimitAngle, 0, 30, 1);
        this.WhenAnyValue(_ => _.TopLimitDegrees.Value).Subscribe(_ => this.CommandData.TopLimitAngle = (float)this.TopLimitDegrees.Value);
        this.BottomLimitDegrees = new NumRangeField("Bottom", this.Editable, this.CommandData.BottomLimitAngle, 0, 30, 1);
        this.WhenAnyValue(_ => _.BottomLimitDegrees.Value).Subscribe(_ => this.CommandData.BottomLimitAngle = (float)this.BottomLimitDegrees.Value);
        this.LeftLimitDegrees = new NumRangeField("Left", this.Editable, this.CommandData.LeftLimitAngle, 0, 50, 1);
        this.WhenAnyValue(_ => _.LeftLimitDegrees.Value).Subscribe(_ => this.CommandData.LeftLimitAngle = (float)this.LeftLimitDegrees.Value);
        this.RightLimitDegrees = new NumRangeField("Right", this.Editable, this.CommandData.RightLimitAngle, 0, 50, 1);
        this.WhenAnyValue(_ => _.RightLimitDegrees.Value).Subscribe(_ => this.CommandData.RightLimitAngle = (float)this.RightLimitDegrees.Value);

        // update interval
        this.UpdateIntervalMinimumFrames = new NumRangeField("Minimum Frames To Wait", this.Editable, this.CommandData.UpdateIntervalMinimumFrameValue, 1, 300, 1);
        this.WhenAnyValue(_ => _.UpdateIntervalMinimumFrames.Value).Subscribe(_ => this.CommandData.UpdateIntervalMinimumFrameValue = (uint)this.UpdateIntervalMinimumFrames.Value);
        this.UpdateIntervalRandomFrames = new NumRangeField("Random Frames Range", this.Editable, this.CommandData.UpdateIntervalRandomFrame, 0, 300, 1);
        this.WhenAnyValue(_ => _.UpdateIntervalRandomFrames).Subscribe(_ => this.CommandData.UpdateIntervalRandomFrame = (uint)this.UpdateIntervalRandomFrames.Value);
    }

    public IntSelectionField AssetID { get; set; }

    public BoolChoiceField EnableUpperBodyRotation   { get; set; }

    // limit angles
    public NumRangeField TopLimitDegrees    { get; set; }
    public NumRangeField BottomLimitDegrees { get; set; }
    public NumRangeField LeftLimitDegrees   { get; set; }
    public NumRangeField RightLimitDegrees  { get; set; }

    // update interval
    public NumRangeField UpdateIntervalMinimumFrames { get; set; }
    public NumRangeField UpdateIntervalRandomFrames  { get; set; }
}
