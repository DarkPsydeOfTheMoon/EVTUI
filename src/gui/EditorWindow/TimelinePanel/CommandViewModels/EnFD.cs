using System;
using System.Collections.Generic;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class EnFD : Generic
{
    public EnFD(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Environment: Fog Distance";

        this.ScaleType = new StringSelectionField("Mode", this.Editable, this.ScaleTypes.Backward[this.CommandData.Mode], this.ScaleTypes.Keys);
        this.FogColor = new ColorSelectionField("Fog Color", this.Editable, this.CommandData.RGBA);

        // distance/range
        this.MatchWithCameraClip = new BoolChoiceField("Match to Camera Clip?", this.Editable, this.CommandData.Flags[16]);
        this.StartDistance = new NumRangeField("Start", this.Editable, this.CommandData.StartDistance, -999999, 999999, 1);
        this.EndDistance = new NumRangeField("End", this.Editable, this.CommandData.EndDistance, -999999, 999999, 1);
    }

    public StringSelectionField ScaleType { get; set; }
    public ColorSelectionField  FogColor  { get; set; }

    // distance/range
    public BoolChoiceField MatchWithCameraClip { get; set; }
    public NumRangeField   StartDistance       { get; set; }
    public NumRangeField   EndDistance         { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();

        this.CommandData.Flags[16] = this.MatchWithCameraClip.Value;

        this.CommandData.Mode          = this.ScaleTypes.Forward[this.ScaleType.Choice];
        this.CommandData.StartDistance = (float)this.StartDistance.Value;
        this.CommandData.EndDistance   = (float)this.EndDistance.Value;
        this.CommandData.RGBA          = this.FogColor.ToUInt32();
    }

    public BiDict<string, uint> ScaleTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Linear",           0},
            {"Exponential (v1)", 1},
            {"Exponential (v2)", 2},
        }
    );
}
