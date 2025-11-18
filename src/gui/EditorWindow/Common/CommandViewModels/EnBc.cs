using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class EnBc : Generic
{
    public EnBc(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Environment: Background Color";
        this.BackgroundColor = new ColorSelectionField("Background Color", this.Editable, this.CommandData.RGBA);
        this.WhenAnyValue(_ => _.BackgroundColor.SelectedColor).Subscribe(_ => this.CommandData.RGBA = this.BackgroundColor.ToUInt32());
    }

    public ColorSelectionField BackgroundColor    { get; set; }
}
