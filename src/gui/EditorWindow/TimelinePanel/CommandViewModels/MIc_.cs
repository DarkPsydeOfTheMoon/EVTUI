using System;
using System.Collections.Generic;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class MIc_ : Generic
{
    public MIc_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Icon (Emote)";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.IconType = new StringSelectionField("Type", this.Editable, this.IconTypes.Backward[this.CommandData.IconType], this.IconTypes.Keys);
        this.IconSize = new StringSelectionField("Scale", this.Editable, this.IconSizes.Backward[this.CommandData.IconSize], this.IconSizes.Keys);
    }

    public IntSelectionField    AssetID  { get; set; }
    public StringSelectionField IconType { get; set; }
    public StringSelectionField IconSize { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        this.CommandData.IconType = this.IconTypes.Forward[this.IconType.Choice];
        this.CommandData.IconSize = this.IconSizes.Forward[this.IconSize.Choice];
    }

    public BiDict<string, uint> IconTypes = new BiDict<string, uint>
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

    public BiDict<string, uint> IconSizes = new BiDict<string, uint>
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
