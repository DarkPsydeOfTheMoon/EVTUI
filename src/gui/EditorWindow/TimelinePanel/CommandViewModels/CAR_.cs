using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class CAR_ : Generic
{
    public CAR_(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Camera: Reset Animation";

        this.ResetCommand = new BoolChoiceField("Reset Command?", this.Editable, this.CommandData.Flags[0]);
        this.WhenAnyValue(_ => _.ResetCommand.Value).Subscribe(_ => this.CommandData.Flags[0] = this.ResetCommand.Value);
        this.ResetParameters = new BoolChoiceField("Reset Parameters?", this.Editable, this.CommandData.Flags[1]);
        this.WhenAnyValue(_ => _.ResetParameters.Value).Subscribe(_ => this.CommandData.Flags[1] = this.ResetParameters.Value);
    }

    public BoolChoiceField ResetCommand    { get; set; }
    public BoolChoiceField ResetParameters { get; set; }
}
