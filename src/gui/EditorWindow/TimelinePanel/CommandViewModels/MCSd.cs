namespace EVTUI.ViewModels.TimelineCommands;

public class MCSd : Generic
{
    public MCSd(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Shadow Color";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);

        // unknown :')
        this.UnkEnum = new NumEntryField("Unknown #1", this.Editable, this.CommandData.UnkEnum, 0, 3, 1);
        this.UnkColor1 = new ColorSelectionField("Unknown #2", this.Editable, this.CommandData.RGBA1);
        this.UnkColor2 = new ColorSelectionField("Unknown #3", this.Editable, this.CommandData.RGBA2);
        this.UnkInd1 = new NumEntryField("Unknown #4", this.Editable, this.CommandData.UnkInd1, 0, 99, 1);
        this.UnkInd2 = new NumEntryField("Unknown #5", this.Editable, this.CommandData.UnkInd2, 0, 99, 1);
    }

    public IntSelectionField AssetID { get; set; }

    // unknown :')
    public NumEntryField       UnkEnum   { get; set; }
    public ColorSelectionField UnkColor1 { get; set; }
    public ColorSelectionField UnkColor2 { get; set; }
    public NumEntryField       UnkInd1   { get; set; }
    public NumEntryField       UnkInd2   { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        this.CommandData.UnkEnum = (uint)this.UnkEnum.Value;

        this.CommandData.RGBA1[0] = this.UnkColor1.SelectedColor.R;
        this.CommandData.RGBA1[1] = this.UnkColor1.SelectedColor.G;
        this.CommandData.RGBA1[2] = this.UnkColor1.SelectedColor.B;
        this.CommandData.RGBA1[3] = this.UnkColor1.SelectedColor.A;

        this.CommandData.RGBA2[0] = this.UnkColor2.SelectedColor.R;
        this.CommandData.RGBA2[1] = this.UnkColor2.SelectedColor.G;
        this.CommandData.RGBA2[2] = this.UnkColor2.SelectedColor.B;
        this.CommandData.RGBA2[3] = this.UnkColor2.SelectedColor.A;

        this.CommandData.UnkInd1 = (ushort)this.UnkInd1.Value;
        this.CommandData.UnkInd2 = (ushort)this.UnkInd2.Value;
    }
}
