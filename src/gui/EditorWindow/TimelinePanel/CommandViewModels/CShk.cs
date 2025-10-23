using System.Collections.Generic;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class CShk : Generic
{
    public CShk(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Camera: Shaking Effect";

        this.ActionType = new StringSelectionField("Mode", this.Editable, this.ActionTypes.Backward[this.CommandData.Action], this.ActionTypes.Keys);
        this.ShakingType = new StringSelectionField("Effect Type", this.Editable, this.ShakingTypes.Backward[this.CommandData.ShakingType], this.ShakingTypes.Keys);
        this.Magnitude = new NumRangeField("Magnitude", this.Editable, this.CommandData.Magnitude, 0, 100, 1);
        this.Speed = new NumRangeField("Speed", this.Editable, this.CommandData.Speed, 0, 100, 1);
    }

    public StringSelectionField ActionType  { get; set; }
    public StringSelectionField ShakingType { get; set; }
    public NumRangeField        Magnitude   { get; set; }
    public NumRangeField        Speed       { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();

        this.CommandData.Action      = this.ActionTypes.Forward[this.ActionType.Choice];
        this.CommandData.ShakingType = this.ShakingTypes.Forward[this.ShakingType.Choice];
        this.CommandData.Magnitude   = (float)this.Magnitude.Value;
        this.CommandData.Speed       = (float)this.Speed.Value;
    }

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
