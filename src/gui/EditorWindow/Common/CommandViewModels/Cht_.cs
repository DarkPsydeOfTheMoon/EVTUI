using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.Utils;

namespace EVTUI.ViewModels.TimelineCommands;

public class Cht_ : Generic
{
    public Cht_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Message: Chat Mode";

        this.ChatMode = new StringSelectionField("Chat Mode", this.Editable, this.ChatModes.Backward[this.CommandData.ChatMode], this.ChatModes.Keys);
        this.WhenAnyValue(_ => _.ChatMode.Choice).Subscribe(_ => this.CommandData.ChatMode = this.ChatModes.Forward[this.ChatMode.Choice]);
    }

    public StringSelectionField ChatMode { get; set; }

    public BiDict<string, uint> ChatModes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"None",        0},
            {"Send On",     1},
            {"Send Off",    2},
            {"Display On",  3},
            {"Display Off", 4},
            {"Clear",       5},
        }
    );
}
