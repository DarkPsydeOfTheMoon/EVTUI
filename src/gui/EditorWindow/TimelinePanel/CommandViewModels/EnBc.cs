namespace EVTUI.ViewModels.TimelineCommands;

public class EnBc : Generic
{
    public EnBc(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Environment: Background Color";
        this.BackgroundColor = new ColorSelectionField("Background Color", this.Editable, this.CommandData.RGBA);
    }

    public ColorSelectionField BackgroundColor    { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.CommandData.RGBA = this.BackgroundColor.ToUInt32();
    }
}
