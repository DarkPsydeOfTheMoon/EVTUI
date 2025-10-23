namespace EVTUI.ViewModels.TimelineCommands;

public class ELd_ : Generic
{
    public ELd_(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Effect: Load";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDsOfType(0x01000002));
    }

    public IntSelectionField AssetID { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;
    }
}
