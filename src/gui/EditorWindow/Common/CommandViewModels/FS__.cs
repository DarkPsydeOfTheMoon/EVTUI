using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.Utils;

namespace EVTUI.ViewModels.TimelineCommands;

public class FS__ : Generic
{
    public FS__(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Field: Placement";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDsOfType(0x00000003));
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.Position = new Position3D("Offset", this.Editable, this.CommandData.Position);
        // i may have yaw and pitch switched around here, dunno
        this.Rotation = new RotationWidget(config, this.CommandData.Rotation, null, yawInd: 0, pitchInd: 1);

        // unknown
        this.UnkBool = new BoolChoiceField("Unknown #1", this.Editable, this.CommandData.UnkBool != 0);
        this.WhenAnyValue(_ => _.UnkBool.Value).Subscribe(_ => this.CommandData.UnkBool = Convert.ToUInt32(this.UnkBool.Value));
        this.UnkFloat = new NumEntryField("Unknown #2", this.Editable, this.CommandData.UnkFloat, -9999, 9999, 1);
        this.WhenAnyValue(_ => _.UnkFloat.Value).Subscribe(_ => this.CommandData.UnkFloat = (float)this.UnkFloat.Value);
    }

    public IntSelectionField AssetID { get; set; }

    public Position3D     Position { get; set; }
    public RotationWidget Rotation { get; set; }

    // unknown
    public BoolChoiceField UnkBool  { get; set; }
    public NumEntryField   UnkFloat { get; set; }
}
