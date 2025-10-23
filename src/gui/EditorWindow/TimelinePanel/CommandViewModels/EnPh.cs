using System;

namespace EVTUI.ViewModels.TimelineCommands;

public class EnPh : Generic
{
    public EnPh(DataManager config, CommandPointer cmd) : base(config, cmd)
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
