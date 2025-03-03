using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ReactiveUI;
//using ReactiveUI.Fody.Helpers;

namespace EVTUI.ViewModels;

public class Timeline : ReactiveObject
{

    public NumEntryField FrameRate                { get; set; }
    //public NumEntryField FrameCount               { get; set; }
    public NumEntryField FrameDuration            { get; set; }
    public BoolChoiceField StartingFrameEnabled   { get; set; }
    public NumEntryField StartingFrameEntry       { get; set; }
    //public NumEntryField CinemascopeStartingFrame { get; set; }

    public Timeline(DataManager dataManager)
    {
        EVT evt = (EVT)dataManager.EventManager.SerialEvent;
        //this.FrameCount = new NumEntryField("Frame Count", !dataManager.ReadOnly, (int)evt.FrameCount, 0, 99999, 1);
        this.FrameRate = new NumEntryField("Frame Rate", !dataManager.ReadOnly, (int)evt.FrameRate, 1, 255, 1);
        this.FrameDuration = new NumEntryField("Frame Count", !dataManager.ReadOnly, (int)evt.FrameCount, 0, 99999, 1);
        this.StartingFrameEnabled = new BoolChoiceField("Set Delayed Starting Frame?", !dataManager.ReadOnly, evt.Flags[0]);
        this.StartingFrameEntry = new NumEntryField("Starting Frame", !dataManager.ReadOnly, (int)evt.StartingFrame, 0, 9999, 1);

        _frameCount = (int)this.FrameDuration.Value;
        _startingFrame = (int)this.StartingFrameEntry.Value;

        _maxMarks = evt.MarkerFrameCount;
        this.MarkedFrames = new ObservableCollection<int>();
        foreach (int frameInd in evt.MarkerFrame)
            if (frameInd > -1 && !(this.MarkedFrames.Contains(frameInd)))
                this.MarkedFrames.Add(frameInd);
        //this.WhenAnyValue(x => x.MarkedFrames).Subscribe(x => 
        //this.MarkedFrames.ToObservableChangeSet(x => 
        this.MarkedFrames.CollectionChanged += (sender, e) =>
        {
            evt.MarkerFrame = new int[evt.MarkerFrameCount];
            for (int i=0; i<evt.MarkerFrameCount; i++)
                evt.MarkerFrame[i] = -1;
            for (int i=0; i<this.MarkedFrames.Count; i++)
                if (i < evt.MarkerFrameCount)
                    evt.MarkerFrame[i] = this.MarkedFrames[i];
        }; //);

        this.Frames = new ObservableCollection<Frame>();
        for (int i=0; i<_frameCount; i++)
            this.Frames.Add(new Frame(i, (i >= _startingFrame && i < _frameCount), this.MarkedFrames.Contains(i)));
        this.ActiveFrame = 0;

        this.Categories = new ObservableCollection<Category>();
        this.Categories.Add(new Category("Field",    1,  this.FrameCount));
        this.Categories.Add(new Category("Env",      2,  this.FrameCount));
        this.Categories.Add(new Category("Camera",   3,  this.FrameCount));
        this.Categories.Add(new Category("Model",    4,  this.FrameCount));
        this.Categories.Add(new Category("Effect",   5,  this.FrameCount));
        this.Categories.Add(new Category("Crowd",    6,  this.FrameCount));
        this.Categories.Add(new Category("Image",    7,  this.FrameCount));
        this.Categories.Add(new Category("Movie",    8,  this.FrameCount));
        this.Categories.Add(new Category("Dialogue", 9,  this.FrameCount));
        this.Categories.Add(new Category("Texture",  10, this.FrameCount));
        this.Categories.Add(new Category("UI",       11, this.FrameCount));
        this.Categories.Add(new Category("Post",     12, this.FrameCount));
        this.Categories.Add(new Category("Audio",    13, this.FrameCount));
        this.Categories.Add(new Category("Script",   14, this.FrameCount));
        this.Categories.Add(new Category("Timing",   15, this.FrameCount));
        this.Categories.Add(new Category("Hardware", 16, this.FrameCount));
        this.Categories.Add(new Category("Other",    17, this.FrameCount));

        this.WhenAnyValue(x => x.FrameDuration.Value).Subscribe(x => 
        {
            evt.FrameCount = (int)this.FrameDuration.Value;
            this.FrameCount = (int)this.FrameDuration.Value;

            this.Frames.Clear();
            for (int i=0; i<_frameCount; i++)
                this.Frames.Add(new Frame(i, (i >= _startingFrame && i < _frameCount), this.MarkedFrames.Contains(i)));

            foreach (Category _cat in this.Categories)
            {
                _cat.FrameCount = this.FrameCount;
                foreach (CommandPointer _cmd in _cat.Commands)
                    _cmd.IsInPlayRange = (_cmd.Frame >= _startingFrame && _cmd.Frame < _frameCount);
            }
        });

        this.WhenAnyValue(x => x.StartingFrameEnabled.Value, x => x.StartingFrameEntry.Value).Subscribe(x => 
        {
            evt.Flags[0] = this.StartingFrameEnabled.Value;
            evt.StartingFrame = (short)this.StartingFrameEntry.Value;
            this.StartingFrame = (this.StartingFrameEnabled.Value) ? (int)this.StartingFrameEntry.Value : 0;
            foreach (Frame frame in this.Frames)
                frame.IsInPlayRange = (frame.Index >= _startingFrame && frame.Index < _frameCount);
            foreach (Category _cat in this.Categories)
                foreach (CommandPointer _cmd in _cat.Commands)
                    _cmd.IsInPlayRange = (_cmd.Frame >= _startingFrame && _cmd.Frame < _frameCount);
        });

        for (int j=0; j<dataManager.EventManager.EventSoundCommands.Length; j++)
        {
            string code = dataManager.EventManager.EventSoundCommands[j].CommandCode;
            int i = dataManager.EventManager.EventSoundCommands[j].FrameStart;
            int len = dataManager.EventManager.EventSoundCommands[j].FrameDuration;
            //if (i >= 0 && i < this.Frames.Count)
            //{
                CommandPointer newCmd = new CommandPointer(dataManager, code, true, j, i, len, (i >= _startingFrame && i < _frameCount));
                int catInd = Timeline.CodeToCategory(code, true);
                //if (catInd > -1)
                this.Categories[catInd].AddCommand(newCmd);
                this.Categories[catInd].IsOpen = true;
            //}
        }
        for (int j=0; j<dataManager.EventManager.EventCommands.Length; j++)
        {
            string code = dataManager.EventManager.EventCommands[j].CommandCode;
            int i = dataManager.EventManager.EventCommands[j].FrameStart;
            int len = dataManager.EventManager.EventCommands[j].FrameDuration;
            //if (i >= 0 && i < this.Frames.Count)
            //{
                CommandPointer newCmd = new CommandPointer(dataManager, code, false, j, i, len, (i >= _startingFrame && i < _frameCount));
                int catInd = Timeline.CodeToCategory(code, false);
                //if (catInd > -1)
                this.Categories[catInd].AddCommand(newCmd);
                this.Categories[catInd].IsOpen = true;
            //}
        }
    }

