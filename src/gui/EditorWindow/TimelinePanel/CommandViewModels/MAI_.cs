using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class MAI_ : Generic
{
    public MAI_(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Model: Idle Animation";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);

        this.Animations = new ObservableCollection<AnimationWidget>();
        for (int i=0; i<10; i++)
            this.Animations.Add(new AnimationWidget(config, this.AssetID, this.CommandData.IdleAnimations[i], this.CommandData.IdleAnimationBitfields[i], $"Animation #{i+1}", enabledInd:(int?)((i == 0) ? null : 0), extInd:1, frameBlendingInd:(int?)((i == 0) ? null : 2)));
    }

    public IntSelectionField AssetID { get; set; }

    public ObservableCollection<AnimationWidget> Animations { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        //foreach (AnimationWidget animation in this.Animations)
        //    animation.SaveChanges();
    }
}
