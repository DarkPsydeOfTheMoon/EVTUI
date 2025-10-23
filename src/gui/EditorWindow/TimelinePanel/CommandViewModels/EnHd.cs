namespace EVTUI.ViewModels.TimelineCommands;

public class EnHd : Generic
{
    public EnHd(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Environment: HDR";

        // tone map
        this.EnableToneMap = new BoolChoiceField("Enable?", this.Editable, this.CommandData.Flags[16]);
        this.ToneMapMediumBrightness = new NumRangeField("Medium Brightness", this.Editable, this.CommandData.ToneMapMediumBrightness, 0, 10, 0.01);
        this.ToneMapBloomStrength = new NumRangeField("Bloom Strength", this.Editable, this.CommandData.ToneMapBloomStrength, 0, 4, 0.01);
        this.ToneMapAdaptiveBrightness = new NumRangeField("Adaptive Brightness", this.Editable, this.CommandData.ToneMapAdaptiveBrightness, 0, 1, 0.01);
        this.ToneMapAdaptiveBloom = new NumRangeField("Adaptive Bloom", this.Editable, this.CommandData.ToneMapAdaptiveBloom, 0, 1.5, 0.01);

        // star filter
        this.EnableStarFilter = new BoolChoiceField("Enable?", this.Editable, this.CommandData.Flags[17]);
        this.StarFilterNumberOfLines = new NumEntryField("Number of Lines", this.Editable, this.CommandData.StarFilterNumberOfLines, 2, 4, 1);
        this.StarFilterLength = new NumRangeField("Length", this.Editable, this.CommandData.StarFilterLength, 0.1, 2, 0.01);
        this.StarFilterStrength = new NumRangeField("Strength", this.Editable, this.CommandData.StarFilterStrength, 0, 4, 0.01);
        this.StarFilterGlareChromaticAberration = new NumRangeField("Glare: Chromatic Aberration", this.Editable, this.CommandData.StarFilterGlareChromaticAberration, 0, 5, 0.01);
        this.StarFilterGlareTilt = new NumRangeField("Glare: Tilt", this.Editable, this.CommandData.StarFilterGlareTilt, 0, 360, 0.1);

        // unknown
        this.UnkBool = new BoolChoiceField("Unknown #1", this.Editable, this.CommandData.Flags[18]);
        this.UnkFloat1 = new NumRangeField("Unknown #2", this.Editable, this.CommandData.UnkFloat1, 0, 100, 0.1);
        this.UnkFloat2 = new NumRangeField("Unknown #3", this.Editable, this.CommandData.UnkFloat2, 0, 6, 0.01);
        this.UnkFloat3 = new NumRangeField("Unknown #4", this.Editable, this.CommandData.UnkFloat3, 0, 2, 0.01);
        this.UnkEnum = new NumEntryField("Unknown #5", this.Editable, this.CommandData.UnkEnum, 1, 2, 1);
        this.UnkColor1 = new ColorSelectionField("Unknown #6", this.Editable, this.CommandData.RGBA1);
        this.UnkColor2 = new ColorSelectionField("Unknown #7", this.Editable, this.CommandData.RGBA2);
        this.UnkColor3 = new ColorSelectionField("Unknown #8", this.Editable, this.CommandData.RGBA3);
    }

    // tone map
    public BoolChoiceField EnableToneMap             { get; set; }
    public NumRangeField   ToneMapMediumBrightness   { get; set; }
    public NumRangeField   ToneMapBloomStrength      { get; set; }
    public NumRangeField   ToneMapAdaptiveBrightness { get; set; }
    public NumRangeField   ToneMapAdaptiveBloom      { get; set; }

    // star filter
    public BoolChoiceField EnableStarFilter                   { get; set; }
    public NumEntryField   StarFilterNumberOfLines            { get; set; }
    public NumRangeField   StarFilterLength                   { get; set; }
    public NumRangeField   StarFilterStrength                 { get; set; }
    public NumRangeField   StarFilterGlareChromaticAberration { get; set; }
    public NumRangeField   StarFilterGlareTilt                { get; set; }

    // unknown
    public BoolChoiceField     UnkBool   { get; set; }
    public NumRangeField       UnkFloat1 { get; set; }
    public NumRangeField       UnkFloat2 { get; set; }
    public NumRangeField       UnkFloat3 { get; set; }
    public NumEntryField       UnkEnum   { get; set; }
    public ColorSelectionField UnkColor1 { get; set; }
    public ColorSelectionField UnkColor2 { get; set; }
    public ColorSelectionField UnkColor3 { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();

        this.CommandData.Flags[16] = this.EnableToneMap.Value;
        this.CommandData.Flags[17] = this.EnableStarFilter.Value;
        this.CommandData.Flags[18] = this.UnkBool.Value;

        this.CommandData.ToneMapMediumBrightness   = (float)this.ToneMapMediumBrightness.Value;
        this.CommandData.ToneMapBloomStrength      = (float)this.ToneMapBloomStrength.Value;
        this.CommandData.ToneMapAdaptiveBrightness = (float)this.ToneMapAdaptiveBrightness.Value;
        this.CommandData.ToneMapAdaptiveBloom      = (float)this.ToneMapAdaptiveBloom.Value;

        this.CommandData.StarFilterNumberOfLines            = (uint)this.StarFilterNumberOfLines.Value;
        this.CommandData.StarFilterLength                   = (float)this.StarFilterLength.Value;
        this.CommandData.StarFilterStrength                 = (float)this.StarFilterStrength.Value;
        this.CommandData.StarFilterGlareChromaticAberration = (float)this.StarFilterGlareChromaticAberration.Value;
        this.CommandData.StarFilterGlareTilt                = (float)this.StarFilterGlareTilt.Value;

        this.CommandData.UnkFloat1 = (float)this.UnkFloat1.Value;
        this.CommandData.UnkFloat2 = (float)this.UnkFloat2.Value;
        this.CommandData.UnkFloat3 = (float)this.UnkFloat3.Value;

        this.CommandData.UnkEnum = (uint)this.UnkEnum.Value;
        this.CommandData.RGBA1   = this.UnkColor1.ToUInt32();
        this.CommandData.RGBA2   = this.UnkColor2.ToUInt32();
        this.CommandData.RGBA3   = this.UnkColor3.ToUInt32();
    }
}
