using System;
using System.Collections.Generic;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class EnCc : Generic
{
    public EnCc(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Environment: Color Correction";

        this.ActionType = new StringSelectionField("Mode", this.Editable, this.ActionTypes.Backward[this.CommandData.Enable], this.ActionTypes.Keys);

        // corrections
        this.Cyan = new NumRangeField("Cyan", this.Editable, this.CommandData.Cyan, -1, 1, 0.01);
        this.Magenta = new NumRangeField("Magenta", this.Editable, this.CommandData.Magenta, -1, 1, 0.01);
        this.Yellow = new NumRangeField("Yellow", this.Editable, this.CommandData.Yellow, -1, 1, 0.01);
        this.Dodge = new NumRangeField("Dodge", this.Editable, this.CommandData.Dodge, 0, 0.99, 0.01);
        this.Burn = new NumRangeField("Burn", this.Editable, this.CommandData.Burn, 0, 0.99, 0.01);

    }

    public StringSelectionField ActionType { get; set; }

    // corrections
    public NumRangeField Cyan    { get; set; }
    public NumRangeField Magenta { get; set; }
    public NumRangeField Yellow  { get; set; }
    public NumRangeField Dodge   { get; set; }
    public NumRangeField Burn    { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();

        this.CommandData.Enable = this.ActionTypes.Forward[this.ActionType.Choice];

        this.CommandData.Cyan    = (float)this.Cyan.Value;
        this.CommandData.Magenta = (float)this.Magenta.Value;
        this.CommandData.Yellow  = (float)this.Yellow.Value;
        this.CommandData.Dodge   = (float)this.Dodge.Value;
        this.CommandData.Burn    = (float)this.Burn.Value;
    }

    public BiDict<string, uint> ActionTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Disable", 0},
            {"Enable",  1},
        }
    );

}
