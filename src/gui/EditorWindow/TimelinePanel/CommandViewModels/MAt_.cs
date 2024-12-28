using System.Collections.Generic;

namespace EVTUI.ViewModels.TimelineCommands;

public class MAt_ : Generic
{
    public MAt_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Attach to Bone";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.BoneID = new IntSelectionField("Bone ID", this.Editable, this.CommandData.BoneId, new List<int>{this.CommandData.BoneId});
        this.ChildAssetID = new IntSelectionField("Attached Asset ID", this.Editable, this.CommandData.ChildObjectId, config.EventManager.AssetIDs);
        this.FrameDelay = new NumRangeField("Frame Delay", this.Editable, this.CommandData.FrameDelay, 0, config.EventManager.EventDuration-(command.FrameStart+command.FrameDuration), 1);
        this.RelativeXPosition = new NumEntryField("X", this.Editable, this.CommandData.RelativePosition[0], null, null, 0.1);
        this.RelativeYPosition = new NumEntryField("Y", this.Editable, this.CommandData.RelativePosition[1], null, null, 0.1);
        this.RelativeZPosition = new NumEntryField("Z", this.Editable, this.CommandData.RelativePosition[2], null, null, 0.1);
        this.XRotation = new NumRangeField("X", this.Editable, this.CommandData.Rotation[0], -180, 180, 0.1);
        this.YRotation = new NumRangeField("Y", this.Editable, this.CommandData.Rotation[1], -180, 180, 0.1);
        this.ZRotation = new NumRangeField("Z", this.Editable, this.CommandData.Rotation[2], -180, 180, 0.1);

        this.ParentModelPreviewVM = new GFDRenderingPanelViewModel();
        List<string> parentAssetPaths = config.EventManager.GetAssetPaths(this.Command.ObjectId, config.CpkList, config.VanillaExtractionPath);
        if (parentAssetPaths.Count > 0)
            this.ParentModelPreviewVM.sceneManager.QueuedLoads.Enqueue((parentAssetPaths[0], null, null, false));

        this.ChildModelPreviewVM = new GFDRenderingPanelViewModel();
        List<string> childAssetPaths = config.EventManager.GetAssetPaths(this.CommandData.ChildObjectId, config.CpkList, config.VanillaExtractionPath);
        if (childAssetPaths.Count > 0)
            this.ChildModelPreviewVM.sceneManager.QueuedLoads.Enqueue((childAssetPaths[0], null, null, false));
    }

    public GFDRenderingPanelViewModel ParentModelPreviewVM { get; set; }
    public GFDRenderingPanelViewModel ChildModelPreviewVM  { get; set; }

    public IntSelectionField AssetID         { get; set; }
    public IntSelectionField BoneID          { get; set; } // TODO: parse GFD and present as string selection...
    public IntSelectionField ChildAssetID    { get; set; }
    public NumRangeField     FrameDelay      { get; set; }
    public NumEntryField   RelativeXPosition { get; set; }
    public NumEntryField   RelativeYPosition { get; set; }
    public NumEntryField   RelativeZPosition { get; set; }
    public NumRangeField   XRotation         { get; set; }
    public NumRangeField   YRotation         { get; set; }
    public NumRangeField   ZRotation         { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId                = this.AssetID.Choice;
        this.CommandData.BoneId              = this.BoneID.Choice;
        this.CommandData.ChildObjectId       = this.ChildAssetID.Choice;
        this.CommandData.FrameDelay          = (int)this.FrameDelay.Value;
        this.CommandData.RelativePosition[0] = (float)this.RelativeXPosition.Value;
        this.CommandData.RelativePosition[1] = (float)this.RelativeYPosition.Value;
        this.CommandData.RelativePosition[2] = (float)this.RelativeZPosition.Value;
        this.CommandData.Rotation[0]         = (float)this.XRotation.Value;
        this.CommandData.Rotation[1]         = (float)this.YRotation.Value;
        this.CommandData.Rotation[2]         = (float)this.ZRotation.Value;
    }
}
