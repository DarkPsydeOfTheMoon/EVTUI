using System.Collections.Generic;

namespace EVTUI.ViewModels.TimelineCommands;

public class MAA_ : Generic
{
    public MAA_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Additive Animation";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);

        this.TrackNumber = new IntSelectionField("Track Number", this.Editable, this.CommandData.TrackNumber+1, new List<int> {1, 2, 3, 4, 5, 6, 7, 8});
        this.AddAnimation = new AnimationWidget(config, this.AssetID, this.CommandData.AddAnimation, this.CommandData.Flags, $"Animation", extInd:0, trackNum:this.TrackNumber.Choice);
        this.DebugFrameForward = new BoolChoiceField("Frame Forward", this.Editable, this.CommandData.Flags[31]);

    }

    public IntSelectionField AssetID           { get; set; }
    public IntSelectionField TrackNumber       { get; set; }
    public AnimationWidget   AddAnimation      { get; set; }
    public BoolChoiceField   DebugFrameForward { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        this.CommandData.TrackNumber = this.TrackNumber.Choice-1;
        this.AddAnimation.SaveChanges();
        this.CommandData.Flags[31] = this.DebugFrameForward.Value;
    }
}
