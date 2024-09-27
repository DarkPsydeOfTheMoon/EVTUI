using System;
using System.Collections.Generic;

namespace EVTUI.ViewModels.TimelineCommands;

public class MSD_ : Generic
{
    public MSD_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Set Data";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.X = new NumEntryField("X", this.Editable, this.CommandData.Position[0], null, null, 0.1);
        this.Y = new NumEntryField("Y", this.Editable, this.CommandData.Position[1], null, null, 0.1);
        this.Z = new NumEntryField("Z", this.Editable, this.CommandData.Position[2], null, null, 0.1);
        this.AnimationID = new IntSelectionField("Animation ID", this.Editable, this.CommandData.AnimationIndex, new List<int>{this.CommandData.AnimationIndex});
        this.AnimationSpeed = new NumEntryField("Animation Speed", this.Editable, this.CommandData.AnimationSpeed, 0.0, null, 0.1);
        this.Loop = new BoolChoiceField("Looping?", this.Editable, this.CommandData.LoopBool != 0);
        this.StartFrame = new NumEntryField("Start at Frame", this.Editable, this.CommandData.FirstFrameInd, 0, null, 1);
        this.EndFrame = new NumEntryField("End at Frame", this.Editable, this.CommandData.LastFrameInd, 0, null, 1);

        this.ModelPreviewVM = new GFDRenderingPanelViewModel();
        List<string> assetPaths = config.EventManager.GetAssetPaths(this.Command.ObjectId, config.CpkList, config.VanillaExtractionPath);
        if (assetPaths.Count > 0)
        {
            List<string> animPaths = config.EventManager.GetAnimPaths(this.Command.ObjectId, true, false, config.CpkList, config.VanillaExtractionPath);
            if (animPaths.Count > 0)
                this.ModelPreviewVM.sceneManager.QueuedLoads.Enqueue((assetPaths[0], animPaths[0], this.CommandData.AnimationIndex, false));
            else
                this.ModelPreviewVM.sceneManager.QueuedLoads.Enqueue((assetPaths[0], null, null, false));
        }
    }

    public GFDRenderingPanelViewModel ModelPreviewVM { get; set; }

    public IntSelectionField AssetID        { get; set; }
    public IntSelectionField AnimationID    { get; set; }
    public NumEntryField     X              { get; set; }
    public NumEntryField     Y              { get; set; }
    public NumEntryField     Z              { get; set; }
    public NumEntryField     AnimationSpeed { get; set; }
    public BoolChoiceField   Loop           { get; set; }
    public NumEntryField     StartFrame     { get; set; }
    public NumEntryField     EndFrame       { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId           = this.AssetID.Choice;
        this.CommandData.Position[0]    = (float)this.X.Value;
        this.CommandData.Position[1]    = (float)this.Y.Value;
        this.CommandData.Position[2]    = (float)this.Z.Value;
        this.CommandData.AnimationIndex = this.AnimationID.Choice;
        this.CommandData.AnimationSpeed = (float)this.AnimationSpeed.Value;
        this.CommandData.LoopBool       = Convert.ToInt32(this.Loop.Value);
        this.CommandData.FirstFrameInd  = (int)this.StartFrame.Value;
        this.CommandData.LastFrameInd   = (int)this.EndFrame.Value;
    }
}
