using System;

using ReactiveUI;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class MFts : Generic
{
    public MFts(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Model: Footsteps";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.Enable = new BoolChoiceField("Show footsteps?", this.Editable, !this.CommandData.Flags[0]);
        this.WhenAnyValue(_ => _.Enable.Value).Subscribe(_ => this.CommandData.Flags[0] = !this.Enable.Value);
        this.AreaDistortion = new BoolChoiceField("Add area distortion effect?", this.Editable, this.CommandData.Flags[1]);
        this.WhenAnyValue(_ => _.AreaDistortion.Value).Subscribe(_ => this.CommandData.Flags[1] = this.AreaDistortion.Value);
        this.PuddleEffect = new BoolChoiceField("Add puddle effect?", this.Editable, this.CommandData.Flags[2]);
        this.WhenAnyValue(_ => _.PuddleEffect.Value).Subscribe(_ => this.CommandData.Flags[2] = this.PuddleEffect.Value);
    }

    public IntSelectionField AssetID { get; set; }

    public BoolChoiceField Enable         { get; set; }
    public BoolChoiceField AreaDistortion { get; set; }
    public BoolChoiceField PuddleEffect   { get; set; }
}
