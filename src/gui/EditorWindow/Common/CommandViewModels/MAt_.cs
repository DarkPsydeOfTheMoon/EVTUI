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
        //this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.UpdateHelperNames(commonVMs);

        //this.HelperID = new NumEntryField("Helper ID", this.Editable, this.CommandData.HelperId, 0, 9999, 1);
        //this.WhenAnyValue(_ => _.HelperID.Value).Subscribe(_ => this.CommandData.HelperId = (uint)this.HelperID.Value);
        this.HelperID = new StringSelectionField("Helper Node", this.Editable, (!(this.CommandData.HelperId is null) && this.HelperNames.Backward.ContainsKey(this.CommandData.HelperId)) ? this.HelperNames.Backward[this.CommandData.HelperId] : null, this.HelperNames.Keys);
        //this.WhenAnyValue(_ => _.HelperID.Choice).Subscribe(_ => this.CommandData.HelperId = this.HelperNames.Forward[this.HelperID.Choice]);
        this.WhenAnyValue(_ => _.HelperID.Choice).Subscribe(_ =>
        {
            if (!(this.HelperID.Choice is null) && this.HelperNames.Forward.ContainsKey(this.HelperID.Choice))
                this.CommandData.HelperId = this.HelperNames.Forward[this.HelperID.Choice];
        });
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ =>
        {
            this.Command.ObjectId = this.AssetID.Choice;
            this.UpdateHelperNames(commonVMs);
            //this.HelperID.Choices = new ObservableCollection<string>(this.HelperNames.Keys);
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

        this.ParentModelPreviewVM = new ModelPreviewWidget(config, this.AssetID);
        this.ChildModelPreviewVM = new ModelPreviewWidget(config, this.ChildAssetID);

        this.UnkBool = new BoolChoiceField("Unknown", this.Editable, this.CommandData.Flags[4]);
        this.WhenAnyValue(_ => _.UnkBool.Value).Subscribe(_ => this.CommandData.Flags[4] = this.UnkBool.Value);
    }

    public ModelPreviewWidget ParentModelPreviewVM { get; set; }
    public ModelPreviewWidget ChildModelPreviewVM  { get; set; }

    public IntSelectionField AssetID { get; set; }

    //public NumEntryField     HelperID     { get; set; } // TODO: parse GFD and present as string selection...
    public StringSelectionField HelperID     { get; set; }
    public IntSelectionField    ChildAssetID { get; set; }

    public Position3D     Offset   { get; set; }
    public RotationWidget Rotation { get; set; }

    public BoolChoiceField UnkBool { get; set; }

    private BiDict<string, uint> HelperNames;
    private void UpdateHelperNames(CommonViewModels commonVMs)
    {
        this.HelperNames = new BiDict<string, uint>();
        foreach (AssetViewModel asset in commonVMs.Assets)
            if ((int)asset.ObjectID.Value == this.AssetID.Choice)
            {
                if (!String.IsNullOrEmpty(asset.ActiveModelPath))
                {
                    GFDLibrary.ModelPack model = GFDLibrary.Api.FlatApi.LoadModel(asset.ActiveModelPath);
                    foreach (GFDLibrary.Models.Node node in model.Model.Nodes)
                        if (node.Properties.ContainsKey("gfdHelperID"))
                        {
                            int id = (int)node.Properties["gfdHelperID"].GetValue();
                            this.HelperNames.Add($"{node.Name} ({id})", (uint)id);
                        }
                }
                break;
            }
    }
}
