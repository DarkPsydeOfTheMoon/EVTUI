using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class Env_ : Generic
{
    public Env_(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Environment: Load";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.CommandData.ObjectId, config.EventManager.AssetIDsOfType(0x00000004));
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);
    }

    public IntSelectionField AssetID { get; set; }
}
