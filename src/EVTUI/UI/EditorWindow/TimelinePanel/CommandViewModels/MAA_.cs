using System.Collections.Generic;

namespace EVTUI.ViewModels.TimelineCommands;

public class MAA_ : Generic
{
    public MAA_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Blend Animation";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.PrimaryAnimationID = new IntSelectionField("Animation ID", this.Editable, this.CommandData.PrimaryAnimationIndex, new List<int>{this.CommandData.PrimaryAnimationIndex});
        this.SecondaryAnimationID = new IntSelectionField("Animation ID", this.Editable, this.CommandData.SecondaryAnimationIndex, new List<int>{this.CommandData.SecondaryAnimationIndex});

        this.PrimaryAnimPreviewVM = new GFDRenderingPanelViewModel();
        this.SecondaryAnimPreviewVM = new GFDRenderingPanelViewModel();
        List<string> assetPaths = config.EventManager.GetAssetPaths(this.Command.ObjectId, config.CpkList, config.VanillaExtractionPath);
        if (assetPaths.Count > 0)
        {
            List<string> animPaths = config.EventManager.GetAnimPaths(this.Command.ObjectId, true, true, config.CpkList, config.VanillaExtractionPath);
            if (animPaths.Count > 0)
            {
                this.PrimaryAnimPreviewVM.sceneManager.QueuedLoads.Enqueue((assetPaths[0], animPaths[0], this.CommandData.PrimaryAnimationIndex, true));
                this.SecondaryAnimPreviewVM.sceneManager.QueuedLoads.Enqueue((assetPaths[0], animPaths[0], this.CommandData.SecondaryAnimationIndex, true));
            }
        }
    }

    public GFDRenderingPanelViewModel PrimaryAnimPreviewVM   { get; set; }
    public GFDRenderingPanelViewModel SecondaryAnimPreviewVM { get; set; }

    public IntSelectionField AssetID              { get; set; }
    public IntSelectionField PrimaryAnimationID   { get; set; }
    public IntSelectionField SecondaryAnimationID { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId                    = this.AssetID.Choice;
        this.CommandData.PrimaryAnimationIndex   = this.PrimaryAnimationID.Choice;
        this.CommandData.SecondaryAnimationIndex = this.SecondaryAnimationID.Choice;
    }
}
