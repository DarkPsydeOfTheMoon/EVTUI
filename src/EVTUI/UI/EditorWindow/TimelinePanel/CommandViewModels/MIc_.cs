using System;
using System.Collections.Generic;

namespace EVTUI.ViewModels.TimelineCommands;

public class MIc_ : Generic
{
    public MIc_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Model: Icon";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.IconType = new StringSelectionField("Type", this.Editable, Enum.GetName(typeof(IconTypes), this.CommandData.IconType), new List<string>(Enum.GetNames(typeof(IconTypes))));
        this.IconSize = new StringSelectionField("Size", this.Editable, Enum.GetName(typeof(IconSizes), this.CommandData.IconSize), new List<string>(Enum.GetNames(typeof(IconSizes))));
    }

    public IntSelectionField AssetID      { get; set; }
    public StringSelectionField IconType { get; set; }
    public StringSelectionField IconSize { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;
        this.CommandData.IconType = (int)Enum.Parse(typeof(IconTypes), this.IconType.Choice);
        this.CommandData.IconSize = (int)Enum.Parse(typeof(IconSizes), this.IconSize.Choice);
    }

    public enum IconTypes : int
    {
        None          = 0,
        Exclamation   = 1,
        Exclamationx2 = 2,
        Question      = 3,
        Interrobang   = 4,
        Bolts1        = 9,
        Bolts2        = 10,
        Talking       = 11,
        Talkingx3     = 12,
        ZZZ           = 14,
        PiPiPi        = 15,
        Sweatdrop     = 16,
        Fluster       = 17,
        MeowUp        = 22,
        MeowRight     = 23,
        MeowLeft      = 24,
        Sparkle       = 25,
        BlueSweat     = 26,
    }

    public enum IconSizes : int
    {
        _100 = 0,
        _70  = 1,
        _150 = 2,
        _200 = 3,
    }
}
