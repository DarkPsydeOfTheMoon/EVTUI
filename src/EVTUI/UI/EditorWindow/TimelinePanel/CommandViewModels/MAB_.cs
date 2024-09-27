using System.Collections.Generic;

namespace EVTUI.ViewModels.TimelineCommands;

public class MAB_ : Generic
{
    public MAB_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Armature Animation";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        //if (((this.CommandData.AnimationMode >> 1) & 1) == 1)
        this.PrimaryAnimationID = new IntSelectionField("Animation ID", this.Editable, this.CommandData.PrimaryAnimationIndex, new List<int>{this.CommandData.PrimaryAnimationIndex});
        //if ((this.CommandData.AnimationMode & 1) != 1)
        this.SecondaryAnimationID = new IntSelectionField("Animation ID", this.Editable, this.CommandData.SecondaryAnimationIndex, new List<int>{this.CommandData.SecondaryAnimationIndex});
        this.HasPrimaryAnimation = new BoolChoiceField("Skip to final frame?", this.Editable, ((this.CommandData.AnimationMode >> 1) & 1) != 0);
        this.HasSecondaryAnimation = new BoolChoiceField("Skip to final frame?", this.Editable, (this.CommandData.AnimationMode & 1) == 0);
        this.PrimaryAnimationFromSecondaryFile = new BoolChoiceField("From Secondary File?", this.Editable, ((this.CommandData.AnimationMode >> 4) & 1) != 0);
        this.SecondaryAnimationFromSecondaryFile = new BoolChoiceField("From Secondary File?", this.Editable, ((this.CommandData.AnimationMode >> 5) & 1) != 0);
        this.PrimaryAnimationSpeed = new NumEntryField("Animation Speed", this.Editable, this.CommandData.PrimaryAnimationSpeed, 0.0, null, 0.1);
        this.SecondaryAnimationSpeed = new NumEntryField("Animation Speed", this.Editable, this.CommandData.SecondaryAnimationSpeed, 0.0, null, 0.1);

        this.PrimaryAnimPreviewVM = new GFDRenderingPanelViewModel();
        this.SecondaryAnimPreviewVM = new GFDRenderingPanelViewModel();
        List<string> assetPaths = config.EventManager.GetAssetPaths(this.Command.ObjectId, config.CpkList, config.VanillaExtractionPath);
        if (assetPaths.Count > 0)
        {
            //if (this.HasPrimaryAnimation.Value)
            //{
                List<string> primaryAnimPaths = config.EventManager.GetAnimPaths(this.Command.ObjectId, !this.PrimaryAnimationFromSecondaryFile.Value, false, config.CpkList, config.VanillaExtractionPath);
                if (primaryAnimPaths.Count > 0)
                    this.PrimaryAnimPreviewVM.sceneManager.QueuedLoads.Enqueue((assetPaths[0], primaryAnimPaths[0], this.CommandData.PrimaryAnimationIndex, false));
            //}
            if (this.HasSecondaryAnimation.Value)
            {
                List<string> secondaryAnimPaths = config.EventManager.GetAnimPaths(this.Command.ObjectId, !this.SecondaryAnimationFromSecondaryFile.Value, false, config.CpkList, config.VanillaExtractionPath);
                if (secondaryAnimPaths.Count > 0)
                    this.SecondaryAnimPreviewVM.sceneManager.QueuedLoads.Enqueue((assetPaths[0], secondaryAnimPaths[0], this.CommandData.SecondaryAnimationIndex, false));
            }
        }
    }

    public GFDRenderingPanelViewModel PrimaryAnimPreviewVM   { get; set; }
    public GFDRenderingPanelViewModel SecondaryAnimPreviewVM { get; set; }

    public IntSelectionField AssetID                             { get; set; }
    public IntSelectionField PrimaryAnimationID                  { get; set; }
    public IntSelectionField SecondaryAnimationID                { get; set; }
    public BoolChoiceField   HasSecondaryAnimation               { get; set; }
    public BoolChoiceField   HasPrimaryAnimation                 { get; set; }
    public BoolChoiceField   PrimaryAnimationFromSecondaryFile   { get; set; }
    public BoolChoiceField   SecondaryAnimationFromSecondaryFile { get; set; }
    public NumEntryField   PrimaryAnimationSpeed               { get; set; }
    public NumEntryField   SecondaryAnimationSpeed             { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId                    = this.AssetID.Choice;
        this.CommandData.PrimaryAnimationIndex   = this.PrimaryAnimationID.Choice;
        this.CommandData.SecondaryAnimationIndex = this.SecondaryAnimationID.Choice;
        // TODO: recompose the bitflags... maybe make a helper function for that
        this.CommandData.PrimaryAnimationSpeed   = (float)this.PrimaryAnimationSpeed.Value;
        this.CommandData.SecondaryAnimationSpeed = (float)this.SecondaryAnimationSpeed.Value;
    }
}
