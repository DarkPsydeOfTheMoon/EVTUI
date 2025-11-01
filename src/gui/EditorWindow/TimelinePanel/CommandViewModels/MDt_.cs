using System;
using System.Collections.Generic;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class MDt_ : Generic
{
    public MDt_(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Model: Detachment";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.HelperID = new NumEntryField("Helper ID", this.Editable, this.CommandData.HelperId, 0, 9999, 1);
        this.WhenAnyValue(_ => _.HelperID.Value).Subscribe(_ => this.CommandData.HelperId = (uint)this.HelperID.Value);
        this.ChildAssetID = new IntSelectionField("Attached Asset ID", this.Editable, this.CommandData.ChildObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.ChildAssetID.Choice).Subscribe(_ => this.CommandData.ChildObjectId = this.ChildAssetID.Choice);

        this.RemainInScene = new BoolChoiceField("Keep object in scene after detaching?", this.Editable, this.CommandData.Flags[3]);
        this.WhenAnyValue(_ => _.RemainInScene.Value).Subscribe(_ => this.CommandData.Flags[3] = this.RemainInScene.Value);

        this.X = new NumRangeField("X", this.Editable, this.CommandData.Position[0], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.X.Value).Subscribe(_ => this.CommandData.Position[0] = (float)this.X.Value);
        this.Y = new NumRangeField("Y", this.Editable, this.CommandData.Position[1], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.Y.Value).Subscribe(_ => this.CommandData.Position[1] = (float)this.Y.Value);
        this.Z = new NumRangeField("Z", this.Editable, this.CommandData.Position[2], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.Z.Value).Subscribe(_ => this.CommandData.Position[2] = (float)this.Z.Value);

        this.Pitch = new NumRangeField("Pitch", this.Editable, this.CommandData.Rotation[0], -180, 180, 1);
        this.WhenAnyValue(_ => _.Pitch.Value).Subscribe(_ => this.CommandData.Rotation[0] = (float)this.Pitch.Value);
        this.Yaw = new NumRangeField("Yaw", this.Editable, this.CommandData.Rotation[1], -180, 180, 1);
        this.WhenAnyValue(_ => _.Yaw.Value).Subscribe(_ => this.CommandData.Rotation[1] = (float)this.Yaw.Value);
        this.Roll = new NumRangeField("Roll", this.Editable, this.CommandData.Rotation[2], -180, 180, 1);
        this.WhenAnyValue(_ => _.Roll.Value).Subscribe(_ => this.CommandData.Rotation[2] = (float)this.Roll.Value);

        this.UnkBool = new BoolChoiceField("Unknown", this.Editable, this.CommandData.Flags[4]);
        this.WhenAnyValue(_ => _.UnkBool.Value).Subscribe(_ => this.CommandData.Flags[4] = this.UnkBool.Value);

        this.ParentModelPreviewVM = new ModelPreviewWidget(config, this.AssetID);
        this.ChildModelPreviewVM = new ModelPreviewWidget(config, this.ChildAssetID);
    }

    public ModelPreviewWidget ParentModelPreviewVM { get; set; }
    public ModelPreviewWidget ChildModelPreviewVM  { get; set; }

    public IntSelectionField AssetID { get; set; }

    public NumEntryField     HelperID     { get; set; } // TODO: parse GFD and present as string selection...
    public IntSelectionField ChildAssetID { get; set; }

    public BoolChoiceField RemainInScene { get; set; }

    public NumRangeField X { get; set; }
    public NumRangeField Y { get; set; }
    public NumRangeField Z { get; set; }

    public NumRangeField Pitch { get; set; }
    public NumRangeField Yaw   { get; set; }
    public NumRangeField Roll  { get; set; }

    public BoolChoiceField UnkBool { get; set; }
}
