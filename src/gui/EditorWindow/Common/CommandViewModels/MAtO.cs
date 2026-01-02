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

        this.InterpolationType = new StringSelectionField("Interpolation Type", this.Editable, MAtO.InterpolationTypes.Backward[this.CommandData.InterpolationType], MAtO.InterpolationTypes.Keys);
        this.WhenAnyValue(_ => _.InterpolationType.Choice).Subscribe(_ => this.CommandData.InterpolationType = MAtO.InterpolationTypes.Forward[this.InterpolationType.Choice]);

        this.Offset = new Position3D("Offset (From Attachment Point)", this.Editable, this.CommandData.RelativePosition);
        this.Rotation = new RotationWidget(config, this.CommandData.Rotation, null, pitchInd: 0, yawInd: 1);

        this.ParentModelPreviewVM = new ModelPreviewWidget(config, commonVMs, this.AssetID);
        this.ChildModelPreviewVM = new ModelPreviewWidget(config, commonVMs, this.ChildAssetID);
    }

    public ModelPreviewWidget ParentModelPreviewVM { get; set; }
    public ModelPreviewWidget ChildModelPreviewVM  { get; set; }

    public IntSelectionField AssetID      { get; set; }

    public IntSelectionField ChildAssetID         { get; set; }
    public StringSelectionField InterpolationType { get; set; }

    public Position3D     Offset   { get; set; }
    public RotationWidget Rotation { get; set; }

    public static BiDict<string, uint> InterpolationTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Linear",  0},
            {"Step",    1},
            {"Hermite", 2},
        }
    );

}
