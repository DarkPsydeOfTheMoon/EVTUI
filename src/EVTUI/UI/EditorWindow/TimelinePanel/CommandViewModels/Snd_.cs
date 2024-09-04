using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class Snd_ : Generic
{
    public Snd_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Sounds: Play Cue";

        _isPlayCmd = ((ActionTypes)this.CommandData.Action == ActionTypes.Play);
        this.Action   = new StringSelectionField("Action", this.Editable, Enum.GetName(typeof(ActionTypes), this.CommandData.Action), new List<string>(Enum.GetNames(typeof(ActionTypes))));
        this.WhenAnyValue(x => x.Action.Choice).Subscribe(x => this.IsPlayCmd = (this.Action.Choice == "Play"));

        this.Channel  = new StringSelectionField("Channel", this.Editable, Enum.GetName(typeof(ChannelTypes), this.CommandData.Channel), new List<string>(Enum.GetNames(typeof(ChannelTypes))));

        this.Source   = new StringSelectionField("Source", this.Editable, Enum.GetName(typeof(SourceTypes), this.CommandData.Source), new List<string>(Enum.GetNames(typeof(SourceTypes))));
        config.AudioManager.SetActiveACBType(this.Source.Choice);
        this.CueID    = new IntSelectionField("Cue ID", this.Editable, this.CommandData.CueId, config.AudioManager.CueIds.ConvertAll(x => (int)x));
        this.WhenAnyValue(x => x.Source.Choice).Subscribe(x =>
        {
            config.AudioManager.SetActiveACBType(x);
            // shenanigans to avoid not-an-object issues when old cueId is not in new set
            if (!config.AudioManager.CueIds.Contains((uint)this.CueID.Choice))
                this.CueID.Choice = 0;
            this.CueID.Choices = new ObservableCollection<int>(config.AudioManager.CueIds.ConvertAll(x => (int)x));
        });

        this.FadeDuration = new NumEntryField("Fade Duration (ms)", this.Editable, this.CommandData.FadeDuration, null, null, 1);
    }

    public IntSelectionField    CueID        { get; set; }
    public StringSelectionField Action       { get; set; }
    public StringSelectionField Channel      { get; set; }
    public StringSelectionField Source       { get; set; }
    public NumEntryField        FadeDuration { get; set; }

    private bool _isPlayCmd;
    public bool IsPlayCmd
    {
        get => _isPlayCmd;
        set => this.RaiseAndSetIfChanged(ref _isPlayCmd, value);
    }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.CommandData.CueId        = this.CueID.Choice;
        this.CommandData.Action       = (int)Enum.Parse(typeof(ActionTypes),  this.Action.Choice );
        this.CommandData.Channel      = (int)Enum.Parse(typeof(ChannelTypes), this.Channel.Choice);
        this.CommandData.Source       = (int)Enum.Parse(typeof(SourceTypes),  this.Source.Choice );
        this.CommandData.FadeDuration = (int)this.FadeDuration.Value;
    }

    public enum ActionTypes : int
    {
        None = 0,
        Play = 1,
        Stop = 2
    }

    public enum ChannelTypes : int
    {
        Default = 0,
        Stereo  = 1,
        Left    = 2,
        Right   = 3
    }

    public enum SourceTypes : int
    {
        None   = 0,
        BGM    = 1,
        System = 2,
        SFX    = 3
    }
}
