using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class MSD_ : Generic
{
    public MSD_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Model: Placement";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.ModelPreviewVM = new ModelPreviewWidget(config, this.AssetID);

        this.Position = new Position3D("Position", this.Editable, this.CommandData.Position);
        this.Rotation = new RotationWidget(config, this.CommandData.Rotation, this.CommandData.Flags, pitchInd: 0, yawInd: 1, enabledInd: 1, enabledFlip: true);

        this.WaitingAnimation = new AnimationWidget(config, this.AssetID, this.CommandData.WaitingAnimation, this.CommandData.Flags, $"Idle Animation", enabledInd:0, extInd:2, enabledFlip:true);
    }

    public IntSelectionField  AssetID        { get; set; }
    public ModelPreviewWidget ModelPreviewVM { get; set; }

    public Position3D      Position         { get; set; }
    public RotationWidget  Rotation         { get; set; }

    public AnimationWidget WaitingAnimation { get; set; }
}
