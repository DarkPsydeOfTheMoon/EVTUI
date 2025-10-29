using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class CAA_ : Generic
{
    public CAA_(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Camera: Additive Animation";

        // animation source
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.CommandData.AssetId, config.EventManager.AssetIDsOfType(0x00000007));
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.CommandData.AssetId = this.AssetID.Choice);
        this.AnimationID = new NumEntryField("Animation ID", this.Editable, this.CommandData.AnimationId, 0, 59, 1);
        this.WhenAnyValue(_ => _.AnimationID.Value).Subscribe(_ => this.CommandData.AnimationId = (uint)this.AnimationID.Value);
        this.PlaybackSpeed = new NumEntryField("Playback Speed", this.Editable, this.CommandData.PlaybackSpeed, 0, 10, 0.1);
        this.WhenAnyValue(_ => _.PlaybackSpeed.Value).Subscribe(_ => this.CommandData.PlaybackSpeed = (float)this.PlaybackSpeed.Value);

        // bits
        this.LoopBool = new BoolChoiceField("Loop Playback?", this.Editable, this.CommandData.Flags[0]);
        this.WhenAnyValue(_ => _.LoopBool.Value).Subscribe(_ => this.CommandData.Flags[0] = this.LoopBool.Value);
        this.EnableViewAngleUpdate = new BoolChoiceField("Enable View Angle Update?", this.Editable, this.CommandData.Flags[2]);
        this.WhenAnyValue(_ => _.EnableViewAngleUpdate.Value).Subscribe(_ => this.CommandData.Flags[2] = this.EnableViewAngleUpdate.Value);
    }

    // animation source
    public IntSelectionField AssetID       { get; set; }
    public NumEntryField     AnimationID   { get; set; }
    public NumEntryField     PlaybackSpeed { get; set; }

    // bits
    public BoolChoiceField LoopBool              { get; set; }
    public BoolChoiceField EnableViewAngleUpdate { get; set; }
}
