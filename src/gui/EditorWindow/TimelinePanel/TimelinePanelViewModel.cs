using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using ReactiveUI;
//using ReactiveUI.Fody.Helpers;

namespace EVTUI.ViewModels;

public class Timeline : ReactiveObject
{

    // from header
    // labels
    public NumEntryField        MajorID { get; set; }
    public NumEntryField        MinorID { get; set; }
    public StringSelectionField Rank    { get; set; }
    public NumEntryField        Level   { get; set; }
    // flags
    public BoolChoiceField StartingFrameEnabled        { get; set; }
    public BoolChoiceField UnkFlag1                    { get; set; }
    public BoolChoiceField CinemascopeEnabled          { get; set; }
    public BoolChoiceField CinemascopeAnimationEnabled { get; set; }
    public BoolChoiceField UnkFlag2                    { get; set; }
    // frames
    public NumEntryField FrameDuration            { get; set; }
    public NumEntryField FrameRate                { get; set; }
    public NumEntryField StartingFrameEntry       { get; set; }
    public NumEntryField CinemascopeStartingFrame { get; set; }
    // indices
    public NumEntryField InitEnvAssetID  { get; set; }
    public NumEntryField DebugEnvAssetID { get; set; }

    public enum Ranks : byte
    {
        None = 0,
        S    = 1,
        A    = 2,
        B    = 3,
        C    = 4,
        D    = 5
    }

    public Timeline(DataManager dataManager)
    {
        EVT evt = (EVT)dataManager.EventManager.SerialEvent;

        // from header

        // labels
        this.MajorID = new NumEntryField("Major ID", !dataManager.ReadOnly, evt.MajorId, 0, 999, 1);
        this.MinorID = new NumEntryField("Minor ID", !dataManager.ReadOnly, evt.MinorId, 0, 999, 1);
        this.Rank = new StringSelectionField("Rank", !dataManager.ReadOnly, Enum.GetName(typeof(Ranks), evt.Rank), new List<string>(Enum.GetNames(typeof(Ranks))));
        this.Level = new NumEntryField("Level", !dataManager.ReadOnly, evt.Level, 0, 3, 1);

        this.WhenAnyValue(x => x.MajorID.Value).Subscribe(x => evt.MajorId = (short)x);
        this.WhenAnyValue(x => x.MinorID.Value).Subscribe(x => evt.MinorId = (short)x);
        this.WhenAnyValue(x => x.Rank.Choice).Subscribe(x => evt.Rank = (byte)Enum.Parse(typeof(Ranks), x));
        this.WhenAnyValue(x => x.Level.Value).Subscribe(x => evt.Level = (byte)x);

        // cinemascope
        this.CinemascopeEnabled = new BoolChoiceField("Enable Cinemascope", !dataManager.ReadOnly, evt.Flags[8]);
        this.CinemascopeAnimationEnabled = new BoolChoiceField("Enable Cinemascope Animation", !dataManager.ReadOnly, evt.Flags[9]);
        this.CinemascopeStartingFrame = new NumEntryField("Cinemascope Starting Frame", !dataManager.ReadOnly, (int)evt.CinemascopeStartingFrame, 0, 9999, 1);

        this.WhenAnyValue(x => x.CinemascopeEnabled.Value).Subscribe(x => evt.Flags[8] = this.CinemascopeEnabled.Value);
        this.WhenAnyValue(x => x.CinemascopeAnimationEnabled.Value).Subscribe(x => evt.Flags[9] = this.CinemascopeAnimationEnabled.Value);
        this.WhenAnyValue(x => x.CinemascopeStartingFrame.Value).Subscribe(x => evt.CinemascopeStartingFrame = (short)x);

        // env
        this.InitEnvAssetID = new NumEntryField("Init ENV ID", !dataManager.ReadOnly, (int)evt.InitEnvAssetID, 0, 9999, 1);
        this.DebugEnvAssetID = new NumEntryField("Debug ENV ID", !dataManager.ReadOnly, (int)evt.InitDebugEnvAssetID, 0, 9999, 1);

        this.WhenAnyValue(x => x.InitEnvAssetID.Value).Subscribe(x => evt.InitEnvAssetID = (int)x);
        this.WhenAnyValue(x => x.DebugEnvAssetID.Value).Subscribe(x => evt.InitDebugEnvAssetID = (int)x);

        // other flags
        this.UnkFlag1 = new BoolChoiceField("Unknown Flag #1", !dataManager.ReadOnly, evt.Flags[6]);
        this.UnkFlag2 = new BoolChoiceField("Unknown Flag #2", !dataManager.ReadOnly, evt.Flags[16]);

        this.WhenAnyValue(x => x.UnkFlag1.Value).Subscribe(x => evt.Flags[6] = this.UnkFlag1.Value);
        this.WhenAnyValue(x => x.UnkFlag2.Value).Subscribe(x => evt.Flags[16] = this.UnkFlag2.Value);

        // frames
        this.FrameRate = new NumEntryField("Frame Rate", !dataManager.ReadOnly, evt.FrameRate, 1, 255, 1);
        this.FrameDuration = new NumEntryField("Frame Count", !dataManager.ReadOnly, evt.FrameCount, 0, 99999, 1);
        this.StartingFrameEnabled = new BoolChoiceField("Set Delayed Starting Frame?", !dataManager.ReadOnly, evt.Flags[0]);
        this.StartingFrameEntry = new NumEntryField("Starting Frame", !dataManager.ReadOnly, evt.StartingFrame, 0, 99999, 1);

        this.WhenAnyValue(x => x.FrameRate.Value).Subscribe(x => evt.FrameRate = (byte)this.FrameRate.Value);
        // (the rest of the WhenAnyValues are below)

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
            if (!dataManager.ReadOnly)
            {
                evt.MarkerFrame = new int[evt.MarkerFrameCount];
                for (int i=0; i<evt.MarkerFrameCount; i++)
                    evt.MarkerFrame[i] = -1;
                for (int i=0; i<this.MarkedFrames.Count; i++)
                    if (i < evt.MarkerFrameCount)
                        evt.MarkerFrame[i] = this.MarkedFrames[i];
            }
        }; //);

