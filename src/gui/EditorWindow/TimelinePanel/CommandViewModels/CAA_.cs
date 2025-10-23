namespace EVTUI.ViewModels.TimelineCommands;

public class CAA_ : Generic
{
    public CAA_(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Camera: Additive Animation";

        // animation source
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.CommandData.AssetId, config.EventManager.AssetIDsOfType(0x00000007));
        this.AnimationID = new NumEntryField("Animation ID", this.Editable, this.CommandData.AnimationId, 0, 59, 1);
        this.PlaybackSpeed = new NumEntryField("Playback Speed", this.Editable, this.CommandData.PlaybackSpeed, 0, 10, 0.1);

        // bits
        this.LoopBool = new BoolChoiceField("Loop Playback?", this.Editable, this.CommandData.Flags[0]);
        this.EnableViewAngleUpdate = new BoolChoiceField("Enable View Angle Update?", this.Editable, this.CommandData.Flags[2]);
    }

    // animation source
    public IntSelectionField AssetID       { get; set; }
    public NumEntryField     AnimationID   { get; set; }
    public NumEntryField     PlaybackSpeed { get; set; }

    // bits
    public BoolChoiceField LoopBool              { get; set; }
    public BoolChoiceField EnableViewAngleUpdate { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();

        this.CommandData.AssetId       = this.AssetID.Choice;
        this.CommandData.AnimationId   = (uint)this.AnimationID.Value;
        this.CommandData.PlaybackSpeed = (float)this.PlaybackSpeed.Value;

        this.CommandData.Flags[0] = this.LoopBool.Value;
        this.CommandData.Flags[2] = this.EnableViewAngleUpdate.Value;
    }
}