    public static int CodeToCategory(string code, bool isAudio)
    {
        if (isAudio)
            return 12;
        int catInd = 16;
        if (code == "FbEn" || code == "Flbk" || code.StartsWith("Im"))
            catInd = 6;
        else if (code == "Msg_" || code == "MsgR" || code == "Cht_")
            catInd = 8;
        else if (code == "LBX_" || code == "Date")
            catInd = 10;
        else if (code == "AlEf" || code.StartsWith("G"))
            catInd = 11;
        else if (code == "Scr_")
            catInd = 13;
        else if (code == "Chap" || code == "FrJ_")
            catInd = 14;
        else if (code == "PRum" || code == "TrMc")
            catInd = 15;
        else if (code.StartsWith("En"))
            catInd = 1;
        else if (code.StartsWith("Cw"))
            catInd = 5;
        else if (code.StartsWith("Mv"))
            catInd = 7;
        else if (code.StartsWith("Fd"))
            catInd = 10;
        else if (code.StartsWith("F"))
            catInd = 0;
        else if (code.StartsWith("C"))
            catInd = 2;
        else if (code.StartsWith("M"))
            catInd = 3;
        else if (code.StartsWith("E"))
            catInd = 4;
        else if (code.StartsWith("T"))
            catInd = 9;
        else if (code.StartsWith("P"))
            catInd = 11;
        //else
        //    catInd = 16;
        return catInd;
    }

    public void TryToggleFrameMarker(int frameInd)
    {
        if (frameInd < 0 || frameInd >= this.Frames.Count)
            return;

        if (this.Frames[frameInd].IsMarked)
        {
            this.MarkedFrames.Remove(frameInd);
            this.Frames[frameInd].IsMarked = false;
        }
        else if (this.MarkedFrames.Count < _maxMarks)
        {
            this.MarkedFrames.Add(frameInd);
            this.Frames[frameInd].IsMarked = true;
        }
    }

    public void AddCommand(CommandPointer newCmd)
    {
        int catInd = Timeline.CodeToCategory(newCmd.Code, newCmd.IsAudioCmd);
        if (catInd > -1)
            this.Categories[catInd].AddCommand(newCmd);
    }

