using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.Utils;

namespace EVTUI.ViewModels.TimelineCommands;

public class SBEA : Generic
{
    public SBEA(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Sounds: Field Noise (All)";
        this.ActionType = new StringSelectionField("Action", this.Editable, Generic.AudioActionTypes.Backward[this.CommandData.Action], Generic.AudioActionTypes.Keys);
        this.WhenAnyValue(_ => _.ActionType.Choice).Subscribe(_ => this.CommandData.Action = Generic.AudioActionTypes.Forward[this.ActionType.Choice]);
    }

    public StringSelectionField ActionType { get; set; }
}
