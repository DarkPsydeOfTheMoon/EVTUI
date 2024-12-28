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
    }

    public string           LongName    { get; set; }
    protected SerialCommand Command;
    protected dynamic       CommandData;
    public bool             Editable { get; }
    public Basics           Basics { get; set; }

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

        this.ConditionalType = new StringSelectionField("Conditional Type", this.Editable, Enum.GetName(typeof(ConditionalTypes), this.Command.ConditionalType), new List<string>(Enum.GetNames(typeof(ConditionalTypes))));
        this.WhenAnyValue(x => x.ConditionalType.Choice).Subscribe(x =>
        {
            this.HasCondition = (x != "None");
            this.UsesBitflag = (x == "Bitflag");
            this.MaxIndex = null;
            if (x == "Count")
                this.MaxIndex = 382;
            this.MaxValue = null;
            if (x == "Bitflag")
                this.MaxValue = 1;
        });

        this.MaxIndex = null;
        if (this.ConditionalType.Choice == "Count")
            this.MaxIndex = 382;
        this.ConditionalIndex = new NumEntryField("Conditional Index", this.Editable, (int)this.Command.ConditionalIndex, 0, this.MaxIndex, 1);

        this.ComparisonType = new StringSelectionField("Comparison Type", this.Editable, Enum.GetName(typeof(ComparisonTypes), this.Command.ConditionalComparisonType), new List<string>(Enum.GetNames(typeof(ComparisonTypes))));

        this.MaxValue = null;
        if (this.ConditionalType.Choice == "Bitflag")
            this.MaxValue = 1;
        this.ConditionalValue = new NumEntryField("Conditional Value", this.Editable, (int)this.Command.ConditionalValue, 0, this.MaxValue, 1);
    }

    public StringSelectionField ConditionalType  { get; set; }
    public NumEntryField        ConditionalIndex { get; set; }
    public StringSelectionField ComparisonType   { get; set; }
    public NumEntryField        ConditionalValue { get; set; }

    protected SerialCommand Command;
    public    bool          Editable { get; set; }

    private bool _hasCondition;
    public bool HasCondition
    {
        get => _hasCondition;
        set => this.RaiseAndSetIfChanged(ref _hasCondition, value);
    }
    //readonly ObservableAsPropertyHelper<bool> hasCondition;
    //public bool HasCondition => hasCondition.Value;

    private bool _usesBitflag;
    public bool UsesBitflag
    {
        get => _usesBitflag;
        set => this.RaiseAndSetIfChanged(ref _usesBitflag, value);
    }

    private decimal? _maxIndex;
    public decimal? MaxIndex
    {
        get => _maxIndex;
        set => this.RaiseAndSetIfChanged(ref _maxIndex, value);
    }

    private decimal? _maxValue;
    public decimal? MaxValue
    {
        get => _maxValue;
        set => this.RaiseAndSetIfChanged(ref _maxValue, value);
    }

    public void SaveChanges()
    {
        this.Command.ConditionalType  = (int)Enum.Parse(typeof(ConditionalTypes), this.ConditionalType.Choice);
    }

    public enum ConditionalTypes : int
    {
        None         = 0,
        False        = 1,
        EvtLocalData = 2,
        Bitflag      = 3,
        Count        = 4,
        EvtAnimData  = 5
    }

    public enum ComparisonTypes : int
    {
        EQ = 0,
        NE = 1,
        LT = 2,
        GT = 3,
        LE = 4,
        GE = 5
    }
}
