using System.Collections.Generic;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class MSSs : Generic
{
    public MSSs(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: \"Shoe\" Meshes";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.ShoeLayer = new StringSelectionField("Active \"Shoe\" Mesh Name Prefix", this.Editable, this.ShoeLayers.Backward[this.CommandData.ShoeLayerIndex], this.ShoeLayers.Keys);
    }

    public IntSelectionField AssetID { get; set; }
    public StringSelectionField ShoeLayer { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;
        this.CommandData.ShoeLayerIndex = this.ShoeLayers.Forward[this.ShoeLayer.Choice];
    }

    public BiDict<string, uint> ShoeLayers = new BiDict<string, uint>
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
