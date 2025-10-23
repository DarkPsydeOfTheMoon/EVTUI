using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EVTUI.ViewModels.TimelineCommands;

public class SBE_ : Generic
{
    public SBE_(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Sounds: Environment Noise";

        config.AudioManager.SetActiveACBType("Field");
        this.CueID    = new IntSelectionField("Cue ID", this.Editable, (config.AudioManager.CueIds.Contains((uint)this.CommandData.CueId)) ? (int)this.CommandData.CueId : 0, config.AudioManager.CueIds.ConvertAll(x => (int)x));

        this.Action   = new StringSelectionField("Action???", this.Editable, Enum.GetName(typeof(ActionTypes), this.CommandData.UnkEnum), new List<string>(Enum.GetNames(typeof(ActionTypes))));
        this.Enable   = new BoolChoiceField("Enabled????", this.Editable, this.CommandData.Enable != 0);
    }

    public IntSelectionField    CueID  { get; set; }
    public StringSelectionField Action { get; set; }
    public BoolChoiceField      Enable { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.CommandData.CueId   = (uint)this.CueID.Choice;
        this.CommandData.UnkEnum = (int)Enum.Parse(typeof(ActionTypes), this.Action.Choice);
        this.CommandData.Enable  = Convert.ToInt32(this.Enable.Value);
    }

    public enum ActionTypes : int
    {
        Play = 1,
        Stop = 2
    }
}
