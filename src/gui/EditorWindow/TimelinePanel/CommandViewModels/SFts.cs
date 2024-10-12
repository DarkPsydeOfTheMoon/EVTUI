using System;

namespace EVTUI.ViewModels.TimelineCommands;

public class SFts : Generic
{
    public SFts(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Sounds: Footsteps";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.CommandData.ObjectId, config.EventManager.AssetIDs);
        this.Enable = new BoolChoiceField("Enabled?", this.Editable, this.CommandData.Enable != 0);
    }

    public IntSelectionField AssetID { get; set; }
    public BoolChoiceField   Enable  { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.CommandData.ObjectId = this.AssetID.Choice;
        this.CommandData.Enable   = Convert.ToInt32(this.Enable.Value);
    }
}
