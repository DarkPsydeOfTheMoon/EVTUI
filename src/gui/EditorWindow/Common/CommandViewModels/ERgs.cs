using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class ERgs : Generic
{
    public ERgs(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Effect: Registry";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDsOfType(0x01000002));
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.Display = new StringSelectionField("Display", this.Editable, this.DisplayTypes.Backward[this.CommandData.DisplayType], this.DisplayTypes.Keys);
        this.WhenAnyValue(_ => _.Display.Choice).Subscribe(_ => this.CommandData.DisplayType = this.DisplayTypes.Forward[this.Display.Choice]);
        this.Scene = new StringSelectionField("Scene", this.Editable, this.SceneTypes.Backward[this.CommandData.Scene], this.SceneTypes.Keys);
        this.WhenAnyValue(_ => _.Scene.Choice).Subscribe(_ => this.CommandData.Scene = this.SceneTypes.Forward[this.Scene.Choice]);
    }

    public IntSelectionField    AssetID { get; set; }
    public StringSelectionField Display { get; set; }
    public StringSelectionField Scene   { get; set; }

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
