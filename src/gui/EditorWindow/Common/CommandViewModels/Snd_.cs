using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ReactiveUI;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class Snd_ : Generic
{
    public Snd_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Sounds: Play Cue";

        this.ActionType = new StringSelectionField("Action", this.Editable, this.ActionTypes.Backward[this.CommandData.Action], this.ActionTypes.Keys);
        this.WhenAnyValue(_ => _.ActionType.Choice).Subscribe(_ => this.CommandData.Action = this.ActionTypes.Forward[this.ActionType.Choice]);

        this.CueID = new IntSelectionField("Cue ID", this.Editable, this.CommandData.CueId, config.AudioManager.CueIds.ConvertAll(x => (int)x));
        this.WhenAnyValue(_ => _.CueID.Choice).Subscribe(_ => this.CommandData.CueId = this.CueID.Choice);

        this.SourceType = new StringSelectionField("Source", this.Editable,  this.SourceTypes.Backward[this.CommandData.Source], this.SourceTypes.Keys);
        config.AudioManager.SetActiveACBType(this.SourceType.Choice);
        this.WhenAnyValue(x => x.SourceType.Choice).Subscribe(x =>
        {
            this.CommandData.Source = this.SourceTypes.Forward[this.SourceType.Choice];
            config.AudioManager.SetActiveACBType(x);
            // shenanigans to avoid not-an-object issues when old cueId is not in new set
            if (!config.AudioManager.CueIds.Contains((uint)this.CueID.Choice))
                this.CueID.Choice = 0;
            this.CueID.Choices = new ObservableCollection<int>(config.AudioManager.CueIds.ConvertAll(x => (int)x));
        });

        this.FadeDuration = new NumEntryField("Fade Duration (ms)", this.Editable, this.CommandData.FadeDuration, 0, 120, 1);
        this.WhenAnyValue(_ => _.FadeDuration.Value).Subscribe(_ => this.CommandData.FadeDuration = (int)this.FadeDuration.Value);

        this.Unk = new NumEntryField("Unknown", this.Editable, this.CommandData.Channel, 0, 3, 1);
        this.WhenAnyValue(_ => _.Unk.Value).Subscribe(_ => this.CommandData.Channel = (int)this.Unk.Value);
    }

    public StringSelectionField SourceType { get; set; }
    public StringSelectionField ActionType { get; set; }

    public IntSelectionField    CueID      { get; set; }

    public NumEntryField FadeDuration { get; set; }
    public NumEntryField Unk          { get; set; }

    public BiDict<string, int> ActionTypes = new BiDict<string, int>
    (
        new Dictionary<string, int>
        {
            {"None", 0},
            {"Play", 1},
            {"Stop", 2},
        }
    );

    public BiDict<string, int> SourceTypes = new BiDict<string, int>
    (
        new Dictionary<string, int>
        {
            {"None",   0},
            {"BGM",    1},
            {"System", 2},
            {"SFX",    3},
        }
    );
}
