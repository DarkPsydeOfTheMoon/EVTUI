using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.Utils;

namespace EVTUI.ViewModels.TimelineCommands;

public class CShk : Generic
{
    public CShk(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Camera: Shaking Effect";

        this.ActionType = new StringSelectionField("Mode", this.Editable, this.ActionTypes.Backward[this.CommandData.Action], this.ActionTypes.Keys);
        this.WhenAnyValue(_ => _.ActionType.Choice).Subscribe(_ => this.CommandData.Action = this.ActionTypes.Forward[this.ActionType.Choice]);
        this.ShakingType = new StringSelectionField("Effect Type", this.Editable, this.ShakingTypes.Backward[this.CommandData.ShakingType], this.ShakingTypes.Keys);
        this.WhenAnyValue(_ => _.ShakingType.Choice).Subscribe(_ => this.CommandData.ShakingType = this.ShakingTypes.Forward[this.ShakingType.Choice]);
        this.Magnitude = new NumRangeField("Magnitude", this.Editable, this.CommandData.Magnitude, 0, 100, 1);
        this.WhenAnyValue(_ => _.Magnitude.Value).Subscribe(_ => this.CommandData.Magnitude = (float)this.Magnitude.Value);
        this.Speed = new NumRangeField("Speed", this.Editable, this.CommandData.Speed, 0, 100, 1);
        this.WhenAnyValue(_ => _.Speed.Value).Subscribe(_ => this.CommandData.Speed = (float)this.Speed.Value);
    }

    public StringSelectionField ActionType  { get; set; }
    public StringSelectionField ShakingType { get; set; }
    public NumRangeField        Magnitude   { get; set; }
    public NumRangeField        Speed       { get; set; }

    public BiDict<string, uint> ActionTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Shaking On",  0},
            {"Shaking Off", 1},
        }
    );

    public BiDict<string, uint> ShakingTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Basic",     0},
            {"Train Car", 1},
            {"Close-Up",  2},
        }
    );
}
