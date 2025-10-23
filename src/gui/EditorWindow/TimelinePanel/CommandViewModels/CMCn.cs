using System.Collections.Generic;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class CMCn : Generic
{
    public CMCn(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Camera: Continuous Movement";

        this.DirectionType = new StringSelectionField("Direction Type", this.Editable, this.DirectionTypes.Backward[this.CommandData.DirectionType], this.DirectionTypes.Keys);
        this.Distance = new NumRangeField("Distance", this.Editable, this.CommandData.Distance, 0, 0.1, 0.001);
    }

    public StringSelectionField DirectionType { get; set; }
    public NumRangeField        Distance      { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();

        this.CommandData.DirectionType = this.DirectionTypes.Forward[this.DirectionType.Choice];
        this.CommandData.Distance = (float)this.Distance.Value;
    }

    public BiDict<string, uint> DirectionTypes = new BiDict<string, uint>
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
