using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class MAt_ : Generic
{
    public MAt_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Model: Attachment";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);

        this.UpdateHelperNames(commonVMs, this.AssetID.Choice);

        this.HelperID = new StringSelectionField("Helper Node", this.Editable, (!(this.CommandData.HelperId is null) && this.HelperNames.Backward.ContainsKey(this.CommandData.HelperId)) ? this.HelperNames.Backward[this.CommandData.HelperId] : null, this.HelperNames.Keys);
        this.WhenAnyValue(_ => _.HelperID.Choice).Subscribe(_ =>
        {
            if (!(this.HelperID.Choice is null) && this.HelperNames.Forward.ContainsKey(this.HelperID.Choice))
                this.CommandData.HelperId = this.HelperNames.Forward[this.HelperID.Choice];
        });
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ =>
        {
            this.Command.ObjectId = this.AssetID.Choice;
            this.UpdateHelperNames(commonVMs, this.AssetID.Choice);
            string choice = this.HelperID.Choice;
            this.HelperID.Choices.Clear();
            foreach (string helperName in this.HelperNames.Keys)
                this.HelperID.Choices.Add(helperName);
            // in case the choice got erased by the last step i guess
            if (!(choice is null) && this.HelperNames.Forward.ContainsKey(choice))
                this.HelperID.Choice = choice;
        });

        this.ChildAssetID = new IntSelectionField("Attached Asset ID", this.Editable, this.CommandData.ChildObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.ChildAssetID.Choice).Subscribe(_ => this.CommandData.ChildObjectId = this.ChildAssetID.Choice);

        this.Offset = new Position3D("Offset (From Attachment Point)", this.Editable, this.CommandData.RelativePosition);
        this.Rotation = new RotationWidget(config, this.CommandData.Rotation, this.CommandData.Flags, pitchInd: 0, yawInd: 1);

        this.ParentModelPreviewVM = new ModelPreviewWidget(config, commonVMs, this.AssetID);
        this.ChildModelPreviewVM = new ModelPreviewWidget(config, commonVMs, this.ChildAssetID);

        this.UnkBool = new BoolChoiceField("Unknown", this.Editable, this.CommandData.Flags[4]);
        this.WhenAnyValue(_ => _.UnkBool.Value).Subscribe(_ => this.CommandData.Flags[4] = this.UnkBool.Value);
    }

    public ModelPreviewWidget ParentModelPreviewVM { get; set; }
    public ModelPreviewWidget ChildModelPreviewVM  { get; set; }

    public IntSelectionField AssetID { get; set; }

    public StringSelectionField HelperID     { get; set; }
    public IntSelectionField    ChildAssetID { get; set; }

    public Position3D     Offset   { get; set; }
    public RotationWidget Rotation { get; set; }

    public BoolChoiceField UnkBool { get; set; }
}
