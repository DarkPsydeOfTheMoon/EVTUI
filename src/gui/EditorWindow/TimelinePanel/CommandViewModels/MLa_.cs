using System;
using System.Collections.Generic;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class MLa_ : Generic
{
    public MLa_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Head Motion";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);

        // head movement basics
        this.ResetEyes = new BoolChoiceField("Reset Eyes When Moving?", this.Editable, this.CommandData.ResetEyeWhenMoving != 0);
        this.MotionType = new StringSelectionField("Motion Type", this.Editable, this.MotionTypes.Backward[this.CommandData.MotionType], this.MotionTypes.Keys);
        this.SpeedType = new StringSelectionField("Speed Type", this.Editable, this.SpeedTypes.Backward[this.CommandData.SpeedType], this.SpeedTypes.Keys);

        // lookat basics
        this.EyeMovementEnabled = new BoolChoiceField("Enable Eye Movement?", this.Editable, BitToBool(this.CommandData.Bitfield, 0));
        this.HeadMovementEnabled = new BoolChoiceField("Enable Head Movement?", this.Editable, BitToBool(this.CommandData.Bitfield, 1));
        this.TorsoMovementEnabled = new BoolChoiceField("Enable Torso Movement?", this.Editable, BitToBool(this.CommandData.Bitfield, 2));
        this.SlowTorsoMovement = new BoolChoiceField("Slow Torso Movement?", this.Editable, BitToBool(this.CommandData.Bitfield, 5));
        this.TargetType = new StringSelectionField("Target Type", this.Editable, this.TargetTypes.Backward[this.CommandData.TargetType], this.TargetTypes.Keys);

        // coordinate lookat
        this.X = new NumRangeField("X", this.Editable, this.CommandData.Target[0], -99999, 99999, 1);
        this.Y = new NumRangeField("Y", this.Editable, this.CommandData.Target[1], -99999, 99999, 1);
        this.Z = new NumRangeField("Z", this.Editable, this.CommandData.Target[2], -99999, 99999, 1);

        // bone lookat
        this.TargetModelID = new NumEntryField("Target Model ID", this.Editable, this.CommandData.TargetModelID, 0, 999, 1);
        this.TargetBoneID = new NumEntryField("Target Bone ID", this.Editable, this.CommandData.TargetBoneID, 0, 9999, 1);

        // unknown
        this.UnkBool = new BoolChoiceField("Unknown Bool", this.Editable, BitToBool(this.CommandData.Bitfield, 6));
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
    public NumEntryField TargetBoneID  { get; set; }

    // unknown
    public BoolChoiceField UnkBool { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId      = this.AssetID.Choice;

        this.CommandData.ResetEyeWhenMoving = Convert.ToUInt16(this.ResetEyes.Value);

        this.CommandData.Bitfield = 0;
        this.CommandData.Bitfield |= BoolToBit(this.EyeMovementEnabled.Value,   0);
        this.CommandData.Bitfield |= BoolToBit(this.HeadMovementEnabled.Value,  1);
        this.CommandData.Bitfield |= BoolToBit(this.TorsoMovementEnabled.Value, 2);
        this.CommandData.Bitfield |= BoolToBit(this.SlowTorsoMovement.Value,    5);
        this.CommandData.Bitfield |= BoolToBit(this.UnkBool.Value,              6);

        this.CommandData.MotionType = this.MotionTypes.Forward[this.MotionType.Choice];
        this.CommandData.SpeedType  = this.SpeedTypes.Forward[this.SpeedType.Choice];
        this.CommandData.TargetType = this.TargetTypes.Forward[this.TargetType.Choice];

        this.CommandData.Target[0]     = (float)this.X.Value;
        this.CommandData.Target[1]     = (float)this.Y.Value;
        this.CommandData.Target[2]     = (float)this.Z.Value;
        this.CommandData.TargetModelID = (uint)this.TargetModelID.Value;
        this.CommandData.TargetBoneID  = (uint)this.TargetBoneID.Value;
    }

    public BiDict<string, ushort> MotionTypes = new BiDict<string, ushort>
    (
        new Dictionary<string, ushort>
        {
            {"Look-At", 0},
            {"Reset",   1},
            {"Nodding", 2},
            {"Shaking", 3},
            {"UNKNOWN", 4},
        }
    );

    public BiDict<string, ushort> SpeedTypes = new BiDict<string, ushort>
    (
        new Dictionary<string, ushort>
        {
            {"Direct", 0},
            {"Slow",   1},
            {"Middle", 2},
            {"Fast",   3},
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
