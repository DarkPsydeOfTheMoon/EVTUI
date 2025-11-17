using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class MScl : Generic
{
    public MScl(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Model: Scale";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.Scale = new NumRangeField("Scale", this.Editable, this.CommandData.Scale, 0, 100, 0.1);
        this.WhenAnyValue(_ => _.Scale.Value).Subscribe(_ => this.CommandData.Scale = (float)this.Scale.Value);
        this.InterpolationSettings = new InterpolationParameters(this.CommandData.InterpolationParameters, this.Editable);
        this.WhenAnyValue(_ => _.InterpolationSettings.InterpolationType.Choice, _ => _.InterpolationSettings.SlopeInType.Choice, _ => _.InterpolationSettings.SlopeOutType.Choice).Subscribe(_ => this.CommandData.InterpolationParameters = this.InterpolationSettings.Compose());
    }

    public IntSelectionField AssetID { get; set; }

    public NumRangeField           Scale                 { get; set; }
    public InterpolationParameters InterpolationSettings { get; set; }
}
