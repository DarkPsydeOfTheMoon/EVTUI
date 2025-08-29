namespace EVTUI.ViewModels.TimelineCommands;

public class EAlp : Generic
{
    public EAlp(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Effect: Transparency";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDsOfType(0x01000002));

        this.AlphaLevel = new NumEntryField("Alpha Level", this.Editable, this.CommandData.RGBA[3], 0, 255, 1);
        this.InterpolationSettings = new InterpolationParameters(this.CommandData.InterpolationParameters, this.Editable);
    }

    public IntSelectionField AssetID { get; set; }

    public NumEntryField           AlphaLevel            { get; set; }
    public InterpolationParameters InterpolationSettings { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        this.CommandData.RGBA[3] = (byte)this.AlphaLevel.Value;
        this.CommandData.InterpolationParameters = this.InterpolationSettings.Compose();
    }
}
