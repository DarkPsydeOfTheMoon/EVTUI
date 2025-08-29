namespace EVTUI.ViewModels.TimelineCommands;

public class ESD_ : Generic
{
    public ESD_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Effect: Placement (Coordinates)";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDsOfType(0x01000002));

        // position
        this.X = new NumRangeField("X", this.Editable, this.CommandData.Position[0], -99999, 99999, 1);
        this.Y = new NumRangeField("Y", this.Editable, this.CommandData.Position[1], -99999, 99999, 1);
        this.Z = new NumRangeField("Z", this.Editable, this.CommandData.Position[2], -99999, 99999, 1);

        // rotation
        this.PitchDegrees = new NumRangeField("Pitch", this.Editable, this.CommandData.Rotation[0], -180, 180, 1);
        this.YawDegrees = new NumRangeField("Yaw", this.Editable, this.CommandData.Rotation[1], -180, 180, 1);
        this.RollDegrees = new NumRangeField("Roll", this.Editable, this.CommandData.Rotation[2], -180, 180, 1);
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

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        this.CommandData.Position[0] = (float)this.X.Value;
        this.CommandData.Position[1] = (float)this.Y.Value;
        this.CommandData.Position[2] = (float)this.Z.Value;

        this.CommandData.Rotation[0] = (float)this.PitchDegrees.Value;
        this.CommandData.Rotation[1] = (float)this.YawDegrees.Value;
        this.CommandData.Rotation[2] = (float)this.RollDegrees.Value;
    }
}
