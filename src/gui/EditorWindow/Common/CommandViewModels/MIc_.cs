using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.Utils;

namespace EVTUI.ViewModels.TimelineCommands;

public class MIc_ : Generic
{
    public MIc_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Model: Icon (Emote)";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.IconType = new StringSelectionField("Type", this.Editable, MIc_.IconTypes.Backward[this.CommandData.IconType], MIc_.IconTypes.Keys);
        this.WhenAnyValue(_ => _.IconType.Choice).Subscribe(_ => this.CommandData.IconType = MIc_.IconTypes.Forward[this.IconType.Choice]);
        this.IconSize = new StringSelectionField("Scale", this.Editable, MIc_.IconSizes.Backward[this.CommandData.IconSize], MIc_.IconSizes.Keys);
        this.WhenAnyValue(_ => _.IconSize.Choice).Subscribe(_ => this.CommandData.IconSize = MIc_.IconSizes.Forward[this.IconSize.Choice]);
    }

    public IntSelectionField    AssetID  { get; set; }
    public StringSelectionField IconType { get; set; }
    public StringSelectionField IconSize { get; set; }

    public static BiDict<string, uint> IconTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"None",             0},
            {"!",                1},
            {"!!",               2},
            {"?",                3},
            {"!?",               4},
            {"Bolts (v1)",       9},
            {"Bolts (v2)",      10},
            {"Talking",         11},
            {"Talking (x3)",    12},
            {"ZZZ",             14},
            {"Pi-Pi-Pi",        15},
            {"Sweatdrop",       16},
            {"Flustered Sweat", 17},
            {"Meow (Up)",       22},
            {"Meow (Right)",    23},
            {"Meow (Left)",     24},
            {"Sparkle",         25},
            {"Blue Sweat",      26},
        }
    );

    public static BiDict<string, uint> IconSizes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"100%", 0},
            {"70%",  1},
            {"150%", 2},
            {"200%", 3},
        }
    );
}
