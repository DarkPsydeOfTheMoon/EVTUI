namespace EVTUI.ViewModels.TimelineCommands;

public class MLa_ : Generic
{
    public MLa_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Look At";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.X = new NumEntryField("X", this.Editable, this.CommandData.Target[0], null, null, 0.1);
        this.Y = new NumEntryField("Y", this.Editable, this.CommandData.Target[1], null, null, 0.1);
        this.Z = new NumEntryField("Z", this.Editable, this.CommandData.Target[2], null, null, 0.1);
    }

    public IntSelectionField AssetID   { get; set; }

    public NumEntryField   X               { get; set; }
    public NumEntryField   Y               { get; set; }
    public NumEntryField   Z               { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId      = this.AssetID.Choice;
        this.CommandData.Target[0] = (float)this.X.Value;
        this.CommandData.Target[1] = (float)this.Y.Value;
        this.CommandData.Target[2] = (float)this.Z.Value;
    }
}
