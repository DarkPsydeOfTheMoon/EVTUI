using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Windows.Input;
using ReactiveUI;

namespace EVTUI.ViewModels;

public struct ViewableEvent
{
    public ViewableEvent(int duration, Dictionary<int, List<ViewableCommand>> commands)
    {
        this.Duration = duration;
        this.Commands = commands;
    }

    public int                                    Duration { get; set; }
    public Dictionary<int, List<ViewableCommand>> Commands { get; set; }
}

public struct ViewableCommand
{
    public ViewableCommand(string code, string longname, Dictionary<string, IntEntryField> intEntryFields, Dictionary<string, FloatEntryField> floatEntryFields, Dictionary<string, IntSelectionField> intSelectionFields, Dictionary<string, StringSelectionField> stringSelectionFields, Dictionary<string, BoolChoiceField> boolChoiceFields)
    {
        this.Code                  = code;
        this.LongName              = longname;
        this.IntEntryFields        = intEntryFields;
        this.FloatEntryFields      = floatEntryFields;
        this.IntSelectionFields    = intSelectionFields;
        this.StringSelectionFields = stringSelectionFields;
        this.BoolChoiceFields      = boolChoiceFields;
    }

    public string                                   Code                  { get; }
    public string                                   LongName              { get; }
    public Dictionary<string, IntEntryField>        IntEntryFields        { get; set; }
    public Dictionary<string, FloatEntryField>      FloatEntryFields      { get; set; }
    public Dictionary<string, IntSelectionField>    IntSelectionFields    { get; set; }
    public Dictionary<string, StringSelectionField> StringSelectionFields { get; set; }
    public Dictionary<string, BoolChoiceField>      BoolChoiceFields      { get; set; }
}

// ints with open ranges
// (to be numerical entry)
public struct IntEntryField
{
    public IntEntryField(int val, int? lowerLimit, int? upperLimit)
    {
        this.Value      = val;
        this.LowerLimit = lowerLimit;
        this.UpperLimit = upperLimit;
    }

    public int  Value      { get; set; }
    public int? LowerLimit { get; set; }
    public int? UpperLimit { get; set; }
}

// basically any float, probably
// (to be text entry)
public struct FloatEntryField
{
    public FloatEntryField(float val, float? lowerLimit, float? upperLimit)
    {
        this.Value      = val;
        this.LowerLimit = lowerLimit;
        this.UpperLimit = upperLimit;
    }

    public float  Value      { get; set; }
    // can free entry actually use these? well, we'll try...
    public float? LowerLimit { get; set; }
    public float? UpperLimit { get; set; }
}

// indices, maybe unknown enums
// (to be dropdowns)
public struct IntSelectionField
{
    public IntSelectionField(int choiceIndex, List<int> choices)
    {
        this.Choice  = choiceIndex;
        this.Choices = choices;
    }

    public int       Choice  { get; set; }
    public List<int> Choices { get; }
}

// known enums
// (to be dropdowns)
public struct StringSelectionField
{
    public StringSelectionField(string choiceIndex, List<string> choices)
    {
        this.Choice  = choiceIndex;
        this.Choices = choices;
    }

    public string       Choice  { get; set; }
    public List<string> Choices { get; }
}

// (to be checkboxes)
public struct BoolChoiceField
{
    public BoolChoiceField(bool val)
    {
        this.Value = val;
    }

    public bool Value { get; set; }
}

