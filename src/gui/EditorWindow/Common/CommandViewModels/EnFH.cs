using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class EnFH : Generic
{
    public EnFH(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Environment: Fog Height";

        this.FogColor = new ColorSelectionField("Fog Color", this.Editable, this.CommandData.RGBA);
        this.WhenAnyValue(_ => _.FogColor.SelectedColor).Subscribe(_ => this.CommandData.RGBA = this.FogColor.ToUInt32());

        // distance/range
        this.StartHeight = new NumRangeField("Start", this.Editable, this.CommandData.StartHeight, -999999, 999999, 1);
        this.WhenAnyValue(_ => _.StartHeight.Value).Subscribe(_ => this.CommandData.StartHeight = (float)this.StartHeight.Value);
        this.EndHeight = new NumRangeField("End", this.Editable, this.CommandData.EndHeight, -999999, 999999, 1);
        this.WhenAnyValue(_ => _.EndHeight.Value).Subscribe(_ => this.CommandData.EndHeight = (float)this.EndHeight.Value);
    }

    public ColorSelectionField  FogColor    { get; set; }

    // distance/range
    public NumRangeField        StartHeight { get; set; }
    public NumRangeField        EndHeight   { get; set; }
}
