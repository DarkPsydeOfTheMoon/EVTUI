using System.Collections.Generic;

namespace EVTUI.ViewModels.TimelineCommands;

public class MAtO : Generic
{
    public MAtO(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Attach to Origin";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.ChildAssetID = new IntSelectionField("Attached Asset ID", this.Editable, this.CommandData.ChildObjectId, config.EventManager.AssetIDs);
        this.FrameDelay = new NumEntryField("Frame Delay", this.Editable, this.CommandData.FrameDelay, 0, config.EventManager.EventDuration-(command.FrameStart+command.FrameDuration), 1);
        this.RelativeXPosition = new NumEntryField("X", this.Editable, this.CommandData.RelativePosition[0], null, null, 0.1);
        this.RelativeYPosition = new NumEntryField("Y", this.Editable, this.CommandData.RelativePosition[1], null, null, 0.1);
        this.RelativeZPosition = new NumEntryField("Z", this.Editable, this.CommandData.RelativePosition[2], null, null, 0.1);
        this.XRotation = new NumEntryField("X", this.Editable, this.CommandData.Rotation[0], -180, 180, 0.1);
        this.YRotation = new NumEntryField("Y", this.Editable, this.CommandData.Rotation[1], -180, 180, 0.1);
        this.ZRotation = new NumEntryField("Z", this.Editable, this.CommandData.Rotation[2], -180, 180, 0.1);

        this.ParentModelPreviewVM = new GFDRenderingPanelViewModel();
        List<string> parentAssetPaths = config.EventManager.GetAssetPaths(this.Command.ObjectId, config.CpkList, config.ModPath);
        if (parentAssetPaths.Count > 0)
            this.ParentModelPreviewVM.sceneManager.QueuedLoads.Enqueue((parentAssetPaths[0], null, null, false));

        this.ChildModelPreviewVM = new GFDRenderingPanelViewModel();
        List<string> childAssetPaths = config.EventManager.GetAssetPaths(this.CommandData.ChildObjectId, config.CpkList, config.ModPath);
        if (childAssetPaths.Count > 0)
            this.ChildModelPreviewVM.sceneManager.QueuedLoads.Enqueue((childAssetPaths[0], null, null, false));
    }

    public GFDRenderingPanelViewModel ParentModelPreviewVM { get; set; }
    public GFDRenderingPanelViewModel ChildModelPreviewVM  { get; set; }

    public IntSelectionField AssetID           { get; set; }
    public IntSelectionField ChildAssetID      { get; set; }
    public NumEntryField     FrameDelay        { get; set; }
    public NumEntryField     RelativeXPosition { get; set; }
    public NumEntryField     RelativeYPosition { get; set; }
    public NumEntryField     RelativeZPosition { get; set; }
    public NumEntryField     XRotation         { get; set; }
    public NumEntryField     YRotation         { get; set; }
    public NumEntryField     ZRotation         { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId                = this.AssetID.Choice;
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
