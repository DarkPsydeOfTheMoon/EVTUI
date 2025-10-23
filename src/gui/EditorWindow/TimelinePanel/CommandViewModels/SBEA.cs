using System;
using System.Collections.Generic;

namespace EVTUI.ViewModels.TimelineCommands;

public class SBEA : Generic
{
    public SBEA(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Sounds: Environment Noise (All)";
        this.Action = new StringSelectionField("Action", this.Editable, Enum.GetName(typeof(ActionTypes), this.CommandData.Action), new List<string>(Enum.GetNames(typeof(ActionTypes))));
    }

    public StringSelectionField Action { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.CommandData.Action = (int)Enum.Parse(typeof(ActionTypes), this.Action.Choice);
    }

    public enum ActionTypes : int
    {
        Play = 1,
        Stop = 2
    }
}
