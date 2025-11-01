using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class EnOl : Generic
{
    public EnOl(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Environment: Outline";

        // unknown
        this.Unk1 = new NumRangeField("Unknown #1", this.Editable, this.CommandData.Strength, 0, 9999, 1);
        this.WhenAnyValue(_ => _.Unk1.Value).Subscribe(_ => this.CommandData.Strength = (float)this.Unk1.Value);
        this.Unk2 = new NumRangeField("Unknown #2", this.Editable, this.CommandData.Width, 0, 9999, 1);
        this.WhenAnyValue(_ => _.Unk2.Value).Subscribe(_ => this.CommandData.Width = (float)this.Unk2.Value);
        this.Unk3 = new NumRangeField("Unknown #3", this.Editable, this.CommandData.Brightness, 0, 1, 0.01);
        this.WhenAnyValue(_ => _.Unk3.Value).Subscribe(_ => this.CommandData.Brightness = (float)this.Unk3.Value);
        this.Unk4 = new NumRangeField("Unknown #4", this.Editable, this.CommandData.RangeMin, 0, 9999, 1);
        this.WhenAnyValue(_ => _.Unk4.Value).Subscribe(_ => this.CommandData.RangeMin = (float)this.Unk4.Value);
        this.Unk5 = new NumRangeField("Unknown #5", this.Editable, this.CommandData.RangeMax, 0, 9999, 1);
        this.WhenAnyValue(_ => _.Unk5.Value).Subscribe(_ => this.CommandData.RangeMax   = (float)this.Unk5.Value);
    }

    // direction
    public NumRangeField Unk1 { get; set; }
    public NumRangeField Unk2 { get; set; }
    public NumRangeField Unk3 { get; set; }
    public NumRangeField Unk4 { get; set; }
    public NumRangeField Unk5 { get; set; }
}
