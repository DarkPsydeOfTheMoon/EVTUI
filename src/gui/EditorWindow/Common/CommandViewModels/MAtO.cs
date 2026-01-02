using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.Utils;

namespace EVTUI.ViewModels.TimelineCommands;

public class MAtO : Generic
{
    public MAtO(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Model: Attachment Offset";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.ChildAssetID = new IntSelectionField("Attached Asset ID", this.Editable, this.CommandData.ChildObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.ChildAssetID.Choice).Subscribe(_ => this.CommandData.ChildObjectId = this.ChildAssetID.Choice);

        this.Offset = new Position3D("Offset (From Attachment Point)", this.Editable, this.CommandData.RelativePosition);
        this.Rotation = new RotationWidget(config, this.CommandData.Rotation, null, pitchInd: 0, yawInd: 1);

        // interpolation
        this.InterpolationSettings = new InterpolationParameters(this.CommandData.InterpolationParameters, this.Editable);
        this.WhenAnyValue(_ => _.InterpolationSettings.InterpolationType.Choice, _ => _.InterpolationSettings.SlopeInType.Choice, _ => _.InterpolationSettings.SlopeOutType.Choice).Subscribe(_ => this.CommandData.InterpolationParameters = this.InterpolationSettings.Compose());

        this.ParentModelPreviewVM = new ModelPreviewWidget(config, commonVMs, this.AssetID);
        this.ChildModelPreviewVM = new ModelPreviewWidget(config, commonVMs, this.ChildAssetID);
    }

    public ModelPreviewWidget ParentModelPreviewVM { get; set; }
    public ModelPreviewWidget ChildModelPreviewVM  { get; set; }

    public IntSelectionField AssetID { get; set; }

    public IntSelectionField ChildAssetID { get; set; }

    public Position3D     Offset   { get; set; }
    public RotationWidget Rotation { get; set; }

    public InterpolationParameters InterpolationSettings { get; set; }
}
