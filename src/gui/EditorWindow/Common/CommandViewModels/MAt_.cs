using System;
using System.Collections.Generic;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class MAt_ : Generic
{
    public MAt_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Model: Attachment";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.HelperID = new NumEntryField("Helper ID", this.Editable, this.CommandData.HelperId, 0, 9999, 1);
        this.WhenAnyValue(_ => _.HelperID.Value).Subscribe(_ => this.CommandData.HelperId = (uint)this.HelperID.Value);
        this.ChildAssetID = new IntSelectionField("Attached Asset ID", this.Editable, this.CommandData.ChildObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.ChildAssetID.Choice).Subscribe(_ => this.CommandData.ChildObjectId = this.ChildAssetID.Choice);

        this.Offset = new Position3D("Offset (From Attachment Point)", this.Editable, this.CommandData.RelativePosition);
        this.Rotation = new RotationWidget(config, this.CommandData.Rotation, this.CommandData.Flags, pitchInd: 0, yawInd: 1);

        this.ParentModelPreviewVM = new ModelPreviewWidget(config, this.AssetID);
        this.ChildModelPreviewVM = new ModelPreviewWidget(config, this.ChildAssetID);

        this.UnkBool = new BoolChoiceField("Unknown", this.Editable, this.CommandData.Flags[4]);
        this.WhenAnyValue(_ => _.UnkBool.Value).Subscribe(_ => this.CommandData.Flags[4] = this.UnkBool.Value);
    }

    public ModelPreviewWidget ParentModelPreviewVM { get; set; }
    public ModelPreviewWidget ChildModelPreviewVM  { get; set; }

    public IntSelectionField AssetID { get; set; }

    public NumEntryField     HelperID     { get; set; } // TODO: parse GFD and present as string selection...
    public IntSelectionField ChildAssetID { get; set; }

    public Position3D     Offset   { get; set; }
    public RotationWidget Rotation { get; set; }

    public BoolChoiceField UnkBool { get; set; }
}
