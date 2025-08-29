namespace EVTUI.ViewModels.TimelineCommands;

public class EScl : Generic
{
    public EScl(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Effect: Scale";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDsOfType(0x01000002));

        this.Scale = new NumRangeField("Scale", this.Editable, this.CommandData.Scale, 0, 100, 0.1);
        this.InterpolationSettings = new InterpolationParameters(this.CommandData.InterpolationParameters, this.Editable);
    }

    public IntSelectionField AssetID { get; set; }

    public NumRangeField           Scale                 { get; set; }
    public InterpolationParameters InterpolationSettings { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        this.CommandData.Scale  = (float)this.Scale.Value;
        this.CommandData.InterpolationParameters = this.InterpolationSettings.Compose();
    }
}
