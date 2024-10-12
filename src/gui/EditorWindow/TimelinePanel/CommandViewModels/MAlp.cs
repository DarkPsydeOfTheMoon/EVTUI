namespace EVTUI.ViewModels.TimelineCommands;

public class MAlp : Generic
{
    public MAlp(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Set Opacity";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.AlphaLevel = new NumEntryField("Alpha Level", this.Editable, this.CommandData.AlphaLevel, 0, 255, 1);
    }

    public IntSelectionField AssetID    { get; set; }
    public NumEntryField     AlphaLevel { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId       = this.AssetID.Choice;
        this.CommandData.AlphaLevel = (byte)this.AlphaLevel.Value;
    }
}
