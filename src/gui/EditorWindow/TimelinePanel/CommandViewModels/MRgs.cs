using System;
using System.Collections.Generic;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class MRgs : Generic
{
    public MRgs(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Registry";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.Display = new StringSelectionField("Display", this.Editable, this.DisplayTypes.Backward[this.CommandData.DisplayType], this.DisplayTypes.Keys);
        this.Scene = new StringSelectionField("Scene", this.Editable, this.SceneTypes.Backward[this.CommandData.Scene], this.SceneTypes.Keys);
    }

    public IntSelectionField    AssetID { get; set; }
    public StringSelectionField Display { get; set; }
    public StringSelectionField Scene   { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        this.CommandData.DisplayType = this.DisplayTypes.Forward[this.Display.Choice];
        this.CommandData.Scene = this.SceneTypes.Forward[this.Scene.Choice];
    }

    public BiDict<string, uint> DisplayTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"None", 0},
            {"On",   1},
            {"Off",  2},
        }
    );

    public BiDict<string, uint> SceneTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Scene 0", 0},
            {"Scene 1", 1},
        }
    );
}
