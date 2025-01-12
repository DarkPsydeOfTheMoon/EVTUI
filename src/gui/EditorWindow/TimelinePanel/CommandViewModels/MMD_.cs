using System;
using System.Collections.Generic;

namespace EVTUI.ViewModels.TimelineCommands;

public class MMD_ : Generic
{
    public MMD_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Movement";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);

        // movement
        this.InterpolationType = new StringSelectionField("Interpolation Type", this.Editable, Enum.GetName(typeof(InterpolationTypes), this.CommandData.InterpolationType), new List<string>(Enum.GetNames(typeof(InterpolationTypes))));
        this.NumControlGroups = new NumRangeField("Control Groups", this.Editable, this.CommandData.NumControlGroups, 1, 8, 1);
        this.MovementSpeed = new NumEntryField("Movement Speed", this.Editable, this.CommandData.MovementSpeed, 0.0, null, 0.1);
        this.MovementLoopBool = new BoolChoiceField("Loop Playback?", this.Editable, ((this.CommandData.Bitflag >> 5) & 1) != 0);
        this.DisableOrientationChange = new BoolChoiceField("Disable Orientation Change?", this.Editable, ((this.CommandData.Bitflag >> 4) & 1) != 0);
        this.StartSpeedType = new StringSelectionField("Start Speed Type", this.Editable, Enum.GetName(typeof(SpeedTypes), this.CommandData.StartSpeedType), new List<string>(Enum.GetNames(typeof(SpeedTypes))));
        this.FinalSpeedType = new StringSelectionField("Final Speed Type", this.Editable, Enum.GetName(typeof(SpeedTypes), this.CommandData.FinalSpeedType), new List<string>(Enum.GetNames(typeof(SpeedTypes))));

        // moving animation
        this.MovingAnimationEnabled = new BoolChoiceField("Enabled?", this.Editable, (this.CommandData.Bitflag & 1) == 0);
        this.MovingAnimationFromExt = new BoolChoiceField("From ext?", this.Editable, ((this.CommandData.Bitflag >> 2) & 1) != 0);
        this.MovingAnimationID = new IntSelectionField("Animation ID", this.Editable, this.CommandData.MovingAnimationID, new List<int>{this.CommandData.MovingAnimationID});
        this.MovingAnimationInterpolatedFrames = new NumEntryField("Interpolated Frames", this.Editable, this.CommandData.MovingAnimationInterpolatedFrames, 0, null, 1);
        this.MovingAnimationLoopBool = new BoolChoiceField("Loop Playback?", this.Editable, this.CommandData.MovingAnimationLoopBool != 0);
        this.MovingAnimationPlaybackSpeed = new NumEntryField("Playback Speed", this.Editable, this.CommandData.MovingAnimationPlaybackSpeed, 0.0, null, 0.1);
        this.MovingAnimationStartingFrame = new NumRangeField("Starting Frame", this.Editable, this.CommandData.MovingAnimationStartingFrame, 0, config.EventManager.EventDuration-(command.FrameStart+command.FrameDuration), 1);

        // waiting animation
        this.WaitingAnimationEnabled = new BoolChoiceField("Enabled?", this.Editable, ((this.CommandData.Bitflag >> 1) & 1) == 0);
        this.WaitingAnimationFromExt = new BoolChoiceField("From ext?", this.Editable, ((this.CommandData.Bitflag >> 3) & 1) != 0);
        this.WaitingAnimationID = new IntSelectionField("Animation ID", this.Editable, this.CommandData.WaitingAnimationID, new List<int>{this.CommandData.WaitingAnimationID});
        this.WaitingAnimationInterpolatedFrames = new NumEntryField("Interpolated Frames", this.Editable, this.CommandData.WaitingAnimationInterpolatedFrames, 0, null, 1);
        this.WaitingAnimationLoopBool = new BoolChoiceField("Loop Playback?", this.Editable, this.CommandData.WaitingAnimationLoopBool != 0);
        this.WaitingAnimationPlaybackSpeed = new NumEntryField("Playback Speed", this.Editable, this.CommandData.WaitingAnimationPlaybackSpeed, 0.0, null, 0.1);
        this.WaitingAnimationStartingFrame = new NumRangeField("Starting Frame", this.Editable, this.CommandData.WaitingAnimationStartingFrame, 0, config.EventManager.EventDuration-(command.FrameStart+command.FrameDuration), 1);

