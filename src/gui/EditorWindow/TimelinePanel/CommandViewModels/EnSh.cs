using System;
using System.Collections.Generic;

namespace EVTUI.ViewModels.TimelineCommands;

public class EnSh : Generic
{
    public EnSh(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Environment: Shadows";

        this.SetCameraClip = new BoolChoiceField("Set Camera Far Clip to Depth Range?", this.Editable, this.CommandData.Flags[0]);
        this.BackSideDrawingEnabled = new BoolChoiceField("Enable Back Side Drawing?", this.Editable, this.CommandData.Flags[3]);

        this.DepthRange = new NumRangeField("Depth Range", this.Editable, this.CommandData.DepthRange, 0, 9999, 1);
        this.Bias = new NumRangeField("Bias", this.Editable, this.CommandData.Bias, -3, 3, 0.01);
        this.Ambient = new NumRangeField("Ambient", this.Editable, this.CommandData.Ambient, 0, 1, 0.01);
        this.Diffuse = new NumRangeField("Diffuse", this.Editable, this.CommandData.Diffuse, 0, 1, 0.01);
        this.CascadedShadowMapPartitionInterval = new NumRangeField("Cascaded Shadow Map Partition Interval", this.Editable, this.CommandData.CascadedShadowMapPartitionInterval, 0.01, 0.99, 0.01);
    }

    public BoolChoiceField SetCameraClip          { get; set; }
    public BoolChoiceField BackSideDrawingEnabled { get; set; }

    public NumRangeField DepthRange                         { get; set; }
    public NumRangeField Bias                               { get; set; }
    public NumRangeField Ambient                            { get; set; }
    public NumRangeField Diffuse                            { get; set; }
    public NumRangeField CascadedShadowMapPartitionInterval { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();

        this.CommandData.Flags[0] = this.SetCameraClip.Value;
        this.CommandData.Flags[3] = this.BackSideDrawingEnabled.Value;

        this.CommandData.DepthRange = (float)this.DepthRange.Value;
        this.CommandData.Bias = (float)this.Bias.Value;
        this.CommandData.Ambient = (float)this.Ambient.Value;
        this.CommandData.Diffuse = (float)this.Diffuse.Value;
        this.CommandData.CascadedShadowMapPartitionInterval = (float)this.CascadedShadowMapPartitionInterval.Value;
    }
}
