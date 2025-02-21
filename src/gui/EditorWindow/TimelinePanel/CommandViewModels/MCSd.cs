namespace EVTUI.ViewModels.TimelineCommands;

public class MCSd : Generic
{
    public MCSd(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Shadow Color";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);

        // inner circle
        this.InnerCircleColor = new ColorSelectionField("Color", this.Editable, this.CommandData.InnerCircleRGBA);
        this.InnerCircleDiameter = new NumEntryField("Diameter", this.Editable, this.CommandData.InnerCircleDiameter, 0, 99, 1);

        // outer circle
        this.OuterCircleColor = new ColorSelectionField("Color", this.Editable, this.CommandData.OuterCircleRGBA);
        this.OuterCircleDiameter = new NumEntryField("Diameter", this.Editable, this.CommandData.OuterCircleDiameter, 0, 99, 1);

        // unknown :')
        this.UnkEnum = new NumEntryField("Unknown", this.Editable, this.CommandData.UnkEnum, 0, 3, 1);
    }

    public IntSelectionField AssetID { get; set; }

    // inner circle
    public ColorSelectionField InnerCircleColor    { get; set; }
    public NumEntryField       InnerCircleDiameter { get; set; }

    // outer circle
    public ColorSelectionField OuterCircleColor    { get; set; }
    public NumEntryField       OuterCircleDiameter { get; set; }

    // unknown :')
    public NumEntryField       UnkEnum   { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        this.CommandData.UnkEnum = (uint)this.UnkEnum.Value;

        this.CommandData.InnerCircleRGBA = this.InnerCircleColor.ToUInt32();
        this.CommandData.OuterCircleRGBA = this.OuterCircleColor.ToUInt32();

        this.CommandData.InnerCircleDiameter = (ushort)this.InnerCircleDiameter.Value;
        this.CommandData.OuterCircleDiameter = (ushort)this.OuterCircleDiameter.Value;
    }
}
