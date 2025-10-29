using System;
using System.Collections.Generic;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class EnSs : Generic
{
    public EnSs(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Environment: SSAO";

        this.Enabled = new BoolChoiceField("Enable Screen Space Ambient Occlusion (SSAO)?", this.Editable, this.CommandData.Enable != 0);
        this.WhenAnyValue(_ => _.Enabled.Value).Subscribe(_ => this.CommandData.Enable = Convert.ToUInt32(this.Enabled.Value));

        this.Range = new NumRangeField("Range", this.Editable, this.CommandData.Range, 0, 1000, 1);
        this.WhenAnyValue(_ => _.Range.Value).Subscribe(_ => this.CommandData.Range = (float)this.Range.Value);
        this.Radius = new NumRangeField("Radius", this.Editable, this.CommandData.Radius, 0, 3, 0.01);
        this.WhenAnyValue(_ => _.Radius.Value).Subscribe(_ => this.CommandData.Radius = (float)this.Radius.Value);
        this.Attenuation = new NumRangeField("Attenuation", this.Editable, this.CommandData.Attenuation, 0.01, 1.0, 0.01);
        this.WhenAnyValue(_ => _.Attenuation.Value).Subscribe(_ => this.CommandData.Attenuation = (float)this.Attenuation.Value);
        this.Concentration = new NumRangeField("Concentration", this.Editable, this.CommandData.Concentration, 0.01, 5.0, 0.01);
        this.WhenAnyValue(_ => _.Concentration.Value).Subscribe(_ => this.CommandData.Concentration = (float)this.Concentration.Value);
        this.Blur = new NumRangeField("Blur", this.Editable, this.CommandData.Blur, 0, 5, 0.01);
        this.WhenAnyValue(_ => _.Blur.Value).Subscribe(_ => this.CommandData.Blur = (float)this.Blur.Value);
    }

    public BoolChoiceField Enabled { get; set; }

    public NumRangeField Range         { get; set; }
    public NumRangeField Radius        { get; set; }
    public NumRangeField Attenuation   { get; set; }
    public NumRangeField Concentration { get; set; }
    public NumRangeField Blur          { get; set; }
}