        this.Frames = new ObservableCollection<Frame>();
        for (int i=0; i<_frameCount; i++)
            this.Frames.Add(new Frame(i, (i >= _startingFrame && i < _frameCount), this.MarkedFrames.Contains(i)));
        this.ActiveFrame = 0;

        this.Categories = new ObservableCollection<Category>();
        this.Categories.Add(new Category("Camera",   1,  this.FrameCount));
        this.Categories.Add(new Category("Field",    2,  this.FrameCount));
        this.Categories.Add(new Category("Model",    3,  this.FrameCount));
        this.Categories.Add(new Category("Effect",   4,  this.FrameCount));
        this.Categories.Add(new Category("Env",      5,  this.FrameCount));
        this.Categories.Add(new Category("Post",     6,  this.FrameCount));
        this.Categories.Add(new Category("Texture",  7,  this.FrameCount));
        this.Categories.Add(new Category("Movie",    8,  this.FrameCount));
        this.Categories.Add(new Category("Overlay",  9,  this.FrameCount));
        this.Categories.Add(new Category("Dialogue", 10, this.FrameCount));
        this.Categories.Add(new Category("Audio",    11, this.FrameCount));
        this.Categories.Add(new Category("Script",   12, this.FrameCount));
        this.Categories.Add(new Category("Timing",   13, this.FrameCount));
        this.Categories.Add(new Category("Hardware", 14, this.FrameCount));
        this.Categories.Add(new Category("Other",    15, this.FrameCount));

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
        if (isAudio || code == "Snd_" || code == "SBE_" || code == "SBEA" || code == "SFts")
            return 10;
        int catInd = 14;
        if (code == "AlEf" || code == "SFlt")
            catInd = 5;
        else if (code == "FbEn" || code == "Flbk" || code.StartsWith("Im"))
            catInd = 6;
        else if (code == "LBX_" || code == "Date")
            catInd = 8;
        else if (code == "Msg_" || code == "MsgR" || code == "Cht_")
            catInd = 9;
        else if (code == "Scr_")
            catInd = 11;
        else if (code == "Chap" || code == "FrJ_")
            catInd = 12;
        else if (code == "PRum" || code == "TrMc")
            catInd = 13;
        else if (code.StartsWith("Cw"))
            catInd = 1;
        else if (code.StartsWith("En"))
            catInd = 4;
        else if (code.StartsWith("Mv"))
            catInd = 7;
        else if (code.StartsWith("Fd"))
            catInd = 8;
        else if (code.StartsWith("C"))
            catInd = 0;
        else if (code.StartsWith("F"))
            catInd = 1;
        else if (code.StartsWith("M"))
            catInd = 2;
        else if (code.StartsWith("E"))
            catInd = 3;
        else if (code.StartsWith("P"))
            catInd = 5;
        else if (code.StartsWith("T"))
            catInd = 6;
        else if (code.StartsWith("G"))
            catInd = 8;
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

