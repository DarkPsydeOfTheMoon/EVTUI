using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.Utils;

namespace EVTUI.ViewModels.TimelineCommands;

public class MSSs : Generic
{
    public MSSs(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Model: \"Shoe\" Visibility";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.ShoeLayer = new StringSelectionField("Active \"Shoe\" Node Name Prefix", this.Editable, MSSs.ShoeLayers.Backward[this.CommandData.ShoeLayerIndex], MSSs.ShoeLayers.Keys);
        this.WhenAnyValue(_ => _.ShoeLayer.Choice).Subscribe(_ => this.CommandData.ShoeLayerIndex = MSSs.ShoeLayers.Forward[this.ShoeLayer.Choice]);
    }

    public IntSelectionField AssetID { get; set; }
    public StringSelectionField ShoeLayer { get; set; }

    public static BiDict<string, uint> ShoeLayers = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"shoe_0", 1},
            {"shoe_1", 2},
            {"shoe_2", 3},
            {"shoe_3", 4},
        }
    );
}
