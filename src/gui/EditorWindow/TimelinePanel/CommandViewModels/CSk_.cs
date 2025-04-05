using System.Collections.Generic;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class CSk_ : Generic
{
    public CSk_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Camera: Shaking Effect (Old)";

        this.VibrationType = new StringSelectionField("Vibration Mode", this.Editable, this.VibrationTypes.Backward[this.CommandData.VibrationMode], this.VibrationTypes.Keys);
    }

    public StringSelectionField VibrationType { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();

        this.CommandData.VibrationMode = this.VibrationTypes.Forward[this.VibrationType.Choice];
    }

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
