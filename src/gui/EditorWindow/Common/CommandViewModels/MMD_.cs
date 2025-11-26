using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using ReactiveUI;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class MMD_ : Generic
{
    public MMD_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Model: Movement";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        // movement
        this.InterpolationType = new StringSelectionField("Interpolation Type", this.Editable, this.InterpolationTypes.Backward[this.CommandData.InterpolationType], this.InterpolationTypes.Keys);
        this.WhenAnyValue(_ => _.InterpolationType.Choice).Subscribe(_ => this.CommandData.InterpolationType = this.InterpolationTypes.Forward[this.InterpolationType.Choice]);
        this.MovementSpeed = new NumEntryField("Movement Speed", this.Editable, this.CommandData.MovementSpeed, 1, 50, 0.1);
        this.WhenAnyValue(_ => _.MovementSpeed.Value).Subscribe(_ => this.CommandData.MovementSpeed = (float)this.MovementSpeed.Value);
        this.MovementLoopBool = new BoolChoiceField("Loop Movement?", this.Editable, this.CommandData.Flags[5]);
        this.WhenAnyValue(_ => _.MovementLoopBool.Value).Subscribe(_ => this.CommandData.Flags[5] = this.MovementLoopBool.Value);
        this.DisableOrientationChange = new BoolChoiceField("Disable Orientation Change?", this.Editable, this.CommandData.Flags[4]);
        this.WhenAnyValue(_ => _.DisableOrientationChange.Value).Subscribe(_ => this.CommandData.Flags[4] = this.DisableOrientationChange.Value);
        this.StartSpeedType = new StringSelectionField("Start Speed Type", this.Editable, this.SpeedTypes.Backward[this.CommandData.StartSpeedType], this.SpeedTypes.Keys);
        this.WhenAnyValue(_ => _.StartSpeedType.Choice).Subscribe(_ => this.CommandData.StartSpeedType = this.SpeedTypes.Forward[this.StartSpeedType.Choice]);
        this.FinalSpeedType = new StringSelectionField("Final Speed Type", this.Editable, this.SpeedTypes.Backward[this.CommandData.FinalSpeedType], this.SpeedTypes.Keys);
        this.WhenAnyValue(_ => _.FinalSpeedType.Choice).Subscribe(_ => this.CommandData.FinalSpeedType = this.SpeedTypes.Forward[this.FinalSpeedType.Choice]);

        this.NumControlGroups = new NumEntryField("Control Groups", this.Editable, this.CommandData.NumControlGroups, 1, 8, 1);
        this.Targets = new ObservableCollection<Target>();
        for (int i=0; i<this.CommandData.Targets.GetLength(0); i++)
            this.Targets.Add(new Target(config, this.CommandData, i, (i < this.NumControlGroups.Value)));
        this.WhenAnyValue(x => x.NumControlGroups.Value).Subscribe(x =>
        {
            this.CommandData.NumControlGroups = (uint)this.NumControlGroups.Value;
            foreach (Target target in this.Targets)
                target.IsActive = (target.Idx < x);
        });

        // animations
        this.MovingAnimation = new AnimationWidget(config, this.AssetID, this.CommandData.MovingAnimation, this.CommandData.Flags, $"Moving Animation", enabledInd:0, extInd:2, enabledFlip:true);
        this.WaitingAnimation = new AnimationWidget(config, this.AssetID, this.CommandData.WaitingAnimation, this.CommandData.Flags, $"Idle Animation", enabledInd:1, extInd:3, enabledFlip:true);

        // unknown
        this.Unk = new NumEntryField("Unknown", this.Editable, this.CommandData.UNK, 0, null, 1);
        this.WhenAnyValue(_ => _.Unk.Value).Subscribe(_ => this.CommandData.UNK = (uint)this.Unk.Value);
    }

    public IntSelectionField AssetID { get; set; }

    // movement
    public StringSelectionField InterpolationType        { get; set; }
    public NumEntryField        MovementSpeed            { get; set; }
    public BoolChoiceField      MovementLoopBool         { get; set; }
    public BoolChoiceField      DisableOrientationChange { get; set; }
    public StringSelectionField StartSpeedType           { get; set; }
    public StringSelectionField FinalSpeedType           { get; set; }
    public NumEntryField        NumControlGroups         { get; set; }

    public ObservableCollection<Target> Targets { get; set; }

    // animations
    public AnimationWidget MovingAnimation { get; set; }
    public AnimationWidget WaitingAnimation { get; set; }

    // unknown
    public NumEntryField Unk { get; set; }

    public BiDict<string, uint> InterpolationTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Linear",       0},
            {"Bezier Curve", 1},
        }
    );

    public BiDict<string, byte> SpeedTypes = new BiDict<string, byte>
    (
        new Dictionary<string, byte>
        {
            {"Fixed",   0},
            {"Running", 1},
            {"Walking", 2},
        }
    );
}
