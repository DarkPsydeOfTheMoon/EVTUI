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
        this.InterpolationSettings = new InterpolationParameters(this.CommandData.InterpolationParameters, this.Editable);
    }

    public IntSelectionField AssetID { get; set; }

    public NumEntryField           AlphaLevel            { get; set; }
    public StringSelectionField    TranslucentMode       { get; set; }
    public InterpolationParameters InterpolationSettings { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        this.CommandData.RGBA[3] = (byte)this.AlphaLevel.Value;
        this.CommandData.InterpolationParameters = this.InterpolationSettings.Compose();
        this.CommandData.TranslucentMode = this.TranslucentModes.Forward[this.TranslucentMode.Choice];
    }

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
