using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class FGFl : Generic
{
    public FGFl(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Field: ???";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDsOfType(0x00000003));
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.Unk = new NumEntryField("Unknown", this.Editable, this.CommandData.Unk, 0, 2, 1);
        this.WhenAnyValue(_ => _.Unk.Value).Subscribe(_ => this.CommandData.Unk = (uint)this.Unk.Value);
    }

    public IntSelectionField AssetID { get; set; }
    public NumEntryField     Unk     { get; set; }
}