        this.MovingAnimationPreviewVM = new GFDRenderingPanelViewModel();
        this.WaitingAnimationPreviewVM = new GFDRenderingPanelViewModel();
        List<string> assetPaths = config.EventManager.GetAssetPaths(this.Command.ObjectId, config.CpkList, config.VanillaExtractionPath);
        if (assetPaths.Count > 0)
        {
            if (this.MovingAnimationEnabled.Value)
            {
                List<string> movingAnimPaths = config.EventManager.GetAnimPaths(this.Command.ObjectId, !this.MovingAnimationFromExt.Value, false, config.CpkList, config.VanillaExtractionPath);
                if (movingAnimPaths.Count > 0)
                    this.MovingAnimationPreviewVM.sceneManager.QueuedLoads.Enqueue((assetPaths[0], movingAnimPaths[0], this.MovingAnimationID.Choice, false));
            }
            if (this.WaitingAnimationEnabled.Value)
            {
                List<string> waitingAnimPaths = config.EventManager.GetAnimPaths(this.Command.ObjectId, !this.WaitingAnimationFromExt.Value, false, config.CpkList, config.VanillaExtractionPath);
                if (waitingAnimPaths.Count > 0)
                    this.WaitingAnimationPreviewVM.sceneManager.QueuedLoads.Enqueue((assetPaths[0], waitingAnimPaths[0], this.WaitingAnimationID.Choice, false));
            }
        }
    }

    public GFDRenderingPanelViewModel MovingAnimationPreviewVM   { get; set; }
    public GFDRenderingPanelViewModel WaitingAnimationPreviewVM  { get; set; }

    public IntSelectionField AssetID { get; set; }

    // movement
    public StringSelectionField InterpolationType        { get; set; }
    public NumRangeField        NumControlGroups         { get; set; }
    public NumEntryField        MovementSpeed            { get; set; }
    public BoolChoiceField      MovementLoopBool         { get; set; }
    public BoolChoiceField      DisableOrientationChange { get; set; }
    public StringSelectionField StartSpeedType           { get; set; }
    public StringSelectionField FinalSpeedType           { get; set; }

    // moving animation
    public BoolChoiceField   MovingAnimationEnabled            { get; set; }
    public BoolChoiceField   MovingAnimationFromExt            { get; set; }
    public IntSelectionField MovingAnimationID                 { get; set; }
    public NumEntryField     MovingAnimationInterpolatedFrames { get; set; }
    public BoolChoiceField   MovingAnimationLoopBool           { get; set; }
    public NumEntryField     MovingAnimationPlaybackSpeed      { get; set; }
    public NumRangeField     MovingAnimationStartingFrame      { get; set; }

    // waiting animation
    public BoolChoiceField   WaitingAnimationEnabled            { get; set; }
    public BoolChoiceField   WaitingAnimationFromExt            { get; set; }
    public IntSelectionField WaitingAnimationID                 { get; set; }
    public NumEntryField     WaitingAnimationInterpolatedFrames { get; set; }
    public BoolChoiceField   WaitingAnimationLoopBool           { get; set; }
    public NumEntryField     WaitingAnimationPlaybackSpeed      { get; set; }
    public NumRangeField     WaitingAnimationStartingFrame      { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId                    = this.AssetID.Choice;
    }

    public enum InterpolationTypes : int
    {
        Linear      = 0,
        BezierCurve = 1,
    }

    public enum SpeedTypes : byte
    {
        Fixed   = 0,
        Running = 1,
        Walking = 2,
    }
}
