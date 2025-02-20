using System.Collections.Generic;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class MAtO : Generic
{
    public MAtO(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Attachment Offset";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);

        this.ChildAssetID = new IntSelectionField("Attached Asset ID", this.Editable, this.CommandData.ChildObjectId, config.EventManager.AssetIDs);

        this.InterpolationType = new StringSelectionField("Interpolation Type", this.Editable, this.InterpolationTypes.Backward[this.CommandData.InterpolationType], this.InterpolationTypes.Keys);

        this.XOffset = new NumRangeField("X", this.Editable, this.CommandData.RelativePosition[0], -99999, 99999, 1);
        this.YOffset = new NumRangeField("Y", this.Editable, this.CommandData.RelativePosition[1], -99999, 99999, 1);
        this.ZOffset = new NumRangeField("Z", this.Editable, this.CommandData.RelativePosition[2], -99999, 99999, 1);

        this.Pitch = new NumRangeField("Pitch", this.Editable, this.CommandData.Rotation[0], -180, 180, 1);
        this.Yaw = new NumRangeField("Yaw", this.Editable, this.CommandData.Rotation[1], -180, 180, 1);
        this.Roll = new NumRangeField("Roll", this.Editable, this.CommandData.Rotation[2], -180, 180, 1);

        this.ParentModelPreviewVM = new ModelPreviewWidget(config, this.AssetID);
        this.ChildModelPreviewVM = new ModelPreviewWidget(config, this.ChildAssetID);
    }

    public ModelPreviewWidget ParentModelPreviewVM { get; set; }
    public ModelPreviewWidget ChildModelPreviewVM  { get; set; }

    public IntSelectionField AssetID      { get; set; }

    public IntSelectionField ChildAssetID         { get; set; }
    public StringSelectionField InterpolationType { get; set; }

    public NumRangeField XOffset { get; set; }
    public NumRangeField YOffset { get; set; }
    public NumRangeField ZOffset { get; set; }

    public NumRangeField Pitch { get; set; }
    public NumRangeField Yaw   { get; set; }
    public NumRangeField Roll  { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId          = this.AssetID.Choice;

        this.CommandData.ChildObjectId = this.ChildAssetID.Choice;

        this.CommandData.RelativePosition[0] = (float)this.XOffset.Value;
        this.CommandData.RelativePosition[1] = (float)this.YOffset.Value;
        this.CommandData.RelativePosition[2] = (float)this.ZOffset.Value;

        this.CommandData.Rotation[0]         = (float)this.Pitch.Value;
        this.CommandData.Rotation[1]         = (float)this.Yaw.Value;
        this.CommandData.Rotation[2]         = (float)this.Roll.Value;

        this.CommandData.InterpolationType = this.InterpolationTypes.Forward[this.InterpolationType.Choice];
    }

    public BiDict<string, uint> InterpolationTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Linear",  0},
            {"Step",    1},
            {"Hermite", 2},
        }
    );

}
