using System;
using System.Collections.Generic;

namespace EVTUI.ViewModels.TimelineCommands;

public class EnSs : Generic
{
    public EnSs(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Environment: SSAO";

        this.Enabled = new BoolChoiceField("Enable Screen Space Ambient Occlusion (SSAO)?", this.Editable, this.CommandData.Enable != 0);

        this.Range = new NumRangeField("Range", this.Editable, this.CommandData.Range, 0, 1000, 1);
        this.Radius = new NumRangeField("Radius", this.Editable, this.CommandData.Radius, 0, 3, 0.01);
        this.Attenuation = new NumRangeField("Attenuation", this.Editable, this.CommandData.Attenuation, 0.01, 1.0, 0.01);
        this.Concentration = new NumRangeField("Concentration", this.Editable, this.CommandData.Concentration, 0.01, 5.0, 0.01);
        this.Blur = new NumRangeField("Blur", this.Editable, this.CommandData.Blur, 0, 5, 0.01);
    }

    public BoolChoiceField Enabled { get; set; }

    public NumRangeField Range         { get; set; }
    public NumRangeField Radius        { get; set; }
    public NumRangeField Attenuation   { get; set; }
    public NumRangeField Concentration { get; set; }
    public NumRangeField Blur          { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();

        this.CommandData.Enable = Convert.ToUInt32(this.Enabled.Value);

        this.CommandData.Range = (float)this.Range.Value;
        this.CommandData.Radius = (float)this.Radius.Value;
        this.CommandData.Attenuation = (float)this.Attenuation.Value;
        this.CommandData.Concentration = (float)this.Concentration.Value;
        this.CommandData.Blur = (float)this.Blur.Value;
    }
}
