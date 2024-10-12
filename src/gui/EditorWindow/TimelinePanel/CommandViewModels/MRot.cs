using System;
using System.Collections.Generic;

namespace EVTUI.ViewModels.TimelineCommands;

public class MRot : Generic
{
    public MRot(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Rotate";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.FrameDelay = new NumEntryField("Frame Delay", this.Editable, this.CommandData.FrameDelay, 0, config.EventManager.EventDuration-(command.FrameStart+command.FrameDuration), 1);
        this.FrameDuration = new NumEntryField("Frame Duration", this.Editable, (int)this.CommandData.FrameDuration, 0, config.EventManager.EventDuration-(command.FrameStart+command.FrameDuration), 1);
        this.X = new NumEntryField("X", this.Editable, this.CommandData.Rotation[0], null, null, 0.1);
        this.Y = new NumEntryField("Y", this.Editable, this.CommandData.Rotation[1], null, null, 0.1);
        this.Z = new NumEntryField("Z", this.Editable, this.CommandData.Rotation[2], null, null, 0.1);
        this.Animation1ID = new IntSelectionField("Index", this.Editable, this.CommandData.Animation1Ind, new List<int>{this.CommandData.Animation1Ind});
        this.Animation1Speed = new NumEntryField("Speed", this.Editable, this.CommandData.Animation1Speed, 0.0, null, 0.1);
        this.Animation1Loop = new BoolChoiceField("Looping?", this.Editable, this.CommandData.Animation1LoopBool != 0);
        this.Animation2ID = new IntSelectionField("Index", this.Editable, this.CommandData.Animation2Ind, new List<int>{this.CommandData.Animation2Ind});
        this.Animation2Speed = new NumEntryField("Speed", this.Editable, this.CommandData.Animation2Speed, 0.0, null, 0.1);
        this.Animation2Loop = new BoolChoiceField("Looping?", this.Editable, this.CommandData.Animation2LoopBool != 0);
        //this.StartFrame = new NumEntryField("Start at Frame", this.Editable, this.CommandData.FirstFrameInd, 0, null, 1);
        //this.EndFrame = new NumEntryField("End at Frame", this.Editable, this.CommandData.LastFrameInd, 0, null, 1);
    }

    public IntSelectionField AssetID         { get; set; }
    public IntSelectionField Animation1ID    { get; set; }
    public IntSelectionField Animation2ID    { get; set; }
    public NumEntryField     FrameDelay      { get; set; }
    public NumEntryField     FrameDuration   { get; set; }
    public NumEntryField     X               { get; set; }
    public NumEntryField     Y               { get; set; }
    public NumEntryField     Z               { get; set; }
    public NumEntryField     Animation1Speed { get; set; }
    public NumEntryField     Animation2Speed { get; set; }
    public BoolChoiceField   Animation1Loop  { get; set; }
    public BoolChoiceField   Animation2Loop  { get; set; }
    //public NumEntryField     StartFrame      { get; set; }
    //public NumEntryField     EndFrame        { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId               = this.AssetID.Choice;
        this.CommandData.FrameDelay         = (ushort)this.FrameDelay.Value;
        this.CommandData.FrameDuration      = (uint)this.FrameDuration.Value;
        this.CommandData.Rotation[0]        = (float)this.X.Value;
        this.CommandData.Rotation[1]        = (float)this.Y.Value;
        this.CommandData.Rotation[2]        = (float)this.Z.Value;
        this.CommandData.Animation1Ind      = this.Animation1ID.Choice;
        this.CommandData.Animation1Speed    = (float)this.Animation1Speed.Value;
        this.CommandData.Animation1LoopBool = Convert.ToUInt32(this.Animation1Loop.Value);
        this.CommandData.Animation2Ind      = this.Animation2ID.Choice;
        this.CommandData.Animation2Speed    = (float)this.Animation2Speed.Value;
        this.CommandData.Animation2LoopBool = Convert.ToUInt32(this.Animation1Loop.Value);
        //this.CommandData.FirstFrameInd  = (int)this.StartFrame.Value;
        //this.CommandData.LastFrameInd   = (int)this.EndFrame.Value;
    }
}
