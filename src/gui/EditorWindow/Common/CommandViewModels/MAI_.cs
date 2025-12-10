using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ReactiveUI;

using static EVTUI.Utils;

namespace EVTUI.ViewModels.TimelineCommands;

public class MAI_ : Generic
{
    public MAI_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Model: Idle Animation";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.Animations = new ObservableCollection<AnimationWidget>();
        for (int i=0; i<10; i++)
            this.Animations.Add(new AnimationWidget(config, this.AssetID, this.CommandData.IdleAnimations[i], this.CommandData.IdleAnimationBitfields[i], $"Animation #{i+1}", enabledInd:(int?)((i == 0) ? null : 0), extInd:1, frameBlendingInd:(int?)((i == 0) ? null : 2)));
    }

    public IntSelectionField AssetID { get; set; }

    public ObservableCollection<AnimationWidget> Animations { get; set; }
}
