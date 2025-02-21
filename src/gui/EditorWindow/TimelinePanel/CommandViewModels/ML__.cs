using System;
using System.Numerics;

namespace EVTUI.ViewModels.TimelineCommands;

public class ML__ : Generic
{
    public ML__(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Lighting";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);

        this.Enabled = new BoolChoiceField("Directional Light Enabled?", this.Editable, this.CommandData.Enable != 0);

        // colors
        this.AmbientColor = new ColorSelectionField("Ambient", this.Editable, this.CommandData.AmbientRGBA);
        this.DiffuseColor = new ColorSelectionField("Diffuse", this.Editable, this.CommandData.DiffuseRGBA);
        this.SpecularColor = new ColorSelectionField("Specular", this.Editable, this.CommandData.SpecularRGBA);

        // direction
        // this is a unit vector, so it scales to len=1, but it's easier to UIify it as spherical coordinates...
        //this.LeftToRight = new NumRangeField("Left-to-Right", false, this.CommandData.Direction[0], -1, 1, 0.1);
        //this.BottomToTop = new NumRangeField("Bottom-to-Top", false, this.CommandData.Direction[1], -1, 1, 0.1);
        //this.BackToFront = new NumRangeField("Back-to-Front", false, this.CommandData.Direction[2], -1, 1, 0.1);
        this.AzimuthDegrees = new NumRangeField("Azimuth", this.Editable, VectorToAzimuth(this.CommandData.Direction), -180, 180, 1);
        this.ElevationDegrees = new NumRangeField("Elevation", this.Editable, VectorToElevation(this.CommandData.Direction), -90, 90, 1);
    }

    private static double VectorToAzimuth(float[] xyz)
    {
        return Double.RadiansToDegrees(Math.Atan2(xyz[0], xyz[2]));
    }

    private static double VectorToElevation(float[] xyz)
    {
        if (xyz[0] == 0 && xyz[2] == 0)
           return (xyz[1] < 0) ? -90.0 : 90.0;
        Vector3 direction = new Vector3(xyz[0], xyz[1], xyz[2]);
        Vector3 projection = new Vector3(xyz[0], 0, xyz[2]);
        return Double.RadiansToDegrees(((xyz[1] < 0) ? -1.0 : 1.0)*Math.Acos(Vector3.Dot(Vector3.Normalize(direction), Vector3.Normalize(projection))));
    }

    private static float[] AnglesToVector(double azimuth, double elevation)
    {
        azimuth = Double.DegreesToRadians(azimuth);
        elevation = Double.DegreesToRadians(elevation);
        double x = Math.Cos(elevation)*Math.Sin(azimuth);
        double y = Math.Sin(elevation);
        double z = Math.Cos(elevation)*Math.Cos(azimuth);
        Vector3 norm = Vector3.Normalize(new Vector3((float)x, (float)y, (float)z));
        return new float[] { norm.X, norm.Y, norm.Z };
    }

    public IntSelectionField AssetID { get; set; }

    public BoolChoiceField Enabled { get; set; }

    // colors
    public ColorSelectionField AmbientColor  { get; set; }
    public ColorSelectionField DiffuseColor  { get; set; }
    public ColorSelectionField SpecularColor { get; set; }

    // direction
    //public NumRangeField LeftToRight { get; set; }
    //public NumRangeField BottomToTop { get; set; }
    //public NumRangeField BackToFront { get; set; }
    public NumRangeField AzimuthDegrees   { get; set; }
    public NumRangeField ElevationDegrees { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        this.CommandData.Enable = Convert.ToUInt32(this.Enabled.Value);

        this.CommandData.AmbientRGBA = this.AmbientColor.ToUInt32();
        this.CommandData.DiffuseRGBA = this.DiffuseColor.ToUInt32();
        this.CommandData.SpecularRGBA = this.SpecularColor.ToUInt32();

        //this.CommandData.Direction[0] = (float)this.LeftToRight.Value;
        //this.CommandData.Direction[1] = (float)this.BottomToTop.Value;
        //this.CommandData.Direction[2] = (float)this.BackToFront.Value;
        this.CommandData.Direction = AnglesToVector((double)this.AzimuthDegrees.Value, (double)this.ElevationDegrees.Value);
    }
}
