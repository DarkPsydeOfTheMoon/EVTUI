using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.Utils;

namespace EVTUI.ViewModels.TimelineCommands;

public class ERgs : Generic
{
    public ERgs(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Effect: Registry";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDsOfType(0x01000002));
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.Display = new StringSelectionField("Display", this.Editable, Generic.RegistryDisplayTypes.Backward[this.CommandData.DisplayType], Generic.RegistryDisplayTypes.Keys);
        this.WhenAnyValue(_ => _.Display.Choice).Subscribe(_ => this.CommandData.DisplayType = Generic.RegistryDisplayTypes.Forward[this.Display.Choice]);
        this.Scene = new StringSelectionField("Scene", this.Editable, Generic.RegistrySceneTypes.Backward[this.CommandData.Scene], Generic.RegistrySceneTypes.Keys);
        this.WhenAnyValue(_ => _.Scene.Choice).Subscribe(_ => this.CommandData.Scene = Generic.RegistrySceneTypes.Forward[this.Scene.Choice]);
    }

    public IntSelectionField    AssetID { get; set; }
    public StringSelectionField Display { get; set; }
    public StringSelectionField Scene   { get; set; }
}
