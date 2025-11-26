using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class MLa_ : Generic
{
    public MLa_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Model: Head Motion";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        // head movement basics
        this.ResetEyes = new BoolChoiceField("Reset Eyes When Moving?", this.Editable, this.CommandData.ResetEyeWhenMoving != 0);
        this.WhenAnyValue(_ => _.ResetEyes.Value).Subscribe(_ => this.CommandData.ResetEyeWhenMoving = Convert.ToUInt16(this.ResetEyes.Value));
        this.MotionType = new StringSelectionField("Motion Type", this.Editable, this.MotionTypes.Backward[this.CommandData.MotionType], this.MotionTypes.Keys);
        this.WhenAnyValue(_ => _.MotionType.Choice).Subscribe(_ => this.CommandData.MotionType = this.MotionTypes.Forward[this.MotionType.Choice]);
        this.SpeedType = new StringSelectionField("Speed Type", this.Editable, this.SpeedTypes.Backward[this.CommandData.SpeedType], this.SpeedTypes.Keys);
        this.WhenAnyValue(_ => _.SpeedType.Choice).Subscribe(_ => this.CommandData.SpeedType = this.SpeedTypes.Forward[this.SpeedType.Choice]);

        // lookat basics
        this.EyeMovementEnabled = new BoolChoiceField("Enable Eye Movement?", this.Editable, this.CommandData.Flags[0]);
        this.WhenAnyValue(_ => _.EyeMovementEnabled.Value).Subscribe(_ => this.CommandData.Flags[0] = this.EyeMovementEnabled.Value);
        this.HeadMovementEnabled = new BoolChoiceField("Enable Head Movement?", this.Editable, this.CommandData.Flags[1]);
        this.WhenAnyValue(_ => _.HeadMovementEnabled.Value).Subscribe(_ => this.CommandData.Flags[1] = this.HeadMovementEnabled.Value);
        this.TorsoMovementEnabled = new BoolChoiceField("Enable Torso Movement?", this.Editable, this.CommandData.Flags[2]);
        this.WhenAnyValue(_ => _.TorsoMovementEnabled.Value).Subscribe(_ => this.CommandData.Flags[2] = this.TorsoMovementEnabled.Value);
        this.SlowTorsoMovement = new BoolChoiceField("Slow Torso Movement?", this.Editable, this.CommandData.Flags[5]);
        this.WhenAnyValue(_ => _.SlowTorsoMovement.Value).Subscribe(_ => this.CommandData.Flags[5] = this.SlowTorsoMovement.Value);
        this.TargetType = new StringSelectionField("Target Type", this.Editable, this.TargetTypes.Backward[this.CommandData.TargetType], this.TargetTypes.Keys);
        this.WhenAnyValue(_ => _.TargetType.Choice).Subscribe(_ => this.CommandData.TargetType = this.TargetTypes.Forward[this.TargetType.Choice]);

        // coordinate lookat
        this.X = new NumRangeField("X", this.Editable, this.CommandData.Target[0], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.X.Value).Subscribe(_ => this.CommandData.Target[0] = (float)this.X.Value);
        this.Y = new NumRangeField("Y", this.Editable, this.CommandData.Target[1], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.Y.Value).Subscribe(_ => this.CommandData.Target[1] = (float)this.Y.Value);
        this.Z = new NumRangeField("Z", this.Editable, this.CommandData.Target[2], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.Z.Value).Subscribe(_ => this.CommandData.Target[2] = (float)this.Z.Value);

        // bone lookat
        this.TargetModelID = new NumEntryField("Target Model ID", this.Editable, this.CommandData.TargetModelID, 0, 999, 1);
        this.WhenAnyValue(_ => _.TargetModelID.Value).Subscribe(_ => this.CommandData.TargetModelID = (uint)this.TargetModelID.Value);
        this.TargetHelperID = new NumEntryField("Target Helper ID", this.Editable, this.CommandData.TargetHelperID, 0, 9999, 1);
        this.WhenAnyValue(_ => _.TargetHelperID.Value).Subscribe(_ => this.CommandData.TargetHelperID = (uint)this.TargetHelperID.Value);

        // unknown
        this.UnkBool = new BoolChoiceField("Unknown Bool", this.Editable, this.CommandData.Flags[6]);
        this.WhenAnyValue(_ => _.UnkBool.Value).Subscribe(_ => this.CommandData.Flags[6] = this.UnkBool.Value);
    }

    public IntSelectionField AssetID   { get; set; }

    // head movement basics
    public BoolChoiceField      ResetEyes  { get; set; }
    public StringSelectionField MotionType { get; set; }
    public StringSelectionField SpeedType  { get; set; }

    // lookat basics
    public BoolChoiceField      EyeMovementEnabled   { get; set; }
    public BoolChoiceField      HeadMovementEnabled  { get; set; }
    public BoolChoiceField      TorsoMovementEnabled { get; set; }
    public BoolChoiceField      SlowTorsoMovement    { get; set; }
    public StringSelectionField TargetType           { get; set; }

    // coordinate lookat
    public NumRangeField X { get; set; }
    public NumRangeField Y { get; set; }
    public NumRangeField Z { get; set; }

    // bone lookat
    public NumEntryField TargetModelID { get; set; }
    public NumEntryField TargetHelperID  { get; set; }

    // unknown
    public BoolChoiceField UnkBool { get; set; }

    public BiDict<string, ushort> MotionTypes = new BiDict<string, ushort>
    (
        new Dictionary<string, ushort>
        {
            {"Look-At",     0},
            {"Reset",       1},
            {"Nodding",     2},
            {"Shaking",     3},
            {"Look-Around", 4},
        }
    );

    public BiDict<string, ushort> SpeedTypes = new BiDict<string, ushort>
    (
        new Dictionary<string, ushort>
        {
            {"Direct", 0},
            {"Fast",   3},
            {"Middle", 2},
            {"Slow",   1},
        }
    );

    public BiDict<string, ushort> TargetTypes = new BiDict<string, ushort>
    (
        new Dictionary<string, ushort>
        {
            {"None",        0},
            {"Reset",       1},
            {"Coordinates", 2},
            {"Model",       3},
        }
    );
}
