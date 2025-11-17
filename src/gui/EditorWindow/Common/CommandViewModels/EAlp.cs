using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class EAlp : Generic
{
    public EAlp(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Effect: Transparency";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDsOfType(0x01000002));
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.AlphaLevel = new NumEntryField("Alpha Level", this.Editable, this.CommandData.RGBA[3], 0, 255, 1);
        this.WhenAnyValue(_ => _.AlphaLevel.Value).Subscribe(_ => this.CommandData.RGBA[3] = (byte)this.AlphaLevel.Value);
        this.InterpolationSettings = new InterpolationParameters(this.CommandData.InterpolationParameters, this.Editable);
        this.WhenAnyValue(_ => _.InterpolationSettings.InterpolationType.Choice, _ => _.InterpolationSettings.SlopeInType.Choice, _ => _.InterpolationSettings.SlopeOutType.Choice).Subscribe(_ => this.CommandData.InterpolationParameters = this.InterpolationSettings.Compose());
    }

    public IntSelectionField AssetID { get; set; }

    public NumEntryField           AlphaLevel            { get; set; }
    public InterpolationParameters InterpolationSettings { get; set; }
}
