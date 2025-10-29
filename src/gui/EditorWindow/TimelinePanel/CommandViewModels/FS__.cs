using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class FS__ : Generic
{
    public FS__(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Field: Placement";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDsOfType(0x00000003));
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        // position
        this.X = new NumRangeField("X", this.Editable, this.CommandData.Coordinates[0], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.X.Value).Subscribe(_ => this.CommandData.Coordinates[0] = (float)this.X.Value);
        this.Y = new NumRangeField("Y", this.Editable, this.CommandData.Coordinates[1], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.Y.Value).Subscribe(_ => this.CommandData.Coordinates[1] = (float)this.Y.Value);
        this.Z = new NumRangeField("Z", this.Editable, this.CommandData.Coordinates[2], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.Z.Value).Subscribe(_ => this.CommandData.Coordinates[2] = (float)this.Z.Value);

        // rotation
        // i may have yaw and pitch switched around here, dunno
        this.Yaw = new NumRangeField("Yaw", this.Editable, this.CommandData.Rotation[0], -180, 180, 1);
        this.WhenAnyValue(_ => _.Yaw.Value).Subscribe(_ => this.CommandData.Rotation[0] = (float)this.Yaw.Value);
        this.Pitch = new NumRangeField("Pitch", this.Editable, this.CommandData.Rotation[1], -180, 180, 1);
        this.WhenAnyValue(_ => _.Pitch.Value).Subscribe(_ => this.CommandData.Rotation[1] = (float)this.Pitch.Value);
        this.Roll = new NumRangeField("Roll", this.Editable, this.CommandData.Rotation[2], -180, 180, 1);
        this.WhenAnyValue(_ => _.Roll.Value).Subscribe(_ => this.CommandData.Rotation[2] = (float)this.Roll.Value);

        // unknown
        this.UnkBool = new BoolChoiceField("Unknown #1", this.Editable, this.CommandData.UnkBool != 0);
        this.WhenAnyValue(_ => _.UnkBool.Value).Subscribe(_ => this.CommandData.UnkBool = Convert.ToUInt32(this.UnkBool.Value));
        this.UnkFloat = new NumEntryField("Unknown #2", this.Editable, this.CommandData.UnkFloat, -9999, 9999, 1);
        this.WhenAnyValue(_ => _.UnkFloat.Value).Subscribe(_ => this.CommandData.UnkFloat = (float)this.UnkFloat.Value);
    }

    public IntSelectionField AssetID { get; set; }

    // position
    public NumRangeField X { get; set; }
    public NumRangeField Y { get; set; }
    public NumRangeField Z { get; set; }

    // rotation
    public NumRangeField Yaw   { get; set; }
    public NumRangeField Pitch { get; set; }
    public NumRangeField Roll  { get; set; }

    // unknown
    public BoolChoiceField UnkBool  { get; set; }
    public NumEntryField   UnkFloat { get; set; }

    public BiDict<string, uint> ActionTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Hide", 0},
            {"Show", 1},
        }
    );

}
