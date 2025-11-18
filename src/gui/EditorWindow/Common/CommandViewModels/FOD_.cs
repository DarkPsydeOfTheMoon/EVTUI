using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class FOD_ : Generic
{
    public FOD_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Field: Object Placement";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDsOfType(0x00000003));
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.ActionType = new StringSelectionField("Mode", this.Editable, this.ActionTypes.Backward[this.CommandData.EnableFieldObject], this.ActionTypes.Keys);
        this.WhenAnyValue(_ => _.ActionType.Choice).Subscribe(_ => this.CommandData.EnableFieldObject = this.ActionTypes.Forward[this.ActionType.Choice]);
        this.ObjectIndex = new NumEntryField("Field Object Index", this.Editable, this.CommandData.ObjectIndex, -1, 65535, 1);
        this.WhenAnyValue(_ => _.ObjectIndex.Value).Subscribe(_ => this.CommandData.ObjectIndex = (int)this.ObjectIndex.Value);

    }

    public IntSelectionField AssetID { get; set; }

    public StringSelectionField ActionType  { get; set; }
    public NumEntryField        ObjectIndex { get; set; }

    public BiDict<string, uint> ActionTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Show", 0},
            {"Hide", 1},
        }
    );

}
