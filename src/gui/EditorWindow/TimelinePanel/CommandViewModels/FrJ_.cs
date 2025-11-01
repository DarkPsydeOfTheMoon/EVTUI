using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class FrJ_ : Generic
{
    public FrJ_(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Frame Jump";

        this.FrameIndex = new NumRangeField("Index", this.Editable, (int)this.CommandData.JumpToFrame, 0, config.EventManager.EventDuration, 1);
        this.WhenAnyValue(_ => _.FrameIndex.Value).Subscribe(_ => this.CommandData.JumpToFrame = (uint)this.FrameIndex.Value);
    }

    public NumRangeField FrameIndex { get; set; }
}
