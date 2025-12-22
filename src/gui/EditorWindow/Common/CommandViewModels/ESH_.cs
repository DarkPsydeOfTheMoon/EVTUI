using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class ESH_ : Generic
{
    public ESH_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Effect: Placement (Helper)";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDsOfType(0x01000002));
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.AlwaysSynchronize = new BoolChoiceField("Always Synchronize?", this.Editable, this.CommandData.Flags[1]);
        this.WhenAnyValue(_ => _.AlwaysSynchronize.Value).Subscribe(_ => this.CommandData.Flags[1] = this.AlwaysSynchronize.Value);
        this.ModelAssetID = new IntSelectionField("Parent Asset ID", this.Editable, this.CommandData.ModelObjectId, config.EventManager.AssetIDs);

        this.UpdateHelperNames(commonVMs, this.ModelAssetID.Choice);

        this.HelperID = new StringSelectionField("Helper Node", this.Editable, (!(this.CommandData.HelperId is null) && this.HelperNames.Backward.ContainsKey(this.CommandData.HelperId)) ? this.HelperNames.Backward[this.CommandData.HelperId] : null, this.HelperNames.Keys);
        this.WhenAnyValue(_ => _.HelperID.Choice).Subscribe(_ =>
        {
            if (!(this.HelperID.Choice is null) && this.HelperNames.Forward.ContainsKey(this.HelperID.Choice))
                this.CommandData.HelperId = this.HelperNames.Forward[this.HelperID.Choice];
        });
        this.WhenAnyValue(_ => _.ModelAssetID.Choice).Subscribe(_ =>
        {
            this.CommandData.ModelObjectId = this.ModelAssetID.Choice;
            this.UpdateHelperNames(commonVMs, this.ModelAssetID.Choice);
            string choice = this.HelperID.Choice;
            this.HelperID.Choices.Clear();
            foreach (string helperName in this.HelperNames.Keys)
                this.HelperID.Choices.Add(helperName);
            // in case the choice got erased by the last step i guess
            if (!(choice is null) && this.HelperNames.Forward.ContainsKey(choice))
                this.HelperID.Choice = choice;
        });

        this.CorrectionFrameEnabled = new BoolChoiceField("Correction Frames Enabled?", this.Editable, this.CommandData.Flags[0]);
        this.WhenAnyValue(_ => _.CorrectionFrameEnabled.Value).Subscribe(_ => this.CommandData.Flags[0] = this.CorrectionFrameEnabled.Value);
        this.StartCorrectionFrameNumber = new NumRangeField("Frame Count", this.Editable, this.CommandData.StartCorrectionFrameNumber, 0, 60, 1);
        this.WhenAnyValue(_ => _.StartCorrectionFrameNumber.Value).Subscribe(_ => this.CommandData.StartCorrectionFrameNumber = (ushort)this.StartCorrectionFrameNumber.Value);
        this.StartInterpolationSettings = new InterpolationParameters(this.CommandData.StartInterpolationParameters, this.Editable);
        this.WhenAnyValue(_ => _.StartInterpolationSettings.InterpolationType.Choice, _ => _.StartInterpolationSettings.SlopeInType.Choice, _ => _.StartInterpolationSettings.SlopeOutType.Choice).Subscribe(_ => this.CommandData.StartInterpolationParameters = this.StartInterpolationSettings.Compose());
        this.EndCorrectionFrameNumber = new NumRangeField("Frame Count", this.Editable, this.CommandData.EndCorrectionFrameNumber, 0, 60, 1);
        this.WhenAnyValue(_ => _.EndCorrectionFrameNumber.Value).Subscribe(_ => this.CommandData.EndCorrectionFrameNumber = (ushort)this.EndCorrectionFrameNumber.Value);
        this.EndInterpolationSettings = new InterpolationParameters(this.CommandData.EndInterpolationParameters, this.Editable);
        this.WhenAnyValue(_ => _.EndInterpolationSettings.InterpolationType.Choice, _ => _.EndInterpolationSettings.SlopeInType.Choice, _ => _.EndInterpolationSettings.SlopeOutType.Choice).Subscribe(_ => this.CommandData.EndInterpolationParameters = this.EndInterpolationSettings.Compose());

        this.ParentModelPreviewVM = new ModelPreviewWidget(config, commonVMs, this.ModelAssetID);
    }

    public ModelPreviewWidget ParentModelPreviewVM { get; set; }

    public IntSelectionField AssetID { get; set; }

    public BoolChoiceField      AlwaysSynchronize { get; set; }
    public IntSelectionField    ModelAssetID      { get; set; }
    public StringSelectionField HelperID          { get; set; }

    public BoolChoiceField         CorrectionFrameEnabled     { get; set; }
    public NumRangeField           StartCorrectionFrameNumber { get; set; }
    public InterpolationParameters StartInterpolationSettings { get; set; }
    public NumRangeField           EndCorrectionFrameNumber   { get; set; }
    public InterpolationParameters EndInterpolationSettings   { get; set; }
}
