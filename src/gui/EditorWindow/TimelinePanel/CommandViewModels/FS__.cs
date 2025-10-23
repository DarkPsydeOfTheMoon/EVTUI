using System;
using System.Collections.Generic;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class FS__ : Generic
{
    public FS__(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Field: Placement";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDsOfType(0x00000003));

        // position
        this.X = new NumRangeField("X", this.Editable, this.CommandData.Coordinates[0], -99999, 99999, 1);
        this.Y = new NumRangeField("Y", this.Editable, this.CommandData.Coordinates[1], -99999, 99999, 1);
        this.Z = new NumRangeField("Z", this.Editable, this.CommandData.Coordinates[2], -99999, 99999, 1);

        // rotation
        // i may have yaw and pitch switched around here, dunno
        this.Yaw = new NumRangeField("Yaw", this.Editable, this.CommandData.Rotation[0], -180, 180, 1);
        this.Pitch = new NumRangeField("Pitch", this.Editable, this.CommandData.Rotation[1], -180, 180, 1);
        this.Roll = new NumRangeField("Roll", this.Editable, this.CommandData.Rotation[2], -180, 180, 1);

        // unknown
        this.UnkBool = new BoolChoiceField("Unknown #1", this.Editable, this.CommandData.UnkBool != 0);
        this.UnkFloat = new NumEntryField("Unknown #2", this.Editable, this.CommandData.UnkFloat, -9999, 9999, 1);
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

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        this.CommandData.UnkBool = Convert.ToUInt32(this.UnkBool.Value);

        this.CommandData.Coordinates[0] = (float)this.X.Value;
        this.CommandData.Coordinates[1] = (float)this.Y.Value;
        this.CommandData.Coordinates[2] = (float)this.Z.Value;

        this.CommandData.Rotation[0] = (float)this.Yaw.Value;
        this.CommandData.Rotation[1] = (float)this.Pitch.Value;
        this.CommandData.Rotation[2] = (float)this.Roll.Value;

        this.CommandData.UnkFloat = (float)this.UnkFloat.Value;
    }

    public BiDict<string, uint> ActionTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Hide", 0},
            {"Show", 1},
        }
    );

}
