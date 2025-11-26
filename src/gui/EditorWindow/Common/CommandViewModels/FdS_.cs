using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class FdS_ : Generic
{
    public FdS_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Fade (Simple)";

        this.FadeType = new StringSelectionField("Fade Type", this.Editable, this.BasicFadeTypes.Backward[this.CommandData.FadeType], this.BasicFadeTypes.Keys);
        this.WhenAnyValue(_ => _.FadeType.Choice).Subscribe(_ => this.CommandData.FadeType = this.BasicFadeTypes.Forward[this.FadeType.Choice]);
        this.UnkBool = new BoolChoiceField("Unknown", this.Editable, this.CommandData.UnkBool != 0);
        this.WhenAnyValue(_ => _.UnkBool.Value).Subscribe(_ => this.CommandData.UnkBool = Convert.ToByte(this.UnkBool.Value));
    }

    public StringSelectionField FadeType { get; set; }
    public BoolChoiceField      UnkBool  { get; set; }

    public BiDict<string, uint> BasicFadeTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"None",             0},
            {"Fade-In (Black)",  1},
            {"Fade-Out (Black)", 2},
            {"Fade-In (White)",  3},
            {"Fade-Out (White)", 4},
        }
    );
}
