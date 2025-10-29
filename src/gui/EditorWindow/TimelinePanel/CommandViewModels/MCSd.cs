using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class MCSd : Generic
{
    public MCSd(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Model: Shadow Color";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.Enabled = new BoolChoiceField("Model shadow enabled?", this.Editable, this.CommandData.Flags[0]);
        this.WhenAnyValue(_ => _.Enabled.Value).Subscribe(_ => this.CommandData.Flags[0] = this.Enabled.Value);

        // inner circle
        this.InnerCircleColor = new ColorSelectionField("Color", this.Editable, this.CommandData.InnerCircleRGBA);
        this.WhenAnyValue(_ => _.InnerCircleColor.SelectedColor).Subscribe(_ => this.CommandData.InnerCircleRGBA = this.InnerCircleColor.ToUInt32());
        this.InnerCircleDiameter = new NumEntryField("Diameter", this.Editable, this.CommandData.InnerCircleDiameter, 0, 99, 1);
        this.WhenAnyValue(_ => _.InnerCircleDiameter.Value).Subscribe(_ => this.CommandData.InnerCircleDiameter = (ushort)this.InnerCircleDiameter.Value);

        // outer circle
        this.OuterCircleColor = new ColorSelectionField("Color", this.Editable, this.CommandData.OuterCircleRGBA);
        this.WhenAnyValue(_ => _.OuterCircleColor.SelectedColor).Subscribe(_ => this.CommandData.OuterCircleRGBA = this.OuterCircleColor.ToUInt32());
        this.OuterCircleDiameter = new NumEntryField("Diameter", this.Editable, this.CommandData.OuterCircleDiameter, 0, 99, 1);
        this.WhenAnyValue(_ => _.OuterCircleDiameter.Value).Subscribe(_ => this.CommandData.OuterCircleDiameter = (ushort)this.OuterCircleDiameter.Value);

        // unknown
        this.UnkBool = new BoolChoiceField("Unknown", this.Editable, this.CommandData.Flags[1]);
        this.WhenAnyValue(_ => _.UnkBool.Value).Subscribe(_ => this.CommandData.Flags[1] = this.UnkBool.Value);
    }

    public IntSelectionField AssetID { get; set; }

    public BoolChoiceField Enabled { get; set; }

    // inner circle
    public ColorSelectionField InnerCircleColor    { get; set; }
    public NumEntryField       InnerCircleDiameter { get; set; }

    // outer circle
    public ColorSelectionField OuterCircleColor    { get; set; }
    public NumEntryField       OuterCircleDiameter { get; set; }

    // unknown
    public BoolChoiceField UnkBool { get; set; }
}
