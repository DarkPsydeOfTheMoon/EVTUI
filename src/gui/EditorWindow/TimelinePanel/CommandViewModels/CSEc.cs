using System;
using System.Collections.Generic;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class CSEc : Generic
{
    public CSEc(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Camera: Load From Asset";

        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.CommandData.AssetId, config.EventManager.AssetIDsOfType(0x00000009));
        this.EnableEditing = new BoolChoiceField("Enable Editing?", this.Editable, this.CommandData.Flags[0]);

        // message
        this.EnableMessageTypes = new BoolChoiceField("Specify Message Position Type?", this.Editable, this.CommandData.Flags[3]);
        this.EnableMessageCoordinates = new BoolChoiceField("Directly Specify Message Coordinates?", this.Editable, this.CommandData.Flags[4]);
        this.MessageCoordinateType = new StringSelectionField("Coordinate Type", this.Editable, this.MessageCoordinateTypes.Backward[this.CommandData.MessageCoordinateType], this.MessageCoordinateTypes.Keys);
        this.MessageX = new NumRangeField("X Coordinate", this.Editable, this.CommandData.MessageCoordinates[0], -9999, 9999, 1);
        this.MessageY = new NumRangeField("Y Coordinate", this.Editable, this.CommandData.MessageCoordinates[1], -9999, 9999, 1);

        // unknown
        this.UnkBool1 = new BoolChoiceField("Unknown #1", this.Editable, this.CommandData.Flags[2]);
        this.UnkBool2 = new BoolChoiceField("Unknown #2", this.Editable, this.CommandData.Flags[5]);
        this.UnkBool3 = new BoolChoiceField("Unknown #3", this.Editable, this.CommandData.UnkBool != 0);
        this.UnkEnum = new NumEntryField("Unknown #4", this.Editable, this.CommandData.UnkEnum, 0, 2, 1);
    }

    public IntSelectionField AssetID       { get; set; }
    public BoolChoiceField   EnableEditing { get; set; }

    // message
    public BoolChoiceField      EnableMessageCoordinates { get; set; }
    public BoolChoiceField      EnableMessageTypes       { get; set; }
    public StringSelectionField MessageCoordinateType    { get; set; }
    public NumRangeField        MessageX                 { get; set; }
    public NumRangeField        MessageY                 { get; set; }

    // unknown
    public BoolChoiceField UnkBool1 { get; set; }
    public BoolChoiceField UnkBool2 { get; set; }
    public BoolChoiceField UnkBool3 { get; set; }
    public NumEntryField   UnkEnum  { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();

        this.CommandData.Flags[0] = this.EnableEditing.Value;
        this.CommandData.Flags[2] = this.UnkBool1.Value;
        this.CommandData.Flags[3] = this.EnableMessageTypes.Value;
        this.CommandData.Flags[4] = this.EnableMessageCoordinates.Value;
        this.CommandData.Flags[5] = this.UnkBool2.Value;

        this.CommandData.AssetId = this.AssetID.Choice;

        this.CommandData.MessageCoordinateType = this.MessageCoordinateTypes.Forward[this.MessageCoordinateType.Choice];
        this.CommandData.MessageCoordinates[0] = (float)this.MessageX.Value;
        this.CommandData.MessageCoordinates[1] = (float)this.MessageY.Value;

        this.CommandData.UnkBool           = Convert.ToByte(this.UnkBool3.Value);
        this.CommandData.UnkEnum           = (byte)this.UnkEnum.Value;
    }

    public BiDict<string, uint> MessageCoordinateTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Top Left",      0},
            {"Top Center",    1},
            {"Top Right",     2},
            {"Bottom Left",   3},
            {"Bottom Center", 4},
            {"Bottom Right",  5},
        }
    );
}
