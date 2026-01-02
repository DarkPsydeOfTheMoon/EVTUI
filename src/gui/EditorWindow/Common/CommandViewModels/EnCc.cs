using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.Utils;

namespace EVTUI.ViewModels.TimelineCommands;

public class EnCc : Generic
{
    public EnCc(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Environment: Color Correction";

        this.ActionType = new StringSelectionField("Mode", this.Editable, EnCc.ActionTypes.Backward[this.CommandData.Enable], EnCc.ActionTypes.Keys);
        this.WhenAnyValue(_ => _.ActionType.Choice).Subscribe(_ => this.CommandData.Enable = EnCc.ActionTypes.Forward[this.ActionType.Choice]);

        // corrections
        this.Cyan = new NumRangeField("Cyan", this.Editable, this.CommandData.Cyan, -1, 1, 0.01);
        this.WhenAnyValue(_ => _.Cyan.Value).Subscribe(_ => this.CommandData.Cyan = (float)this.Cyan.Value);
        this.Magenta = new NumRangeField("Magenta", this.Editable, this.CommandData.Magenta, -1, 1, 0.01);
        this.WhenAnyValue(_ => _.Magenta.Value).Subscribe(_ => this.CommandData.Magenta = (float)this.Magenta.Value);
        this.Yellow = new NumRangeField("Yellow", this.Editable, this.CommandData.Yellow, -1, 1, 0.01);
        this.WhenAnyValue(_ => _.Yellow.Value).Subscribe(_ => this.CommandData.Yellow = (float)this.Yellow.Value);
        this.Dodge = new NumRangeField("Dodge", this.Editable, this.CommandData.Dodge, 0, 0.99, 0.01);
        this.WhenAnyValue(_ => _.Dodge.Value).Subscribe(_ => this.CommandData.Dodge = (float)this.Dodge.Value);
        this.Burn = new NumRangeField("Burn", this.Editable, this.CommandData.Burn, 0, 0.99, 0.01);
        this.WhenAnyValue(_ => _.Burn.Value).Subscribe(_ => this.CommandData.Burn = (float)this.Burn.Value);
    }

    public StringSelectionField ActionType { get; set; }

    // corrections
    public NumRangeField Cyan    { get; set; }
    public NumRangeField Magenta { get; set; }
    public NumRangeField Yellow  { get; set; }
    public NumRangeField Dodge   { get; set; }
    public NumRangeField Burn    { get; set; }

    public static BiDict<string, uint> ActionTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Disable", 0},
            {"Enable",  1},
        }
    );

}
