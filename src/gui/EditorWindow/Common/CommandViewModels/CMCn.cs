using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.Utils;

namespace EVTUI.ViewModels.TimelineCommands;

public class CMCn : Generic
{
    public CMCn(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Camera: Continuous Movement";

        this.DirectionType = new StringSelectionField("Direction Type", this.Editable, CMCn.DirectionTypes.Backward[this.CommandData.DirectionType], CMCn.DirectionTypes.Keys);
        this.WhenAnyValue(_ => _.DirectionType.Choice).Subscribe(_ => this.CommandData.DirectionType = CMCn.DirectionTypes.Forward[this.DirectionType.Choice]);
        this.Distance = new NumRangeField("Distance", this.Editable, this.CommandData.Distance, 0, 0.1, 0.001);
        this.WhenAnyValue(_ => _.Distance.Value).Subscribe(_ => this.CommandData.Distance = (float)this.Distance.Value);
    }

    public StringSelectionField DirectionType { get; set; }
    public NumRangeField        Distance      { get; set; }

    public static BiDict<string, uint> DirectionTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"None",    0},
            {"Forward", 1},
            {"Back",    2},
            {"Left",    3},
            {"Right",   4},
            {"Up",      5},
            {"Down",    6},
        }
    );
}
