using System.Collections.Generic;

namespace EVTUI.ViewModels.TimelineCommands;

public class Scr_ : Generic
{
    public Scr_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Script: Run Procedure";
        this.ProcedureIndex = new IntSelectionField("Procedure Index", this.Editable, this.CommandData.ProcedureIndex, new List<int>{this.CommandData.ProcedureIndex});
    }

    public IntSelectionField ProcedureIndex { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.CommandData.ProcedureIndex = this.ProcedureIndex.Choice;
    }
}
