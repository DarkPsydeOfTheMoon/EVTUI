using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class EnPh : Generic
{
    public EnPh(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Environment: Physics";

        this.Enabled = new BoolChoiceField("Enable Physics?", this.Editable, this.CommandData.Enable != 0);
        this.WhenAnyValue(_ => _.Enabled.Value).Subscribe(_ => this.CommandData.Enable = Convert.ToUInt32(this.Enabled.Value));
    }

    public BoolChoiceField Enabled { get; set; }
}
