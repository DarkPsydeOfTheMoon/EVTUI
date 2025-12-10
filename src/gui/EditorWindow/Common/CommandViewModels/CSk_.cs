using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.Utils;

namespace EVTUI.ViewModels.TimelineCommands;

public class CSk_ : Generic
{
    public CSk_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Camera: Shaking Effect (Old)";

        this.VibrationType = new StringSelectionField("Vibration Mode", this.Editable, this.VibrationTypes.Backward[this.CommandData.VibrationMode], this.VibrationTypes.Keys);
        this.WhenAnyValue(_ => _.VibrationType.Choice).Subscribe(_ => this.CommandData.VibrationMode = this.VibrationTypes.Forward[this.VibrationType.Choice]);
    }

    public StringSelectionField VibrationType { get; set; }

    public BiDict<string, uint> VibrationTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"None",   0},
            {"Stop",   1},
            {"Off",    2},
            {"Low",    3},
            {"Middle", 4},
            {"High",   5},
        }
    );
}
