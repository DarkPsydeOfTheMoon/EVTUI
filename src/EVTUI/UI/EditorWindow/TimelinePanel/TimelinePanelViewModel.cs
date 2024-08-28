using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ReactiveUI;
// TODO: add this back in when it no longer breaks Audio Panel...
//using ReactiveUI.Fody.Helpers;

namespace EVTUI.ViewModels;

public class FieldBase : ReactiveObject
{
    public FieldBase(string name, bool editable)
    {
        this.Name     = name;
        this.Editable = editable;
    }

    public string Name     { get; }
    public bool   Editable { get; }
}

// ints/floats with open ranges
// (to be numerical entry)
public class NumEntryField : FieldBase
{
    public NumEntryField(string name, bool editable, dynamic val, dynamic? lowerLimit, dynamic? upperLimit, dynamic increment) : base(name, editable)
    {
        _value          = (decimal)val;
        this.LowerLimit = (decimal?)lowerLimit;
        this.UpperLimit = (decimal?)upperLimit;
        this.Increment  = (decimal)increment;
    }

    private decimal _value;
    public decimal Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }

    public decimal? LowerLimit { get; set; }
    public decimal? UpperLimit { get; set; }
    public decimal  Increment  { get; set; }
}

// TODO: enable inline non-string elements...?
public class StringEntryField : FieldBase
{
    public StringEntryField(string name, bool editable, string text) : base(name, editable)
    {
        _text = text;
    }

    private string _text;
    public string Text
    {
        get => _text;
        set => this.RaiseAndSetIfChanged(ref _text, value);
    }
}

// ints/floats with definite ranges
// (to be sliders)
public class NumRangeField : FieldBase
{
    public NumRangeField(string name, bool editable, dynamic val, dynamic lowerLimit, dynamic upperLimit, dynamic? increment) : base(name, editable)
    {
        _value          = (decimal)val;
        this.LowerLimit = (decimal)lowerLimit;
        this.UpperLimit = (decimal)upperLimit;
        this.Increment  = (decimal?)increment;
    }

    private decimal _value;
    public decimal Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }

    public decimal  LowerLimit { get; set; }
    public decimal  UpperLimit { get; set; }
    public decimal? Increment  { get; set; }
}

// indices, maybe unknown enums
// (to be dropdowns)
public class IntSelectionField : FieldBase
{
    public IntSelectionField(string name, bool editable, int choiceIndex, List<int> choices) : base(name, editable)
    {
        _choice = choiceIndex;
        _choices = new ObservableCollection<int>(choices);
    }

    private int _choice;
    public int Choice
    {
        get => _choice;
        set => this.RaiseAndSetIfChanged(ref _choice, value);
    }
    //private readonly ObservableAsPropertyHelper<int> _choice;
    //public int Choice => _choice.Value;

    private ObservableCollection<int> _choices;
    public ObservableCollection<int> Choices
    {
        get => _choices;
        set => this.RaiseAndSetIfChanged(ref _choices, value);
    }
}

// known enums
// (to be dropdowns)
public class StringSelectionField : FieldBase
{
    public StringSelectionField(string name, bool editable, string choiceIndex, List<string> choices) : base(name, editable)
    {
        _choice = choiceIndex;
        _choices = new ObservableCollection<string>(choices);
    }

    private string _choice;
    public string Choice
    {
        get => _choice;
        set => this.RaiseAndSetIfChanged(ref _choice, value);
    }

    private ObservableCollection<string> _choices;
    public ObservableCollection<string> Choices
    {
        get => _choices;
        set => this.RaiseAndSetIfChanged(ref _choices, value);
    }
}

// (to be checkboxes)
public class BoolChoiceField : FieldBase
{
    public BoolChoiceField(string name, bool editable, bool val) : base(name, editable)
    {
        _value = val;
    }

    private bool _value;
    public bool Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }
}

public class Timeline
{
    public Timeline(DataManager dataManager)
    {
        this.Frames = new List<Frame>();
		for (int i=0; i<dataManager.EventManager.EventDuration; i++)
            this.Frames.Add(new Frame(i));
        for (int j=0; j<dataManager.EventManager.EventSoundCommands.Length; j++)
        {
            string code = dataManager.EventManager.EventSoundCommands[j].CommandCode;
            int i = dataManager.EventManager.EventSoundCommands[j].FrameStart;
            if (i < this.Frames.Count)
            {
                int k = this.Frames[i].Commands.Count;
                this.Frames[i].Commands.Add(new CommandPointer(code, true, j, k));
            }
        }
        for (int j=0; j<dataManager.EventManager.EventCommands.Length; j++)
        {
            string code = dataManager.EventManager.EventCommands[j].CommandCode;
            int i = dataManager.EventManager.EventCommands[j].FrameStart;
            if (i < this.Frames.Count)
            {
                int k = this.Frames[i].Commands.Count;
                this.Frames[i].Commands.Add(new CommandPointer(code, false, j, k));
            }
        }
    }

    public List<Frame> Frames { get; set; }
}

public class Frame
{
    public Frame(int index)
    {
        this.Index    = index;
        this.Commands = new List<CommandPointer>();
    }

    public int                  Index    { get; set; }
    public List<CommandPointer> Commands { get; set; }
}

public class CommandPointer
{
    public CommandPointer(string code, bool isAudioCmd, int cmdIndex, int indexWithinFrame)
    {
        this.Code             = code;
        this.IsAudioCmd       = isAudioCmd;
        this.CmdIndex         = cmdIndex;
        this.IndexWithinFrame = indexWithinFrame;
    }

    public string Code             { get; }
    public bool   IsAudioCmd       { get; }
    public int    CmdIndex         { get; }
    public int    IndexWithinFrame { get; }
}

public class TimelinePanelViewModel : ViewModelBase
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public DataManager Config { get; }

    public Timeline TimelineContent { get; set; }
    public dynamic ActiveCommand { get; set; }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public TimelinePanelViewModel(DataManager dataManager)
    {
        this.Config = dataManager;
        this.TimelineContent = new Timeline(dataManager);
    }

    public void SetActiveCommand(CommandPointer cmd)
    {
        SerialCommand command     = ((cmd.IsAudioCmd) ? this.Config.EventManager.EventSoundCommands : this.Config.EventManager.EventCommands)[cmd.CmdIndex];
        dynamic       commandData = ((cmd.IsAudioCmd) ? this.Config.EventManager.EventSoundCommandData : this.Config.EventManager.EventCommandData)[cmd.CmdIndex];
        Type          commandType = Type.GetType($"EVTUI.ViewModels.TimelineCommands.{cmd.Code}");
		if (commandType is null)
            this.ActiveCommand = new TimelineCommands.Generic(this.Config, command, commandData);
        else
            this.ActiveCommand = Activator.CreateInstance(commandType, new object[] { this.Config, command, commandData });
    }

    public void UnsetActiveCommand(bool saveFirst)
    {
        if (!this.Config.ReadOnly && saveFirst)
            this.ActiveCommand.SaveChanges();
        this.ActiveCommand = null;
    }

    public void PlayCueFromSource(string source, int cueId, int trackIndex)
    {
        if (this.Config.AudioManager.AcbByType[source].Count > 0)
        {
            this.Config.AudioManager.ActiveACB = this.Config.AudioManager.AcbByType[source][0];
            this.Config.AudioManager.PlayCueTrack((uint)cueId, trackIndex);
        }
    }

}
