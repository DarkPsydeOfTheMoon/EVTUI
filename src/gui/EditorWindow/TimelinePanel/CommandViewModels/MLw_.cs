using System;

namespace EVTUI.ViewModels.TimelineCommands;

public class MLw_ : Generic
{
    public MLw_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Look Around";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);

        this.EnableUpperBodyRotation = new BoolChoiceField("Enable Upper Body Rotation?", this.Editable, this.CommandData.EnableUpperBodyRotation != 0);

        // limit angles
        this.TopLimitDegrees = new NumRangeField("Top", this.Editable, this.CommandData.TopLimitAngle, 0, 30, 1);
        this.BottomLimitDegrees = new NumRangeField("Bottom", this.Editable, this.CommandData.BottomLimitAngle, 0, 30, 1);
        this.LeftLimitDegrees = new NumRangeField("Left", this.Editable, this.CommandData.LeftLimitAngle, 0, 50, 1);
        this.RightLimitDegrees = new NumRangeField("Right", this.Editable, this.CommandData.RightLimitAngle, 0, 50, 1);

        // update interval
        this.UpdateIntervalMinimumFrames = new NumRangeField("Minimum Frames To Wait", this.Editable, this.CommandData.UpdateIntervalMinimumFrameValue, 1, 300, 1);
        this.UpdateIntervalRandomFrames = new NumRangeField("Random Frames Range", this.Editable, this.CommandData.UpdateIntervalRandomFrame, 0, 300, 1);
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

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        this.CommandData.EnableUpperBodyRotation = Convert.ToUInt32(this.EnableUpperBodyRotation.Value);

        this.CommandData.TopLimitAngle    = (float)this.TopLimitDegrees.Value;
        this.CommandData.BottomLimitAngle = (float)this.BottomLimitDegrees.Value;
        this.CommandData.LeftLimitAngle   = (float)this.LeftLimitDegrees.Value;
        this.CommandData.RightLimitAngle  = (float)this.RightLimitDegrees.Value;

        this.CommandData.UpdateIntervalMinimumFrameValue = (uint)this.UpdateIntervalMinimumFrames.Value;
        this.CommandData.UpdateIntervalRandomFrame       = (uint)this.UpdateIntervalRandomFrames.Value;
    }
}
