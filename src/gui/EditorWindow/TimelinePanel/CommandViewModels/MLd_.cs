namespace EVTUI.ViewModels.TimelineCommands;

public class MLd_ : Generic
{
    public MLd_(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Model: Load";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
    }

    public IntSelectionField AssetID { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;
    }
}
