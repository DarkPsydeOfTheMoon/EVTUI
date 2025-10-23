using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class MAAB : Generic
{
    public MAAB(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Model: Attachment Animation";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDs);

        this.ChildAssetID = new IntSelectionField("Attached Asset ID", this.Editable, this.CommandData.ChildObjectId, config.EventManager.AssetIDs);

        // animations
        this.FirstAnimation = new AnimationWidget(config, this.ChildAssetID, this.CommandData.FirstAnimation, this.CommandData.Flags, $"First Animation", extInd:4);
        this.SecondAnimation = new AnimationWidget(config, this.ChildAssetID, this.CommandData.SecondAnimation, this.CommandData.Flags, $"Second Animation", enabledInd:0, extInd:5, frameBlendingInd:1, enabledFlip:true);
    }

    public IntSelectionField AssetID      { get; set; }

    public IntSelectionField ChildAssetID { get; set; }

    // animations
    public AnimationWidget FirstAnimation  { get; set; }
    public AnimationWidget SecondAnimation { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.Command.ObjectId = this.AssetID.Choice;

        this.CommandData.ChildObjectId = this.ChildAssetID.Choice;

        //this.FirstAnimation.SaveChanges();
        //this.SecondAnimation.SaveChanges();
    }
}
