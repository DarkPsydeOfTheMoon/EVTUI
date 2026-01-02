using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.Utils;

namespace EVTUI.ViewModels.TimelineCommands;

public class EnFD : Generic
{
    public EnFD(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Environment: Fog Distance";

        this.ScaleType = new StringSelectionField("Mode", this.Editable, EnFD.ScaleTypes.Backward[this.CommandData.Mode], EnFD.ScaleTypes.Keys);
        this.WhenAnyValue(_ => _.ScaleType.Choice).Subscribe(_ => this.CommandData.Mode = EnFD.ScaleTypes.Forward[this.ScaleType.Choice]);
        this.FogColor = new ColorSelectionField("Fog Color", this.Editable, this.CommandData.RGBA);
        this.WhenAnyValue(_ => _.FogColor.SelectedColor).Subscribe(_ => this.CommandData.RGBA = this.FogColor.ToUInt32());

        // distance/range
        this.MatchWithCameraClip = new BoolChoiceField("Match to Camera Clip?", this.Editable, this.CommandData.Flags[16]);
        this.WhenAnyValue(_ => _.MatchWithCameraClip.Value).Subscribe(_ => this.CommandData.Flags[16] = this.MatchWithCameraClip.Value);
        this.StartDistance = new NumRangeField("Start", this.Editable, this.CommandData.StartDistance, -999999, 999999, 1);
        this.WhenAnyValue(_ => _.StartDistance.Value).Subscribe(_ => this.CommandData.StartDistance = (float)this.StartDistance.Value);
        this.EndDistance = new NumRangeField("End", this.Editable, this.CommandData.EndDistance, -999999, 999999, 1);
        this.WhenAnyValue(_ => _.EndDistance.Value).Subscribe(_ => this.CommandData.EndDistance = (float)this.EndDistance.Value);
    }

    public StringSelectionField ScaleType { get; set; }
    public ColorSelectionField  FogColor  { get; set; }

    // distance/range
    public BoolChoiceField MatchWithCameraClip { get; set; }
    public NumRangeField   StartDistance       { get; set; }
    public NumRangeField   EndDistance         { get; set; }

    public static BiDict<string, uint> ScaleTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Linear",           0},
            {"Exponential (v1)", 1},
            {"Exponential (v2)", 2},
        }
    );
}
