using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class MFts : Generic
{
    public MFts(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Footsteps";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);

        this.Enable = new BoolChoiceField("Show footsteps?", this.Editable, !this.CommandData.Flags[0]);
        this.AreaDistortion = new BoolChoiceField("Add area distortion effect?", this.Editable, this.CommandData.Flags[1]);
        this.PuddleEffect = new BoolChoiceField("Add puddle effect?", this.Editable, this.CommandData.Flags[2]);
    }

    public IntSelectionField AssetID { get; set; }

    public BoolChoiceField Enable         { get; set; }
    public BoolChoiceField AreaDistortion { get; set; }
    public BoolChoiceField PuddleEffect   { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        this.CommandData.Flags[0] = !this.Enable.Value;
        this.CommandData.Flags[1] = this.AreaDistortion.Value;
        this.CommandData.Flags[2] = this.PuddleEffect.Value;
    }
}
