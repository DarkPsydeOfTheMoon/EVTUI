using System;
using System.Collections.Generic;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class EnSh : Generic
{
    public EnSh(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Environment: Shadows";

        this.SetCameraClip = new BoolChoiceField("Set Camera Far Clip to Depth Range?", this.Editable, this.CommandData.Flags[0]);
        this.WhenAnyValue(_ => _.SetCameraClip.Value).Subscribe(_ => this.CommandData.Flags[0] = this.SetCameraClip.Value);
        this.BackSideDrawingEnabled = new BoolChoiceField("Enable Back Side Drawing?", this.Editable, this.CommandData.Flags[3]);
        this.WhenAnyValue(_ => _.BackSideDrawingEnabled.Value).Subscribe(_ => this.CommandData.Flags[3] = this.BackSideDrawingEnabled.Value);

        this.DepthRange = new NumRangeField("Depth Range", this.Editable, this.CommandData.DepthRange, 0, 9999, 1);
        this.WhenAnyValue(_ => _.DepthRange.Value).Subscribe(_ => this.CommandData.DepthRange = (float)this.DepthRange.Value);
        this.Bias = new NumRangeField("Bias", this.Editable, this.CommandData.Bias, -3, 3, 0.01);
        this.WhenAnyValue(_ => _.Bias.Value).Subscribe(_ => this.CommandData.Bias = (float)this.Bias.Value);
        this.Ambient = new NumRangeField("Ambient", this.Editable, this.CommandData.Ambient, 0, 1, 0.01);
        this.WhenAnyValue(_ => _.Ambient.Value).Subscribe(_ => this.CommandData.Ambient = (float)this.Ambient.Value);
        this.Diffuse = new NumRangeField("Diffuse", this.Editable, this.CommandData.Diffuse, 0, 1, 0.01);
        this.WhenAnyValue(_ => _.Diffuse.Value).Subscribe(_ => this.CommandData.Diffuse = (float)this.Diffuse.Value);
        this.CascadedShadowMapPartitionInterval = new NumRangeField("Cascaded Shadow Map Partition Interval", this.Editable, this.CommandData.CascadedShadowMapPartitionInterval, 0.01, 0.99, 0.01);
        this.WhenAnyValue(_ => _.CascadedShadowMapPartitionInterval.Value).Subscribe(_ => this.CommandData.CascadedShadowMapPartitionInterval = (float)this.CascadedShadowMapPartitionInterval.Value);
    }

    public BoolChoiceField SetCameraClip          { get; set; }
    public BoolChoiceField BackSideDrawingEnabled { get; set; }

    public NumRangeField DepthRange                         { get; set; }
    public NumRangeField Bias                               { get; set; }
    public NumRangeField Ambient                            { get; set; }
    public NumRangeField Diffuse                            { get; set; }
    public NumRangeField CascadedShadowMapPartitionInterval { get; set; }
}
