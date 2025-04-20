using System.Collections.Generic;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class EnDf : Generic
{
    public EnDf(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Environment: Depth of Field";

        // focus/blur
        this.FocalDistance = new NumRangeField("Focal Distance", this.Editable, this.CommandData.FocalPlaneDistance, 0, 100, 1);
        this.NearBlurDistance = new NumRangeField("Near Blur Distance", this.Editable, this.CommandData.NearBlurSurface, 0, 100, 1);
        this.FarBlurDistance = new NumRangeField("Far Blur Distance", this.Editable, this.CommandData.FarBlurSurface, 0, 100, 1);
        this.DistanceBlurLimit = new NumRangeField("Distance Blur Limit", this.Editable, this.CommandData.DistanceBlurLimit, 0, 100, 1);
        this.BlurStrength = new NumRangeField("Blur Strength", this.Editable, this.CommandData.BlurStrength, 0.5, 1, 0.01);
        this.BlurType = new StringSelectionField("Blur Type", this.Editable, this.BlurTypes.Backward[this.CommandData.BlurType], this.BlurTypes.Keys);

        // unknown
        this.Unk = new NumEntryField("Unknown", this.Editable, this.CommandData.Unk1, 0, 3, 1);
    }

    // focus/blur
    public BoolChoiceField      EnableDOF         { get; set; }
    public NumRangeField        FocalDistance     { get; set; }
    public NumRangeField        NearBlurDistance  { get; set; }
    public NumRangeField        FarBlurDistance   { get; set; }
    public NumRangeField        DistanceBlurLimit { get; set; }
    public NumRangeField        BlurStrength      { get; set; }
    public StringSelectionField BlurType          { get; set; }

    // unknown
    public NumEntryField Unk { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();

        this.CommandData.Unk1 = (uint)this.Unk.Value;

        this.CommandData.FocalPlaneDistance = (float)this.FocalDistance.Value;
        this.CommandData.NearBlurSurface = (float)this.NearBlurDistance.Value;
        this.CommandData.FarBlurSurface = (float)this.FarBlurDistance.Value;

        this.CommandData.DistanceBlurLimit = (float)this.DistanceBlurLimit.Value;

        this.CommandData.BlurStrength = (float)this.BlurStrength.Value;
        this.CommandData.BlurType = this.BlurTypes.Forward[this.BlurType.Choice];
    }

    public BiDict<string, uint> BlurTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"5x5 Gaussian Filter",  0},
            {"2-Iteration Gaussian", 1},
            {"3-Iteration Gaussian", 2},
            {"5-Iteration Gaussian", 3},
            {"7-Iteration Gaussian", 4},
        }
    );
}
