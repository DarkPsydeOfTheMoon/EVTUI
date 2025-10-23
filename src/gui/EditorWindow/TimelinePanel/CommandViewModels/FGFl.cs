namespace EVTUI.ViewModels.TimelineCommands;

public class FGFl : Generic
{
    public FGFl(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Field: ???";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDsOfType(0x00000003));
        this.Unk = new NumEntryField("Unknown", this.Editable, this.CommandData.Unk, 0, 2, 1);

    }

    public IntSelectionField AssetID { get; set; }
    public NumEntryField     Unk     { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;
        this.CommandData.Unk = (uint)this.Unk.Value;
    }
}
