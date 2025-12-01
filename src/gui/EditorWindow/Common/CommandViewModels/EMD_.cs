using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using ReactiveUI;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class EMD_ : Generic
{
    public EMD_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
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
        this.Positions = new ObservableCollection<Position3D>();
        for (int i=0; i<this.CommandData.Targets.Length; i++)
            this.Positions.Add(new Position3D($"Position #{i+1}", this.Editable, this.CommandData.Targets[i]));
        this.WhenAnyValue(x => x.NumControlGroups.Value).Subscribe(x =>
        {
            this.CommandData.NumControlGroups = (uint)this.NumControlGroups.Value;
            for (int i=0; i<this.Positions.Count; i++)
                this.Positions[i].Name = (i < this.NumControlGroups.Value) ? $"Position #{i+1}" : "";
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

    public ObservableCollection<Position3D> Positions { get; set; }

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
