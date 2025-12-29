using System;
using System.Collections.ObjectModel;
using System.Linq;

using ReactiveUI;

namespace EVTUI.ViewModels;

public class TimelinePanelViewModel : ViewModelBase
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public DataManager Config { get; private set; }
    public Clipboard SharedClipboard { get; private set; }

    public CommonViewModels Common { get; private set; }
    public TimelineViewModel TimelineContent { get; private set; }
    public TimelineCommands.Generic ActiveCommand { get; set; }
    protected CommandPointer _activeCommandPointer;
    protected Category _activeCategory;

    public ObservableCollection<string> AddableCodes { get; set; } //= new ObservableCollection<string>();

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public TimelinePanelViewModel(DataManager dataManager, CommonViewModels commonVMs, Clipboard clipboard)
    {
        this.Config          = dataManager;
        this.Common          = commonVMs;
        this.TimelineContent = commonVMs.Timeline;
        this.SharedClipboard = clipboard;
        this.AddableCodes    = new ObservableCollection<string>();
    }

    public void Dispose()
    {
        this.Config = null;
        this.Common = null;
        this.TimelineContent = null;
        this.SharedClipboard = null;
        this.AddableCodes.Clear();
    }

    public void InsertFrames(int afterFrame, int numberFrames)
    {
        this.TimelineContent.InsertFrames(afterFrame, numberFrames);
        if (!(this.ActiveCommand is null) && this.ActiveCommand.Basics.StartingFrame.Value > afterFrame)
            this.ActiveCommand.Basics.StartingFrame.Value += numberFrames;
    }

    public void ClearFrames(int startingFrame, int endingFrame, bool deleteFrames)
    {
        if (!(this.ActiveCommand is null) && this.ActiveCommand.Basics.StartingFrame.Value >= startingFrame && this.ActiveCommand.Basics.StartingFrame.Value <= endingFrame)
            this.UnsetActiveCommand();

        foreach (Category cat in this.TimelineContent.Categories)
            foreach (CommandPointer cmd in cat.Commands.ToList())
                if (cmd.Frame >= startingFrame && cmd.Frame <= endingFrame)
                {
                    bool success = this.Config.EventManager.DeleteCommand(cmd.CmdIndex, cmd.IsAudioCmd);
                    if (success)
                       this.TimelineContent.DeleteCommand(cmd);
                }

        this.TimelineContent.ClearFrames(startingFrame, endingFrame, deleteFrames);

        if (deleteFrames && !(this.ActiveCommand is null) && this.ActiveCommand.Basics.StartingFrame.Value > endingFrame)
            this.ActiveCommand.Basics.StartingFrame.Value -= 1 + endingFrame - startingFrame;
    }

    public void SetAddableCodes(Category category)
    {
        this.AddableCodes.Clear();
        foreach (string code in this.Config.EventManager.AddableCodes)
            if (TimelineViewModel.CodeToCategory(code, false) + 1 == category.Index)
                this.AddableCodes.Add(code);
    }

    public void SetActiveCommand(CommandPointer cmd)
    {
        this._activeCommandPointer = cmd;
        this._activeCategory = this.TimelineContent.Categories[TimelineViewModel.CodeToCategory(cmd.Code, false)];
        if (cmd.CommandType is null)
            this.ActiveCommand = new TimelineCommands.Generic(this.Config, this.Common, cmd);
        else
            this.ActiveCommand = (TimelineCommands.Generic)Activator.CreateInstance(cmd.CommandType, new object[] { this.Config, this.Common, cmd });

        this.WhenAnyValue(x => x.ActiveCommand.Basics.StartingFrame.Value).Subscribe(x => {
            this._activeCategory.MoveCommand(this._activeCommandPointer, (int)this.ActiveCommand.Basics.StartingFrame.Value);
        });
        this.WhenAnyValue(x => x.ActiveCommand.Basics.FrameCount.Value).Subscribe(x => {
            this._activeCommandPointer.Duration = (int)this.ActiveCommand.Basics.FrameCount.Value;
        });
    }

    public void UnsetActiveCommand()
    {
        this._activeCommandPointer = null;
        this._activeCategory = null;;
        this.ActiveCommand = null;
    }

    public bool DeleteCommand(CommandPointer cmd)
    {
        bool success = this.Config.EventManager.DeleteCommand(cmd.CmdIndex, cmd.IsAudioCmd);
        if (success)
            this.TimelineContent.DeleteCommand(cmd);
        return success;
    }

    public void CopyCommand(CommandPointer cmd)
    {
        this.SharedClipboard.CopyCommand(cmd);
    }

    public void CopyPosition(Position3D pos)
    {
        this.SharedClipboard.CopyPosition(pos);
    }

    public void CopyRotation(RotationWidget rot)
    {
        this.SharedClipboard.CopyRotation(rot);
    }

    public void PasteCommand(int frame)
    {
        // this shouldn't happen because pasting is disabled if so, but...
        if (this.SharedClipboard.CopiedCommand is null)
            return;

        int newCmdIndex = this.Config.EventManager.CopyCommandToNewFrame(this.SharedClipboard.CopiedCommand.Command, this.SharedClipboard.CopiedCommand.CommandData, this.SharedClipboard.CopiedCommand.IsAudioCmd, frame);
        if (newCmdIndex >= 0)
        {
            CommandPointer newCmd = new CommandPointer(this.Config, this.SharedClipboard.CopiedCommand.Code, this.SharedClipboard.CopiedCommand.IsAudioCmd, newCmdIndex, frame, this.SharedClipboard.CopiedCommand.Duration, (frame >= TimelineContent.StartingFrame && frame < TimelineContent.FrameCount));
            this.TimelineContent.AddCommand(newCmd);
        }
    }

    public void PastePosition(Position3D pos)
    {
        // this shouldn't happen because pasting is disabled if so, but...
        if (this.SharedClipboard.CopiedPosition is null)
            return;

        pos.X.Value = this.SharedClipboard.CopiedPosition.X.Value;
        pos.Y.Value = this.SharedClipboard.CopiedPosition.Y.Value;
        pos.Z.Value = this.SharedClipboard.CopiedPosition.Z.Value;
    }

    public void PasteRotation(RotationWidget rot)
    {
        // this shouldn't happen because pasting is disabled if so, but...
        if (this.SharedClipboard.CopiedRotation is null)
            return;

        if (rot.RotationEnabled.Editable)
            rot.RotationEnabled.Value = this.SharedClipboard.CopiedRotation.RotationEnabled.Value;
        if (rot.PitchEnabled.Editable)
            rot.PitchEnabled.Value = this.SharedClipboard.CopiedRotation.PitchEnabled.Value;
        if (rot.YawEnabled.Editable)
            rot.YawEnabled.Value = this.SharedClipboard.CopiedRotation.YawEnabled.Value;
        if (rot.RollEnabled.Editable)
            rot.RollEnabled.Value = this.SharedClipboard.CopiedRotation.RollEnabled.Value;

        rot.PitchDegrees.Value = this.SharedClipboard.CopiedRotation.PitchDegrees.Value;
        rot.YawDegrees.Value = this.SharedClipboard.CopiedRotation.YawDegrees.Value;
        rot.RollDegrees.Value = this.SharedClipboard.CopiedRotation.RollDegrees.Value;
    }

    public void NewCommand(string code, int frame)
    {
        bool isAudio = ECS.ValidEcsCommands.Contains(code);
        int newCmdIndex = this.Config.EventManager.NewCommand(code, frame);
        if (newCmdIndex >= 0)
        {
            int len = ((isAudio) ? this.Config.EventManager.EventSoundCommands : this.Config.EventManager.EventCommands)[newCmdIndex].FrameDuration;
            CommandPointer newCmd = new CommandPointer(this.Config, code, isAudio, newCmdIndex, frame, len, (frame >= TimelineContent.StartingFrame && frame < TimelineContent.FrameCount));
            this.TimelineContent.AddCommand(newCmd);
        }
    }

    public void PlayCueFromSource(string source, int cueId, int trackIndex)
    {
        if (this.Config.AudioManager.AcbByType[source].Count > 0)
        {
            this.Config.AudioManager.ActiveACB = this.Config.AudioManager.AcbByType[source][0];
            this.Config.AudioManager.PlayCueTrack((uint)cueId, trackIndex, this.Config.ProjectManager.AdxKey);
        }
    }

}
