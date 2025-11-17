using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class MLd_ : Generic
{
    public MLd_(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Model: Load";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);
    }

    public IntSelectionField AssetID { get; set; }
}
