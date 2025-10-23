using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class EnLI : Generic
{
    public EnLI(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Environment: Lighting (Characters)";

        // colors
        this.AmbientColor = new ColorSelectionField("Ambient", this.Editable, this.CommandData.AmbientRGBA);
        this.DiffuseColor = new ColorSelectionField("Diffuse", this.Editable, this.CommandData.DiffuseRGBA);
        this.SpecularColor = new ColorSelectionField("Specular", this.Editable, this.CommandData.SpecularRGBA);

        // direction
        this.AzimuthDegrees = new NumRangeField("Azimuth", this.Editable, VectorToAzimuth(this.CommandData.Direction), -180, 180, 1);
        this.ElevationDegrees = new NumRangeField("Elevation", this.Editable, VectorToElevation(this.CommandData.Direction), -90, 90, 1);
    }

    // colors
    public ColorSelectionField AmbientColor  { get; set; }
    public ColorSelectionField DiffuseColor  { get; set; }
    public ColorSelectionField SpecularColor { get; set; }

    // direction
    public NumRangeField AzimuthDegrees   { get; set; }
    public NumRangeField ElevationDegrees { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();

        this.CommandData.AmbientRGBA = this.AmbientColor.ToUInt32();
        this.CommandData.DiffuseRGBA = this.DiffuseColor.ToUInt32();
        this.CommandData.SpecularRGBA = this.SpecularColor.ToUInt32();

        this.CommandData.Direction = AnglesToVector((double)this.AzimuthDegrees.Value, (double)this.ElevationDegrees.Value);
    }
}
