namespace EVTUI.ViewModels.TimelineCommands;

public class CClp : Generic
{
    public CClp(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Camera: Clipping Distance";

        this.NearClip = new NumRangeField("Near Clip", this.Editable, this.CommandData.NearClip, 1, 1000, 1);
        this.FarClip = new NumRangeField("Far Clip", this.Editable, this.CommandData.FarClip, 1, 1000000, 1);
    }

    public NumRangeField NearClip { get; set; }
    public NumRangeField FarClip  { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();

        this.CommandData.NearClip = (float)this.NearClip.Value;
        this.CommandData.FarClip  = (float)this.FarClip.Value;
    }
}
