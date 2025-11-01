using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using ReactiveUI;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class EMD_ : Generic
{
    public EMD_(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Effect: Movement";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDsOfType(0x01000002));
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        // movement
        this.InterpolationType = new StringSelectionField("Interpolation Type", this.Editable, this.InterpolationTypes.Backward[this.CommandData.InterpolationType], this.InterpolationTypes.Keys);
        this.WhenAnyValue(_ => _.InterpolationType.Choice).Subscribe(_ => this.CommandData.InterpolationType = this.InterpolationTypes.Forward[this.InterpolationType.Choice]);
        this.MovementSpeed = new NumEntryField("Movement Speed", this.Editable, this.CommandData.MovementSpeed, 1, 50, 0.1);
        this.WhenAnyValue(_ => _.MovementSpeed.Value).Subscribe(_ => this.CommandData.MovementSpeed  = (float)this.MovementSpeed.Value);

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

        // unknown
        this.Unk = new NumEntryField("Unknown", this.Editable, this.CommandData.UNK, 0, null, 1);
        this.WhenAnyValue(_ => _.Unk.Value).Subscribe(_ => this.CommandData.UNK = (uint)this.Unk.Value);
    }

    public IntSelectionField AssetID { get; set; }

    // movement
    public StringSelectionField InterpolationType        { get; set; }
    public NumEntryField        MovementSpeed            { get; set; }
    public NumEntryField        NumControlGroups         { get; set; }

    public ObservableCollection<Target> Targets { get; set; }

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
}