    public void DeleteCommand(CommandPointer cmd)
    {
        int catInd = Timeline.CodeToCategory(cmd.Code, cmd.IsAudioCmd);
        if (catInd > -1)
        {
            foreach (Category _cat in this.Categories)
                foreach (CommandPointer _cmd in _cat.Commands)
                    if (_cmd.IsAudioCmd == cmd.IsAudioCmd && _cmd.CmdIndex > cmd.CmdIndex)
                        _cmd.CmdIndex -= 1;
            this.Categories[catInd].DeleteCommand(cmd);
        }
    }

    private int _frameCount;
    public int FrameCount
    {
        get => _frameCount;
        set => this.RaiseAndSetIfChanged(ref _frameCount, value);
    }

    private int _startingFrame;
    public int StartingFrame
    {
        get => _startingFrame;
        set => this.RaiseAndSetIfChanged(ref _startingFrame, value);
    }

    private int                       _maxMarks;
    public ObservableCollection<int> MarkedFrames;

    public ObservableCollection<Frame>    Frames     { get; set; }
    public ObservableCollection<Category> Categories { get; set; }

    private int _activeFrame;
    public int ActiveFrame
    {
        get => _activeFrame;
        set => this.RaiseAndSetIfChanged(ref _activeFrame, value);
    }
}

public class Frame : ViewModelBase
{
    public Frame(int index, bool isInPlayRange, bool isMarked)
    {
        this.Index    = index;
        this._isInPlayRange = isInPlayRange;
        this._isMarked = isMarked;
    }

    public int Index { get; set; }

    private bool _isInPlayRange;
    public bool IsInPlayRange
    {
        get => _isInPlayRange;
        set
        {
            this.RaiseAndSetIfChanged(ref _isInPlayRange, value);
            OnPropertyChanged(nameof(IsInPlayRange));
        }
    }

    private bool _isMarked;
    public bool IsMarked
    {
        get => _isMarked;
        set
        {
            this.RaiseAndSetIfChanged(ref _isMarked, value);
            OnPropertyChanged(nameof(IsMarked));
        }
    }
}

public class Category : ViewModelBase
{
    public Category(string name, int index, int frameCount)
    {
        _frameCount = frameCount;
        this.Name  = name;
        this.Index = index;
        this.Commands = new ObservableCollection<CommandPointer>();
        this.MaxInOneFrame = 0;
        this.CommandsPerFrame = new Dictionary<int, int>();
        this.IsOpen = false;
    }

    private Dictionary<int, int> CommandsPerFrame;

    public void AddCommand(CommandPointer newCmd)
    {
        if (!(this.CommandsPerFrame.ContainsKey(newCmd.Frame)))
            this.CommandsPerFrame[newCmd.Frame] = 0;
        newCmd.PositionWithinFrame = this.CommandsPerFrame[newCmd.Frame];
        this.CommandsPerFrame[newCmd.Frame] += 1;
        if (this.CommandsPerFrame[newCmd.Frame] > this.MaxInOneFrame)
            this.MaxInOneFrame = this.CommandsPerFrame[newCmd.Frame];
        this.Commands.Add(newCmd);
    }

    public void DeleteCommand(CommandPointer cmd)
    {
        // remove the command
        for (int i=this.Commands.Count-1; i>=0; i--)
            if (this.Commands[i] == cmd)
            {
                this.CommandsPerFrame[cmd.Frame] -= 1;
                this.Commands.RemoveAt(i);
                break;
            }

        // TODO: deal with it if nothing was removed somehow???

        // shrink the category height if necessary
        if (this.CommandsPerFrame[cmd.Frame] + 1 == this.MaxInOneFrame)
        {
            this.MaxInOneFrame -= 1;
            foreach (int frame in this.CommandsPerFrame.Keys)
                if (this.CommandsPerFrame[frame] > this.MaxInOneFrame)
                    this.MaxInOneFrame = this.CommandsPerFrame[frame];
        }

        // move up all subsequent commands in the same category + frame
        for (int i=this.Commands.Count-1; i>=0; i--)
            if (this.Commands[i].Frame == cmd.Frame && this.Commands[i].PositionWithinFrame > cmd.PositionWithinFrame)
                this.Commands[i].PositionWithinFrame -= 1;
    }

    private int _frameCount;
    public int FrameCount
    {
        get => _frameCount;
        set
        {
            this.RaiseAndSetIfChanged(ref _frameCount, value);
            OnPropertyChanged(nameof(FrameCount));
        }
    }


    private int _maxInOneFrame;
    public int MaxInOneFrame
    {
        get => _maxInOneFrame;
        set
        {
            this.RaiseAndSetIfChanged(ref _maxInOneFrame, value);
            OnPropertyChanged(nameof(MaxInOneFrame));
        }
    }