    public void InsertFrames(int afterFrame, int numberFrames)
    {
        // if starting frame follows new frames, shift it
        if (this.StartingFrameEntry.Value > afterFrame)
            this.StartingFrameEntry.Value += afterFrame;
        // increase the total frame duration
        this.FrameDuration.Value += numberFrames;
        // shift all commands coming after the new frames
        foreach (Category cat in this.Categories)
            foreach (CommandPointer cmd in cat.Commands.ToList())
                if (cmd.Frame > afterFrame)
                {
                    cat.MoveCommand(cmd, cmd.Frame + numberFrames);
                    cmd.IsInPlayRange = (cmd.Frame >= this.StartingFrameEntry.Value && cmd.Frame < this.FrameDuration.Value);
                }
        // first delete all frame markers coming after the new frames...
        List<int> frames = MarkedFrames.ToList();
        foreach (int frame in frames)
            if (frame > afterFrame)
            {
                this.MarkedFrames.Remove(frame);
                if (frame > 0 && frame < this.Frames.Count)
                    this.Frames[frame].IsMarked = false;
            }
        // ...and then put them back in, shifted
        foreach (int frame in frames)
            if (frame > afterFrame)
            {
                this.MarkedFrames.Add(frame + numberFrames);
                if ((frame + numberFrames) > 0 && (frame + numberFrames) < this.Frames.Count)
                    this.Frames[frame + numberFrames].IsMarked = true;
            }
    }

    public void ClearFrames(int startingFrame, int endingFrame, bool deleteFrames)
    {
        // delete all commands within the range
        foreach (Category cat in this.Categories)
            foreach (CommandPointer cmd in cat.Commands.ToList())
                if (cmd.Frame >= startingFrame && cmd.Frame <= endingFrame)
                    cat.DeleteCommand(cmd);
        // remove all marked frames within the range
        List<int> frames = MarkedFrames.ToList();
        foreach (int frame in frames)
            if (frame >= startingFrame && frame <= endingFrame)
            {
                this.MarkedFrames.Remove(frame);
                // shouldn't really need this check here, but why not
                if (frame > 0 && frame < this.Frames.Count)
                    this.Frames[frame].IsMarked = false;
            }

        if (deleteFrames)
        {
            // if starting frame follows new frames, shift it (back)
            if (this.StartingFrameEntry.Value > endingFrame)
                this.StartingFrameEntry.Value -= 1 + endingFrame - startingFrame;
            // if starting frame is within deleted frames...
            else if (this.StartingFrameEntry.Value >= startingFrame && this.StartingFrameEntry.Value <= endingFrame)
                    this.StartingFrameEntry.Value = startingFrame;
            // decrease the total frame duration
            this.FrameDuration.Value -= 1 + endingFrame - startingFrame;
            // shift (back) all commands coming after the new frames
            foreach (Category cat in this.Categories)
                foreach (CommandPointer cmd in cat.Commands.ToList())
                    if (cmd.Frame > endingFrame)
                    {
                        cat.MoveCommand(cmd, cmd.Frame - (1 + endingFrame - startingFrame));
                        cmd.IsInPlayRange = (cmd.Frame >= this.StartingFrameEntry.Value && cmd.Frame < this.FrameDuration.Value);
                    }
            // first delete all frame markers coming after the new frames...
            foreach (int frame in frames)
                if (frame > endingFrame)
                {
                    this.MarkedFrames.Remove(frame);
                    if (frame > 0 && frame < this.Frames.Count)
                        this.Frames[frame].IsMarked = false;
                }
            // ...and then put them back in, shifted (back)
            foreach (int frame in frames)
                if (frame > endingFrame)
                {
                    this.MarkedFrames.Add(frame - (1 + endingFrame - startingFrame));
                    if ((frame - (1 + endingFrame - startingFrame)) > 0 && (frame - (1 + endingFrame - startingFrame)) < this.Frames.Count)
                        this.Frames[frame - (1 + endingFrame - startingFrame)].IsMarked = true;
                }
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

    private int                      _maxMarks;
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

    public void MoveCommand(CommandPointer cmd, int newFrame)
    {
        // uh....... idk why this would happen but let's. avoid that weirdness.
        if (!this.Commands.Contains(cmd) || cmd.Frame == newFrame)
            return;

        // remove it from the old frame...
        this.CommandsPerFrame[cmd.Frame] -= 1;
        if (this.CommandsPerFrame[cmd.Frame] <= 0)
            this.CommandsPerFrame.Remove(cmd.Frame);
        // shrink the category height if necessary
        this._maxInOneFrame = 0;
        foreach (int frame in this.CommandsPerFrame.Keys)
            if (this.CommandsPerFrame[frame] > this._maxInOneFrame)
                this.MaxInOneFrame = this.CommandsPerFrame[frame];
        // move up all subsequent commands in the same category + frame
        for (int i=this.Commands.Count-1; i>=0; i--)
            if (this.Commands[i].Frame == cmd.Frame && this.Commands[i].PositionWithinFrame > cmd.PositionWithinFrame)
                this.Commands[i].PositionWithinFrame -= 1;

        // ...and then add it to the new frame
        if (!(this.CommandsPerFrame.ContainsKey(newFrame)))
            this.CommandsPerFrame[newFrame] = 0;
        cmd.PositionWithinFrame = this.CommandsPerFrame[newFrame];
        this.CommandsPerFrame[newFrame] += 1;
        if (this.CommandsPerFrame[newFrame] > this.MaxInOneFrame)
            this.MaxInOneFrame = this.CommandsPerFrame[newFrame];

        // ......and actually change it
        cmd.Frame = newFrame;
        this.SortCommands();
    }

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
                if (this.CommandsPerFrame[cmd.Frame] <= 0)
                    this.CommandsPerFrame.Remove(cmd.Frame);
                this.Commands.RemoveAt(i);
                break;
            }

        // TODO: deal with it if nothing was removed somehow???

        // shrink the category height if necessary
        this._maxInOneFrame = 0;
        foreach (int frame in this.CommandsPerFrame.Keys)
            if (this.CommandsPerFrame[frame] > this._maxInOneFrame)
                this.MaxInOneFrame = this.CommandsPerFrame[frame];

        // move up all subsequent commands in the same category + frame
        for (int i=this.Commands.Count-1; i>=0; i--)
            if (this.Commands[i].Frame == cmd.Frame && this.Commands[i].PositionWithinFrame > cmd.PositionWithinFrame)
                this.Commands[i].PositionWithinFrame -= 1;
    }