public class TimelinePanelViewModel : ViewModelBase
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public DataManager Config;
    public ViewableEvent ViewableEvent;

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public TimelinePanelViewModel(DataManager dataManager)
    {
        this.Config = dataManager;
        this.UpdateViewableEvent();
    }

    public void UpdateViewableEvent()
    {
        List<int> frameNumbers = Enumerable.Range(0, this.Config.EventManager.EventDuration).ToList();
        List<int> modelIndices = this.Config.EventManager.ObjectIndices("model");
        // once we're reading .BMDs properly we can actually use that for this, maybe
        List<string> msgCache = new List<string>();
        List<string> selCache = new List<string>();
        // same for below re: above for .GMD and .GAP files
        Dictionary<int, List<int>> armatureAnimations = new Dictionary<int, List<int>>();
        //Dictionary<int, List<int>> blendAnimations = new Dictionary<int, List<int>>();

        Dictionary<int, List<ViewableCommand>> commands = new Dictionary<int, List<ViewableCommand>>();
        for (int i=0; i<this.Config.EventManager.EventCommands.Length; i++)
        {
            SerialCommand command = this.Config.EventManager.EventCommands[i];
            object commandData = this.Config.EventManager.EventCommandData[i];

            if (!commands.ContainsKey(command.FrameStart))
                commands[command.FrameStart] = new List<ViewableCommand>();

            string longName = "";
            Dictionary<string, IntEntryField> intEntryFields = new Dictionary<string, IntEntryField>();
            Dictionary<string, FloatEntryField> floatEntryFields = new Dictionary<string, FloatEntryField>();
            Dictionary<string, IntSelectionField> intSelectionFields = new Dictionary<string, IntSelectionField>();
            Dictionary<string, StringSelectionField> stringSelectionFields = new Dictionary<string, StringSelectionField>();
            Dictionary<string, BoolChoiceField> boolChoiceFields = new Dictionary<string, BoolChoiceField>();

            if (command.CommandCode == "CSD_")
            {
                longName = "Camera: Set Data";
                //intSelectionFields[""] = new FloatEntryField();
                commands[command.FrameStart].Add(new ViewableCommand(command.CommandCode, longName, intEntryFields, floatEntryFields, intSelectionFields, stringSelectionFields, boolChoiceFields));
            }
            else if (command.CommandCode == "MAA_")
            {
                longName = "Model: Blend Animation";
                intSelectionFields["Asset ID"] = new IntSelectionField(command.ObjectId, modelIndices);
                commands[command.FrameStart].Add(new ViewableCommand(command.CommandCode, longName, intEntryFields, floatEntryFields, intSelectionFields, stringSelectionFields, boolChoiceFields));
            }
            else if (command.CommandCode == "MAB_")
            {
                longName = "Model: Armature Animation";
                CommandTypes.MAB_ MAB_Data = (CommandTypes.MAB_)commandData;
                intSelectionFields["Asset ID"] = new IntSelectionField(command.ObjectId, modelIndices);
                if (!armatureAnimations.ContainsKey(command.ObjectId))
                    armatureAnimations[command.ObjectId] = new List<int>();
                // TODO: the below fields should probably be included regardless, just hidden in certain cases...? or nullable or something
                if (((MAB_Data.AnimationMode >> 1) & 1) == 1)
                {
                    if (!armatureAnimations[command.ObjectId].Contains(MAB_Data.PrimaryAnimationIndex))
                        armatureAnimations[command.ObjectId].Add(MAB_Data.PrimaryAnimationIndex);
                    intSelectionFields["Primary Animation ID"] = new IntSelectionField(MAB_Data.PrimaryAnimationIndex, armatureAnimations[command.ObjectId]);
                }
                if ((MAB_Data.AnimationMode & 1) != 1 && MAB_Data.SecondaryAnimationIndex != 0 && !armatureAnimations[command.ObjectId].Contains(MAB_Data.SecondaryAnimationIndex))
                {
                    armatureAnimations[command.ObjectId].Add(MAB_Data.SecondaryAnimationIndex);
                    intSelectionFields["Secondary Animation ID"] = new IntSelectionField(MAB_Data.SecondaryAnimationIndex, armatureAnimations[command.ObjectId]);
                }
                boolChoiceFields["IgnoreSecondAnimation"] = new BoolChoiceField((MAB_Data.AnimationMode & 1) == 1);
                boolChoiceFields["HasPrimaryAnimation"] = new BoolChoiceField(((MAB_Data.AnimationMode >> 1) & 1) == 1);
                boolChoiceFields["PrimaryAnimationFromSecondaryFile"] = new BoolChoiceField(((MAB_Data.AnimationMode >> 4) & 1) == 1);
                boolChoiceFields["SecondaryAnimationFromSecondaryFile"] = new BoolChoiceField(((MAB_Data.AnimationMode >> 5) & 1) == 1);
                commands[command.FrameStart].Add(new ViewableCommand(command.CommandCode, longName, intEntryFields, floatEntryFields, intSelectionFields, stringSelectionFields, boolChoiceFields));
            }
            else if (command.CommandCode == "MRgs")
            {
                longName = "Model: Register";
                CommandTypes.MRgs MRgsData = (CommandTypes.MRgs)commandData;
                intSelectionFields["Asset ID"] = new IntSelectionField(command.ObjectId, modelIndices);
                boolChoiceFields["SpawnElseDespawn"] = new BoolChoiceField(MRgsData.ActionType == 1);
                commands[command.FrameStart].Add(new ViewableCommand(command.CommandCode, longName, intEntryFields, floatEntryFields, intSelectionFields, stringSelectionFields, boolChoiceFields));
            }
            else if (command.CommandCode == "MSD_")
            {
                longName = "Model: Set Direct";
                intSelectionFields["Asset ID"] = new IntSelectionField(command.ObjectId, modelIndices);
                commands[command.FrameStart].Add(new ViewableCommand(command.CommandCode, longName, intEntryFields, floatEntryFields, intSelectionFields, stringSelectionFields, boolChoiceFields));
            }
            else if (command.CommandCode == "Msg_")
            {
                longName = "Dialogue Turn";
                CommandTypes.Msg_ Msg_Data = (CommandTypes.Msg_)commandData;
                string msgId = $"MSG_{Msg_Data.MessageMajorId:000}_{Msg_Data.MessageMinorId}_{Msg_Data.MessageSubId}";
                msgCache.Add(msgId);
                stringSelectionFields["Message ID"] = new StringSelectionField(msgId, msgCache);
                if (((Msg_Data.MessageMode >> 1) & 1) == 1)
                {
                    string selId = $"SEL_{Msg_Data.SelectMajorId:000}_{Msg_Data.SelectMinorId}_{Msg_Data.SelectSubId}";
                    selCache.Add(selId);
                    stringSelectionFields["Selection ID"] = new StringSelectionField(selId, selCache);
                }
                boolChoiceFields["HasMessage"] = new BoolChoiceField((Msg_Data.MessageMode & 1) == 1);
                boolChoiceFields["HasSelect"] = new BoolChoiceField(((Msg_Data.MessageMode >> 1) & 1) == 1);
                boolChoiceFields["IsSubtitle"] = new BoolChoiceField(((Msg_Data.MessageMode >> 2) & 1) == 1);
                commands[command.FrameStart].Add(new ViewableCommand(command.CommandCode, longName, intEntryFields, floatEntryFields, intSelectionFields, stringSelectionFields, boolChoiceFields));
            }
            else
            {
                longName = "(Not Yet Implemented)";
                commands[command.FrameStart].Add(new ViewableCommand(command.CommandCode, longName, intEntryFields, floatEntryFields, intSelectionFields, stringSelectionFields, boolChoiceFields));
            }
        }

        this.ViewableEvent = new ViewableEvent(this.Config.EventManager.EventDuration, commands);
    }

    public async void CallUpdateTimelineInView()
    {
        // worth noting that now ViewableEvent is NOT getting updated when we switch tabs
        // which is mostly fine, but we might want it to refresh based on changes in other tabs
        // ...at some point. but for now it's too annoying for it to happen every time.
        bool success = await this.UpdateTimelineInView.Handle(this.ViewableEvent);
    }

    // View Interactions
    public Interaction<ViewableEvent, bool> UpdateTimelineInView { get; } = new Interaction<ViewableEvent, bool>();

}
