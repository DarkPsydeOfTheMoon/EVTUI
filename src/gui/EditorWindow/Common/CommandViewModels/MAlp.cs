using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class MAlp : Generic
{
    public MAlp(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Model: Transparency";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.AlphaLevel = new NumEntryField("Alpha Level", this.Editable, this.CommandData.RGBA[3], 0, 255, 1);
        this.WhenAnyValue(_ => _.AlphaLevel.Value).Subscribe(_ => this.CommandData.RGBA[3] = (byte)this.AlphaLevel.Value);
        this.TranslucentMode = new StringSelectionField("Translucent Mode", this.Editable, this.TranslucentModes.Backward[this.CommandData.TranslucentMode], this.TranslucentModes.Keys);
        this.WhenAnyValue(_ => _.TranslucentMode.Choice).Subscribe(_ => this.CommandData.TranslucentMode = this.TranslucentModes.Forward[this.TranslucentMode.Choice]);
        this.InterpolationSettings = new InterpolationParameters(this.CommandData.InterpolationParameters, this.Editable);
        this.WhenAnyValue(_ => _.InterpolationSettings.InterpolationType.Choice, _ => _.InterpolationSettings.SlopeInType.Choice, _ => _.InterpolationSettings.SlopeOutType.Choice).Subscribe(_ => this.CommandData.InterpolationParameters = this.InterpolationSettings.Compose());
    }

    public IntSelectionField AssetID { get; set; }

    public NumEntryField           AlphaLevel            { get; set; }
    public StringSelectionField    TranslucentMode       { get; set; }
    public InterpolationParameters InterpolationSettings { get; set; }

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
