using System.Collections.Generic;

namespace EVTUI.ViewModels.TimelineCommands;

public class Scr_ : Generic
{
    public Scr_(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Script: Run Procedure";
        //this.ProcedureIndex = new IntSelectionField("Procedure Index", this.Editable, this.CommandData.ProcedureIndex, new List<int>{this.CommandData.ProcedureIndex});
        this.ProcedureIndex = new NumEntryField("Procedure Index", this.Editable, this.CommandData.ProcedureIndex, 0, 99, 1);
    }

    //public IntSelectionField ProcedureIndex { get; set; }
    public NumEntryField ProcedureIndex { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        //this.CommandData.ProcedureIndex = this.ProcedureIndex.Choice;
        this.CommandData.ProcedureIndex = (int)this.ProcedureIndex.Value;
    }
}
