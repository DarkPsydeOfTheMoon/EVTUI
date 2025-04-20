using System;

namespace EVTUI.ViewModels.TimelineCommands;

public class EnPh : Generic
{
    public EnPh(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Environment: Physics";

        this.Enabled = new BoolChoiceField("Enable Physics?", this.Editable, this.CommandData.Enable != 0);
    }

    public BoolChoiceField Enabled { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();

        this.CommandData.Enable = Convert.ToUInt32(this.Enabled.Value);
    }
}
