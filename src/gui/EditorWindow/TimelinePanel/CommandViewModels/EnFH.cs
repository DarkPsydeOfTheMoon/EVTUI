namespace EVTUI.ViewModels.TimelineCommands;

public class EnFH : Generic
{
    public EnFH(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Environment: Fog Height";

        this.FogColor = new ColorSelectionField("Fog Color", this.Editable, this.CommandData.RGBA);

        // distance/range
        this.StartHeight = new NumRangeField("Start", this.Editable, this.CommandData.StartHeight, -999999, 999999, 1);
        this.EndHeight = new NumRangeField("End", this.Editable, this.CommandData.EndHeight, -999999, 999999, 1);
    }

    public ColorSelectionField  FogColor    { get; set; }

    // distance/range
    public NumRangeField        StartHeight { get; set; }
    public NumRangeField        EndHeight   { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();

        this.CommandData.StartHeight = (float)this.StartHeight.Value;
        this.CommandData.EndHeight   = (float)this.EndHeight.Value;
        this.CommandData.RGBA        = this.FogColor.ToUInt32();
    }
}
