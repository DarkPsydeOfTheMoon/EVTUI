namespace EVTUI.ViewModels.TimelineCommands;

public class FrJ_ : Generic
{
    public FrJ_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Frame Jump";
        this.FrameIndex = new NumRangeField("Index", this.Editable, (int)this.CommandData.JumpToFrame, 0, config.EventManager.EventDuration, 1);
    }

    public NumRangeField FrameIndex { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.CommandData.JumpToFrame = (uint)this.FrameIndex.Value;
    }
}
