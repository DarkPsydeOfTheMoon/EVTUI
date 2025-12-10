using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ReactiveUI;

using static EVTUI.Utils;

namespace EVTUI.ViewModels.TimelineCommands;

public class SBE_ : Generic
{
    public SBE_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Sounds: Field Noise";

        this.ActionType = new StringSelectionField("Action", this.Editable, this.ActionTypes.Backward[this.CommandData.Action], this.ActionTypes.Keys);
        this.WhenAnyValue(_ => _.ActionType.Choice).Subscribe(_ => this.CommandData.Action = this.ActionTypes.Forward[this.ActionType.Choice]);

        config.AudioManager.SetActiveACBType("Field");
        this.CueID = new IntSelectionField("Cue ID", this.Editable, (config.AudioManager.CueIds.Contains((uint)this.CommandData.CueId)) ? (int)this.CommandData.CueId : 0, config.AudioManager.CueIds.ConvertAll(x => (int)x));
        this.WhenAnyValue(_ => _.CueID.Choice).Subscribe(_ => this.CommandData.CueId = (uint)this.CueID.Choice);

        this.Unk = new BoolChoiceField("Unknown", this.Editable, this.CommandData.Enable != 0);
        this.WhenAnyValue(_ => _.Unk.Value).Subscribe(_ => this.CommandData.Enable = Convert.ToInt32(this.Unk.Value));
    }

    public StringSelectionField ActionType { get; set; }
    public IntSelectionField    CueID      { get; set; }
    public BoolChoiceField      Unk        { get; set; }

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
