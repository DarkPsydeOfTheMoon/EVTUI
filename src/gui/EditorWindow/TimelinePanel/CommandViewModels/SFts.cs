using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class SFts : Generic
{
    public SFts(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Sounds: Footsteps";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.CommandData.ObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.Enable = new BoolChoiceField("Enabled?", this.Editable, this.CommandData.Enable != 0);
        this.WhenAnyValue(_ => _.Enable.Value).Subscribe(_ => this.CommandData.Enable = Convert.ToInt32(this.Enable.Value));
    }

    public IntSelectionField AssetID { get; set; }
    public BoolChoiceField   Enable  { get; set; }
}
