namespace EVTUI.ViewModels.TimelineCommands;

public class MScl : Generic
{
    public MScl(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Scale";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);

        this.Scale = new NumRangeField("Scale", this.Editable, this.CommandData.Scale, 0, 3, 0.1);
    }

    public IntSelectionField AssetID { get; set; }
    public NumRangeField     Scale   { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;
        this.CommandData.Scale  = (float)this.Scale.Value;
    }
}
