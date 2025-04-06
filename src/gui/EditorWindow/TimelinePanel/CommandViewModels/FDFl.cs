namespace EVTUI.ViewModels.TimelineCommands;

public class FDFl : Generic
{
    public FDFl(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Field: ???";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDsOfType(0x00000003));
        this.Unk = new NumEntryField("Unknown", this.Editable, this.CommandData.Unk1, 0, null, 1);

    }

    public IntSelectionField AssetID { get; set; }
    public NumEntryField     Unk     { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;
        this.CommandData.Unk1 = (uint)this.Unk.Value;
    }
}
