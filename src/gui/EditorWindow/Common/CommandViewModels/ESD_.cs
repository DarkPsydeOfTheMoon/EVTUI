using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class ESD_ : Generic
{
    public ESD_(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Effect: Placement (Coordinates)";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDsOfType(0x01000002));
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        // position
        this.X = new NumRangeField("X", this.Editable, this.CommandData.Position[0], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.X.Value).Subscribe(_ => this.CommandData.Position[0] = (float)this.X.Value);
        this.Y = new NumRangeField("Y", this.Editable, this.CommandData.Position[1], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.Y.Value).Subscribe(_ => this.CommandData.Position[1] = (float)this.Y.Value);
        this.Z = new NumRangeField("Z", this.Editable, this.CommandData.Position[2], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.Z.Value).Subscribe(_ => this.CommandData.Position[2] = (float)this.Z.Value);

        // rotation
        this.PitchDegrees = new NumRangeField("Pitch", this.Editable, this.CommandData.Rotation[0], -180, 180, 1);
        this.WhenAnyValue(_ => _.PitchDegrees.Value).Subscribe(_ => this.CommandData.Rotation[0] = (float)this.PitchDegrees.Value);
        this.YawDegrees = new NumRangeField("Yaw", this.Editable, this.CommandData.Rotation[1], -180, 180, 1);
        this.WhenAnyValue(_ => _.YawDegrees.Value).Subscribe(_ => this.CommandData.Rotation[1] = (float)this.YawDegrees.Value);
        this.RollDegrees = new NumRangeField("Roll", this.Editable, this.CommandData.Rotation[2], -180, 180, 1);
        this.WhenAnyValue(_ => _.RollDegrees.Value).Subscribe(_ => this.CommandData.Rotation[2] = (float)this.RollDegrees.Value);
    }

    public IntSelectionField AssetID { get; set; }

    // position
    public NumRangeField X { get; set; }
    public NumRangeField Y { get; set; }
    public NumRangeField Z { get; set; }

    // rotation
    public NumRangeField PitchDegrees { get; set; }
    public NumRangeField YawDegrees   { get; set; }
    public NumRangeField RollDegrees  { get; set; }
}