    public string Name  { get; set; }
    public int    Index { get; set; }

    public ObservableCollection<CommandPointer> Commands   { get; set; }

    private bool _isOpen;
    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            this.RaiseAndSetIfChanged(ref _isOpen, value);
            OnPropertyChanged(nameof(IsOpen));
        }
    }
}

public class CommandPointer : ViewModelBase
{
    public CommandPointer(DataManager config, string code, bool isAudioCmd, int cmdIndex, int frame, int duration, bool isInPlayRange)
    {
        this.Code       = code;
        this.IsAudioCmd = isAudioCmd;
        this.CmdIndex   = cmdIndex;
        this.Frame      = frame;
        this.Duration   = duration;
        this._isInPlayRange = isInPlayRange;

        this.Command     = ((isAudioCmd) ? config.EventManager.EventSoundCommands : config.EventManager.EventCommands)[cmdIndex];
        this.CommandData = ((isAudioCmd) ? config.EventManager.EventSoundCommandData : config.EventManager.EventCommandData)[cmdIndex];
        this.CommandType = Type.GetType($"EVTUI.ViewModels.TimelineCommands.{code}");
    }

    public string Code       { get; }
    public bool   IsAudioCmd { get; }

    public SerialCommand Command     { get; }
    public dynamic       CommandData { get; }
    public Type          CommandType { get; }

    private int _cmdIndex;
    public int CmdIndex
    {
        get => _cmdIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _cmdIndex, value);
            OnPropertyChanged(nameof(CmdIndex));
        }
    }

    public int    Frame      { get; }
    public int    Duration   { get; }

    private int _positionWithinFrame;
    public int PositionWithinFrame
    {
        get => _positionWithinFrame;
        set
        {
            this.RaiseAndSetIfChanged(ref _positionWithinFrame, value);
            OnPropertyChanged(nameof(PositionWithinFrame));
        }
    }

    private bool _isInPlayRange;
    public bool IsInPlayRange
    {
        get => _isInPlayRange;
        set
        {
            this.RaiseAndSetIfChanged(ref _isInPlayRange, value);
            OnPropertyChanged(nameof(IsInPlayRange));
        }
    }
}

public class TimelinePanelViewModel : ViewModelBase
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public DataManager Config { get; }
    public Clipboard SharedClipboard { get; }

    public Timeline TimelineContent { get; set; }
    public dynamic ActiveCommand { get; set; }

    public List<string> AddableCodes { get => this.Config.EventManager.AddableCodes; }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public TimelinePanelViewModel(DataManager dataManager, Clipboard clipboard)
    {
        this.Config          = dataManager;
        this.TimelineContent = new Timeline(dataManager);
        this.SharedClipboard = clipboard;
    }

    public void SetActiveCommand(CommandPointer cmd)
    {
        if (cmd.CommandType is null)
            this.ActiveCommand = new TimelineCommands.Generic(this.Config, cmd.Command, cmd.CommandData);
        else
            this.ActiveCommand = Activator.CreateInstance(cmd.CommandType, new object[] { this.Config, cmd.Command, cmd.CommandData });
    }

    public void UnsetActiveCommand(bool saveFirst)
    {
        if (!this.Config.ReadOnly && saveFirst)
            this.ActiveCommand.SaveChanges();
        this.ActiveCommand = null;
    }

    public bool DeleteCommand(CommandPointer cmd)
    {
        bool success = this.Config.EventManager.DeleteCommand(cmd.CmdIndex, cmd.IsAudioCmd);
        if (success)
            this.TimelineContent.DeleteCommand(cmd);
        return success;
    }

    public void CopyCommand(CommandPointer cmd, bool deleteOriginal)
    {
        this.SharedClipboard.CopyCommand(this, cmd, deleteOriginal);
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
            if (this.SharedClipboard.DeleteOriginal)
                this.SharedClipboard.DeleteCommand();
        }
    }

    public void NewCommand(string code, int frame)
    {
        bool isAudio = ECS.ValidEcsCommands.Contains(code);
        int newCmdIndex = this.Config.EventManager.NewCommand(code, frame);
        if (newCmdIndex >= 0)
        {
            int len = ((isAudio) ? this.Config.EventManager.EventSoundCommands : this.Config.EventManager.EventCommands)[newCmdIndex].FrameDuration;
            CommandPointer newCmd = new CommandPointer(this.Config, code, isAudio, newCmdIndex, frame, len, (frame >= TimelineContent.StartingFrame && frame < TimelineContent.FrameCount));
            this.TimelineContent.Categories[Timeline.CodeToCategory(code, isAudio)].AddCommand(newCmd);
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
