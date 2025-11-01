using System;
using System.Collections.Generic;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class Scr_ : Generic
{
    public Scr_(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Script: Run Procedure";
        //this.ProcedureIndex = new IntSelectionField("Procedure Index", this.Editable, this.CommandData.ProcedureIndex, new List<int>{this.CommandData.ProcedureIndex});
        //this.WhenAnyValue(_ => _.ProcedureIndex.Choice).Subscribe(_ => this.CommandData.ProcedureIndex = this.ProcedureIndex.Choice);
        this.ProcedureIndex = new NumEntryField("Procedure Index", this.Editable, this.CommandData.ProcedureIndex, 0, 99, 1);
        this.WhenAnyValue(_ => _.ProcedureIndex.Value).Subscribe(_ => this.CommandData.ProcedureIndex = (int)this.ProcedureIndex.Value);
    }

    //public IntSelectionField ProcedureIndex { get; set; }
    public NumEntryField ProcedureIndex { get; set; }
}
