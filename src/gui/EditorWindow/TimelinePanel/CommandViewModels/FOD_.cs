using System.Collections.Generic;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class FOD_ : Generic
{
    public FOD_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Field: Object Placement";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDsOfType(0x00000003));

        this.ActionType = new StringSelectionField("Mode", this.Editable, this.ActionTypes.Backward[this.CommandData.EnableFieldObject], this.ActionTypes.Keys);
        this.ObjectIndex = new NumEntryField("Field Object Index", this.Editable, this.CommandData.ObjectIndex, 0, 65535, 1);

    }

    public IntSelectionField AssetID { get; set; }

    public StringSelectionField ActionType  { get; set; }
    public NumEntryField        ObjectIndex { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        this.CommandData.EnableFieldObject = this.ActionTypes.Forward[this.ActionType.Choice];
        this.CommandData.ObjectIndex       = (uint)this.ObjectIndex.Value;
    }

    public BiDict<string, uint> ActionTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Hide", 0},
            {"Show", 1},
        }
    );

}
