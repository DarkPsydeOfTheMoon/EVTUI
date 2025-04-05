namespace EVTUI.ViewModels.TimelineCommands;

public class CAR_ : Generic
{
    public CAR_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Camera: Reset Animation";

        this.ResetCommand = new BoolChoiceField("Reset Command?", this.Editable, this.CommandData.Flags[0]);
        this.ResetParameters = new BoolChoiceField("Reset Parameters?", this.Editable, this.CommandData.Flags[1]);
    }

    public BoolChoiceField ResetCommand    { get; set; }
    public BoolChoiceField ResetParameters { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();

        this.CommandData.Flags[0] = this.ResetCommand.Value;
        this.CommandData.Flags[1] = this.ResetParameters.Value;
    }
}
