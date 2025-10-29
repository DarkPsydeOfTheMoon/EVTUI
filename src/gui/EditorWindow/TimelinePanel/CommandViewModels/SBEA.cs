using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class SBEA : Generic
{
    public SBEA(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Sounds: Field Noise (All)";
        this.ActionType = new StringSelectionField("Action", this.Editable, this.ActionTypes.Backward[this.CommandData.Action], this.ActionTypes.Keys);
        this.WhenAnyValue(_ => _.ActionType.Choice).Subscribe(_ => this.CommandData.Action = this.ActionTypes.Forward[this.ActionType.Choice]);
    }

    public StringSelectionField ActionType { get; set; }

    public BiDict<string, int> ActionTypes = new BiDict<string, int>
    (
        new Dictionary<string, int>
        {
            {"None", 0},
            {"Play", 1},
            {"Stop", 2},
        }
    );
}
