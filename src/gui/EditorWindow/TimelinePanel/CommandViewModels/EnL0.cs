using System;

using ReactiveUI;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class EnL0 : Generic
{
    public EnL0(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Environment: Lighting (Objects)";

        // colors
        this.AmbientColor = new ColorSelectionField("Ambient", this.Editable, this.CommandData.AmbientRGBA);
        this.WhenAnyValue(_ => _.AmbientColor.SelectedColor).Subscribe(_ => this.CommandData.AmbientRGBA = this.AmbientColor.ToUInt32());
        this.DiffuseColor = new ColorSelectionField("Diffuse", this.Editable, this.CommandData.DiffuseRGBA);
        this.WhenAnyValue(_ => _.DiffuseColor.SelectedColor).Subscribe(_ => this.CommandData.DiffuseRGBA = this.DiffuseColor.ToUInt32());
        this.SpecularColor = new ColorSelectionField("Specular", this.Editable, this.CommandData.SpecularRGBA);
        this.WhenAnyValue(_ => _.SpecularColor.SelectedColor).Subscribe(_ => this.CommandData.SpecularRGBA = this.SpecularColor.ToUInt32());

        // direction
        this.AzimuthDegrees = new NumRangeField("Azimuth", this.Editable, VectorToAzimuth(this.CommandData.Direction), -180, 180, 1);
        this.ElevationDegrees = new NumRangeField("Elevation", this.Editable, VectorToElevation(this.CommandData.Direction), -90, 90, 1);
        this.WhenAnyValue(_ => _.AzimuthDegrees.Value, _ => _.ElevationDegrees.Value).Subscribe(_ => this.CommandData.Direction = AnglesToVector((double)this.AzimuthDegrees.Value, (double)this.ElevationDegrees.Value));
    }

    // colors
    public ColorSelectionField AmbientColor  { get; set; }
    public ColorSelectionField DiffuseColor  { get; set; }
    public ColorSelectionField SpecularColor { get; set; }

    // direction
    public NumRangeField AzimuthDegrees   { get; set; }
    public NumRangeField ElevationDegrees { get; set; }
}