    // make sure they get drawn in the right order, basically...
    public void SortCommands()
    {
        ObservableCollection<CommandPointer> temp = new ObservableCollection<CommandPointer>(this.Commands.OrderBy(cmd => cmd.Frame));
        this.Commands.Clear();
        foreach (CommandPointer cmd in temp)
            this.Commands.Add(cmd);
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

        this.WhenAnyValue(_ => _.Frame).Subscribe(_ => this.Command.FrameStart = this.Frame);
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

    private int _frame;
    public int Frame
    {
        get => _frame;
        set
        {
            this.RaiseAndSetIfChanged(ref _frame, value);
            OnPropertyChanged(nameof(Frame));
        }
    }

    private int _duration;
    public int Duration
    {
        get => _duration;
        set
        {
            this.RaiseAndSetIfChanged(ref _duration, value);
            OnPropertyChanged(nameof(Duration));
        }
    }

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
    //public dynamic ActiveCommand { get; set; }
    public TimelineCommands.Generic ActiveCommand { get; set; }
    protected CommandPointer _activeCommandPointer;
    protected Category _activeCategory;

    public ObservableCollection<string> AddableCodes { get; set; } = new ObservableCollection<string>();

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public TimelinePanelViewModel(DataManager dataManager, Clipboard clipboard)
    {
        this.Config          = dataManager;
        this.TimelineContent = new Timeline(dataManager);
        this.SharedClipboard = clipboard;
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

        this.TimelineContent.ClearFrames(startingFrame, endingFrame, deleteFrames);

        if (deleteFrames && !(this.ActiveCommand is null) && this.ActiveCommand.Basics.StartingFrame.Value > endingFrame)
            this.ActiveCommand.Basics.StartingFrame.Value -= 1 + endingFrame - startingFrame;
    }

    public void SetAddableCodes(Category category)
    {
        this.AddableCodes.Clear();
        foreach (string code in this.Config.EventManager.AddableCodes)
            if (Timeline.CodeToCategory(code, false) + 1 == category.Index)
                this.AddableCodes.Add(code);
    }

    public void SetActiveCommand(CommandPointer cmd)
    {
        this._activeCommandPointer = cmd;
        this._activeCategory = this.TimelineContent.Categories[Timeline.CodeToCategory(cmd.Code, false)];
        if (cmd.CommandType is null)
            this.ActiveCommand = new TimelineCommands.Generic(this.Config, cmd);
        else
            this.ActiveCommand = (TimelineCommands.Generic)Activator.CreateInstance(cmd.CommandType, new object[] { this.Config, cmd });

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
