using System;
using System.Collections.Generic;

namespace EVTUI.ViewModels.TimelineCommands;

public class MRgs : Generic
{
    public MRgs(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Registry";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.Action = new StringSelectionField("Spawn or Despawn?", this.Editable, Enum.GetName(typeof(Actions), this.CommandData.ActionType), new List<string>(Enum.GetNames(typeof(Actions))));
    }

    public IntSelectionField AssetID   { get; set; }
    public StringSelectionField Action { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;
        this.CommandData.ActionType = (int)Enum.Parse(typeof(Actions), this.Action.Choice);
    }

    public enum Actions : int
    {
        Spawn   = 1,
        Despawn = 2
    }
}
