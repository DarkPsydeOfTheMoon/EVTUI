using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class ELd_ : Generic
{
    public ELd_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Effect: Load";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDsOfType(0x01000002));
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);
    }

    public IntSelectionField AssetID { get; set; }
}
