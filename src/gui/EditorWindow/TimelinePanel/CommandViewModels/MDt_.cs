using System.Collections.Generic;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class MDt_ : Generic
{
    public MDt_(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Model: Detachment";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);

        this.HelperID = new NumEntryField("Helper ID", this.Editable, this.CommandData.HelperId, 0, 9999, 1);
        this.ChildAssetID = new IntSelectionField("Attached Asset ID", this.Editable, this.CommandData.ChildObjectId, config.EventManager.AssetIDs);

        this.RemainInScene = new BoolChoiceField("Keep object in scene after detaching?", this.Editable, this.CommandData.Flags[3]);

        this.X = new NumRangeField("X", this.Editable, this.CommandData.Position[0], -99999, 99999, 1);
        this.Y = new NumRangeField("Y", this.Editable, this.CommandData.Position[1], -99999, 99999, 1);
        this.Z = new NumRangeField("Z", this.Editable, this.CommandData.Position[2], -99999, 99999, 1);

        this.Pitch = new NumRangeField("Pitch", this.Editable, this.CommandData.Rotation[0], -180, 180, 1);
        this.Yaw = new NumRangeField("Yaw", this.Editable, this.CommandData.Rotation[1], -180, 180, 1);
        this.Roll = new NumRangeField("Roll", this.Editable, this.CommandData.Rotation[2], -180, 180, 1);

        this.UnkBool = new BoolChoiceField("Unknown", this.Editable, this.CommandData.Flags[4]);

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

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        this.CommandData.HelperId      = (uint)this.HelperID.Value;
        this.CommandData.ChildObjectId = this.ChildAssetID.Choice;

        this.CommandData.Flags[3] = this.RemainInScene.Value;
        this.CommandData.Flags[4] = this.UnkBool.Value;

        this.CommandData.Position[0] = (float)this.X.Value;
        this.CommandData.Position[1] = (float)this.Y.Value;
        this.CommandData.Position[2] = (float)this.Z.Value;

        this.CommandData.Rotation[0] = (float)this.Pitch.Value;
        this.CommandData.Rotation[1] = (float)this.Yaw.Value;
        this.CommandData.Rotation[2] = (float)this.Roll.Value;
    }
}
