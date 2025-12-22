using System;
using System.Collections.Generic;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class MAA_ : Generic
{
    public MAA_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Model: Additive Animation";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.TrackNumber = new NumEntryField("Track Number", this.Editable, this.CommandData.TrackNumber, 1, 7, 1);
        this.WhenAnyValue(_ => _.TrackNumber.Value).Subscribe(_ => this.CommandData.TrackNumber = (int)this.TrackNumber.Value);
        this.DebugFrameForward = new BoolChoiceField("Frame Forward", this.Editable, this.CommandData.Flags[31]);
        this.WhenAnyValue(_ => _.DebugFrameForward.Value).Subscribe(_ => this.CommandData.Flags[31] = this.DebugFrameForward.Value);

        this.AddAnimation = new AnimationWidget(config, commonVMs, this.AssetID, this.CommandData.AddAnimation, this.CommandData.Flags, $"Animation", extInd:0, trackNum:(int)this.TrackNumber.Value);
    }

    public IntSelectionField AssetID           { get; set; }
    public NumEntryField     TrackNumber       { get; set; }
    public BoolChoiceField   DebugFrameForward { get; set; }
    public AnimationWidget   AddAnimation      { get; set; }
}
