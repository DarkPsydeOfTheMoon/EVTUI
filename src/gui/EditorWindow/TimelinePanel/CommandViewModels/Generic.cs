using System;
using System.Collections.Generic;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class Generic : ReactiveObject
{
    public Generic(DataManager config, SerialCommand command, object commandData)
    {
        this.LongName    = command.CommandCode;
        this.Command     = command;
        this.CommandData = commandData;
        this.Editable    = !config.ReadOnly;
        this.Basics      = new Basics(config, command);

        this.Size = command.DataSize;
    }

    public string           LongName    { get; set; }
    protected SerialCommand Command;
    protected dynamic       CommandData;
    public bool             Editable { get; }
    public Basics           Basics { get; set; }

    public int Size { get; }

    public void SaveChanges()
    {
        this.Basics.SaveChanges();
    }
}

public class Basics : ReactiveObject
{
    public Basics(DataManager config, SerialCommand command)
    {
        this.Command = command;
        this.Editable = !config.ReadOnly;

        this.WaitOnCommand = new BoolChoiceField("Wait while frame is stopped?", this.Editable, this.Command.Flags[1]);
        // i don't feel like making this reactive lol that's annoying, woe readonly be upon you
        this.StartingFrame = new NumEntryField("Starting Frame", false, this.Command.FrameStart, 0, 999999, 1);
        this.FrameCount = new NumEntryField("Frame Duration", this.Editable, this.Command.FrameDuration, 0, 999999, 1);

        this.ForceSkipCommand = new BoolChoiceField("Force-skip command?", this.Editable, this.Command.Flags[0]);
        this.ConditionalType = new StringSelectionField("Conditional Type", this.Editable, this.ConditionalTypes.Backward[this.Command.ConditionalType], this.ConditionalTypes.Keys);
        this.ConditionalIndex = new NumEntryField("Conditional Index", this.Editable, this.Command.ConditionalIndex, 0, null, 1);
        this.ComparisonType = new StringSelectionField("Comparison Type", this.Editable, this.ComparisonTypes.Backward[this.Command.ConditionalComparisonType], this.ComparisonTypes.Keys);
        this.ConditionalValue = new NumEntryField("Conditional Value", this.Editable, this.Command.ConditionalValue, null, null, 1);

        this.WhenAnyValue(x => x.ConditionalType.Choice).Subscribe(x =>
        {
            this.ConditionalIndex.UpperLimit = null;
            if (x == "Reference Global Count")
                this.ConditionalIndex.UpperLimit = 382;
            this.ConditionalValue.LowerLimit = null;
            this.ConditionalValue.UpperLimit = null;
            if (x == "Reference Global Bitflag")
            {
                this.ConditionalValue.LowerLimit = 0;
                this.ConditionalValue.UpperLimit = 1;
            }
        });
    }

    public BoolChoiceField      WaitOnCommand { get; set; }
    public NumEntryField        StartingFrame { get; set; }
    public NumEntryField        FrameCount    { get; set; }

    public BoolChoiceField      ForceSkipCommand { get; set; }
    public StringSelectionField ConditionalType  { get; set; }
    public NumEntryField        ConditionalIndex { get; set; }
    public StringSelectionField ComparisonType   { get; set; }
    public NumEntryField        ConditionalValue { get; set; }

    protected SerialCommand Command;
    public    bool          Editable { get; set; }

    public void SaveChanges()
    {

        this.Command.Flags[0] = this.ForceSkipCommand.Value;
        this.Command.Flags[1] = this.WaitOnCommand.Value;

        this.Command.FrameStart    = (int)this.StartingFrame.Value;
        this.Command.FrameDuration = (int)this.FrameCount.Value;

        this.Command.ConditionalType           = this.ConditionalTypes.Forward[this.ConditionalType.Choice];
        this.Command.ConditionalIndex          = (uint)this.ConditionalIndex.Value;
        this.Command.ConditionalValue          = (int)this.ConditionalValue.Value;
        this.Command.ConditionalComparisonType = this.ComparisonTypes.Forward[this.ComparisonType.Choice];
    }

    public BiDict<string, uint> ConditionalTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Always Run",                  0},
            {"Never Run",                   1},
            {"Reference Local Data",        2},
            {"Reference Global Bitflag",    3},
            {"Reference Global Counter",    4},
            {"Reference Animation Counter", 5},
        }
    );

    public BiDict<string, uint> ComparisonTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"==", 0},
            {"!=", 1},
            {"<",  2},
            {">",  3},
            {"<=", 4},
            {">=", 5},
        }
    );
}
