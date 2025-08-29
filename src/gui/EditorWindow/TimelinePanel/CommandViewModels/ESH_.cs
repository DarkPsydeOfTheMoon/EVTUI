namespace EVTUI.ViewModels.TimelineCommands;

public class ESH_ : Generic
{
    public ESH_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Effect: Placement (Helper)";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDsOfType(0x01000002));

        this.AlwaysSynchronize = new BoolChoiceField("Always Synchronize?", this.Editable, this.CommandData.Flags[1]);
        this.ModelAssetID = new IntSelectionField("Parent Asset ID", this.Editable, this.CommandData.ModelObjectId, config.EventManager.AssetIDs);
        this.HelperID = new NumEntryField("Helper ID", this.Editable, this.CommandData.HelperId, 0, 9999, 1);

        this.CorrectionFrameEnabled = new BoolChoiceField("Correction Frames Enabled?", this.Editable, this.CommandData.Flags[0]);
        this.StartCorrectionFrameNumber = new NumRangeField("Frame Count", this.Editable, this.CommandData.StartCorrectionFrameNumber, 0, 60, 1);
        this.StartInterpolationSettings = new InterpolationParameters(this.CommandData.StartInterpolationParameters, this.Editable);
        this.EndCorrectionFrameNumber = new NumRangeField("Frame Count", this.Editable, this.CommandData.EndCorrectionFrameNumber, 0, 60, 1);
        this.EndInterpolationSettings = new InterpolationParameters(this.CommandData.EndInterpolationParameters, this.Editable);

        this.ParentModelPreviewVM = new ModelPreviewWidget(config, this.ModelAssetID);
    }

    public ModelPreviewWidget ParentModelPreviewVM { get; set; }

    public IntSelectionField AssetID { get; set; }

    public BoolChoiceField   AlwaysSynchronize { get; set; }
    public IntSelectionField ModelAssetID      { get; set; }
    public NumEntryField     HelperID          { get; set; } // TODO: parse GFD and present as string selection...

    public BoolChoiceField         CorrectionFrameEnabled     { get; set; }
    public NumRangeField           StartCorrectionFrameNumber { get; set; }
    public InterpolationParameters StartInterpolationSettings { get; set; }
    public NumRangeField           EndCorrectionFrameNumber   { get; set; }
    public InterpolationParameters EndInterpolationSettings   { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        this.CommandData.Flags[0] = this.CorrectionFrameEnabled.Value;
        this.CommandData.Flags[1] = this.AlwaysSynchronize.Value;

        this.CommandData.StartCorrectionFrameNumber = (ushort)this.StartCorrectionFrameNumber.Value;
        this.CommandData.EndCorrectionFrameNumber = (ushort)this.EndCorrectionFrameNumber.Value;

        this.CommandData.StartInterpolationParameters = this.StartInterpolationSettings.Compose();
        this.CommandData.EndInterpolationParameters = this.EndInterpolationSettings.Compose();

        this.CommandData.ModelObjectId = this.ModelAssetID.Choice;
        this.CommandData.HelperId      = (uint)this.HelperID.Value;
    }
}
