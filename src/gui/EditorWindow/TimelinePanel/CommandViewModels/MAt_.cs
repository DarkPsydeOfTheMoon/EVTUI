using System.Collections.Generic;

namespace EVTUI.ViewModels.TimelineCommands;

public class MAt_ : Generic
{
    public MAt_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Attachment";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);

        this.HelperID = new NumEntryField("Helper ID", this.Editable, this.CommandData.HelperId, 0, 9999, 1);
        this.ChildAssetID = new IntSelectionField("Attached Asset ID", this.Editable, this.CommandData.ChildObjectId, config.EventManager.AssetIDs);

        this.XOffset = new NumRangeField("X", this.Editable, this.CommandData.RelativePosition[0], -99999, 99999, 1);
        this.YOffset = new NumRangeField("Y", this.Editable, this.CommandData.RelativePosition[1], -99999, 99999, 1);
        this.ZOffset = new NumRangeField("Z", this.Editable, this.CommandData.RelativePosition[2], -99999, 99999, 1);

        this.Pitch = new NumRangeField("Pitch", this.Editable, this.CommandData.Rotation[0], -180, 180, 1);
        this.Yaw = new NumRangeField("Yaw", this.Editable, this.CommandData.Rotation[1], -180, 180, 1);
        this.Roll = new NumRangeField("Roll", this.Editable, this.CommandData.Rotation[2], -180, 180, 1);

        this.ParentModelPreviewVM = new ModelPreviewWidget(config, this.AssetID);
        this.ChildModelPreviewVM = new ModelPreviewWidget(config, this.ChildAssetID);

        this.UnkBool = new BoolChoiceField("Unknown", this.Editable, this.CommandData.Flags[4]);
    }

    public ModelPreviewWidget ParentModelPreviewVM { get; set; }
    public ModelPreviewWidget ChildModelPreviewVM  { get; set; }

    public IntSelectionField AssetID { get; set; }

    public NumEntryField     HelperID     { get; set; } // TODO: parse GFD and present as string selection...
    public IntSelectionField ChildAssetID { get; set; }

    public NumRangeField XOffset { get; set; }
    public NumRangeField YOffset { get; set; }
    public NumRangeField ZOffset { get; set; }

    public NumRangeField Pitch { get; set; }
    public NumRangeField Yaw   { get; set; }
    public NumRangeField Roll  { get; set; }

    public BoolChoiceField UnkBool { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        this.CommandData.Flags[4] = this.UnkBool.Value;

        this.CommandData.HelperId       = (uint)this.HelperID.Value;
        this.CommandData.ChildObjectId  = this.ChildAssetID.Choice;

        this.CommandData.RelativePosition[0] = (float)this.XOffset.Value;
        this.CommandData.RelativePosition[1] = (float)this.YOffset.Value;
        this.CommandData.RelativePosition[2] = (float)this.ZOffset.Value;

        this.CommandData.Rotation[0]         = (float)this.Pitch.Value;
        this.CommandData.Rotation[1]         = (float)this.Yaw.Value;
        this.CommandData.Rotation[2]         = (float)this.Roll.Value;
    }
}
