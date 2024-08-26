using System;
using System.Collections.Generic;

namespace EVTUI.ViewModels.TimelineCommands;

public class SBEA : Generic
{
    public SBEA(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Sounds: Background Effects (All)";
        this.UnkEnum = new StringSelectionField("(Unknown)", this.Editable, Enum.GetName(typeof(Options), this.CommandData.UnkEnum), new List<string>(Enum.GetNames(typeof(Options))));
    }

    public StringSelectionField UnkEnum { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.CommandData.UnkEnum = (int)Enum.Parse(typeof(Options), this.UnkEnum.Choice);
    }

    public enum Options : int
    {
        Option1 = 1,
        Option2 = 2
    }
}
