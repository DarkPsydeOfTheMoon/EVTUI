using System;
using System.Collections.Generic;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class MAlp : Generic
{
    public MAlp(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Transparency";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);

        this.AlphaLevel = new NumEntryField("Alpha Level", this.Editable, this.CommandData.RGBA[3], 0, 255, 1);
        this.TranslucentMode = new StringSelectionField("Translucent Mode", this.Editable, this.TranslucentModes.Backward[this.CommandData.TranslucentMode], this.TranslucentModes.Keys);

        // interpolation
        this.InterpolationType = new StringSelectionField("Interpolation Type", this.Editable, this.InterpolationTypes.Backward[(this.CommandData.InterpolationParameters & 0xFF)], this.InterpolationTypes.Keys);
        this.SlopeInType = new StringSelectionField("Slope-In Type", this.Editable, this.SlopeTypes.Backward[((this.CommandData.InterpolationParameters >> 8) & 0xF)], this.SlopeTypes.Keys);
        this.SlopeOutType = new StringSelectionField("Slope-Out Type", this.Editable, this.SlopeTypes.Backward[((this.CommandData.InterpolationParameters >> 12) & 0xF)], this.SlopeTypes.Keys);
    }

    public IntSelectionField AssetID { get; set; }

    public NumEntryField        AlphaLevel      { get; set; }
    public StringSelectionField TranslucentMode { get; set; }

    // interpolation
    public StringSelectionField InterpolationType { get; set; }
    public StringSelectionField SlopeInType       { get; set; }
    public StringSelectionField SlopeOutType      { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        this.CommandData.RGBA[3] = (byte)this.AlphaLevel.Value;

        this.CommandData.InterpolationParameters = 0;
        this.CommandData.InterpolationParameters |= this.InterpolationTypes.Forward[this.InterpolationType.Choice];
        this.CommandData.InterpolationParameters |= (this.SlopeTypes.Forward[this.SlopeInType.Choice] << 8);
        this.CommandData.InterpolationParameters |= (this.SlopeTypes.Forward[this.SlopeOutType.Choice] << 12);

        this.CommandData.TranslucentMode = this.TranslucentModes.Forward[this.TranslucentMode.Choice];
    }

    public BiDict<string, uint> InterpolationTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Linear",   0},
            {"Step",     1},
            {"Hermite",  2},
        }
    );

    public BiDict<string, uint> SlopeTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Normal", 0},
            {"Slow",   1},
            {"Fast",   2},
        }
    );

    public BiDict<string, byte> TranslucentModes = new BiDict<string, byte>
    (
        new Dictionary<string, byte>
        {
            {"Normal",    0},
            {"Mask",      1},
            {"Post-Mask", 2},
        }
    );

}
