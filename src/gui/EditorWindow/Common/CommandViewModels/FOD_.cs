using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.Utils;

namespace EVTUI.ViewModels.TimelineCommands;

public class FOD_ : Generic
{
    public FOD_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Field: Object Placement";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDsOfType(0x00000003));
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        // TODO... IDK exactly how resIDs work with subfield models yet
        /*this.UpdateHelperNames(commonVMs, this.AssetID.Choice, isField:true);
        //if (commonVMs.AssetsByID.ContainsKey(this.AssetID.Choice))
        //    this.TexturePath = commonVMs.AssetsByID[this.AssetID.Choice].ActiveTextureBinPath;
        //else
            this.TexturePath = "";*/

        this.ActionType = new StringSelectionField("Mode", this.Editable, FOD_.ActionTypes.Backward[this.CommandData.EnableFieldObject], FOD_.ActionTypes.Keys);
        this.WhenAnyValue(_ => _.ActionType.Choice).Subscribe(_ => this.CommandData.EnableFieldObject = FOD_.ActionTypes.Forward[this.ActionType.Choice]);

        this.ObjectIndex = new NumEntryField("Field Object Index", this.Editable, this.CommandData.ObjectIndex, -1, 65535, 1);
        this.WhenAnyValue(_ => _.ObjectIndex.Value).Subscribe(_ => this.CommandData.ObjectIndex = (int)this.ObjectIndex.Value);
        /*this.ObjectIndex = new StringSelectionField("Field Object Name", this.Editable, (!(this.CommandData.ObjectIndex is null) && this.ResourceHelperNames.Backward.ContainsKey(this.CommandData.ObjectIndex)) ? this.ResourceHelperNames.Backward[this.CommandData.ObjectIndex] : null, this.ResourceHelperNames.Keys);
        this.ModelPreviewVM = new ModelPreviewWidget(config, commonVMs, this.AssetID, this.ObjectIndex, texturePath:this.TexturePath, isField:true);
        this.WhenAnyValue(_ => _.ObjectIndex.Choice).Subscribe(_ =>
        {
            if (!(this.ObjectIndex.Choice is null) && this.ResourceHelperNames.Forward.ContainsKey(this.ObjectIndex.Choice))
                this.CommandData.ObjectIndex = this.ResourceHelperNames.Forward[this.ObjectIndex.Choice];
        });
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ =>
        {
            this.Command.ObjectId = this.AssetID.Choice;
            this.UpdateHelperNames(commonVMs, this.AssetID.Choice, isField:true);
            //if (commonVMs.AssetsByID.ContainsKey(this.AssetID.Choice))
            //    this.TexturePath = commonVMs.AssetsByID[this.AssetID.Choice].ActiveTextureBinPath;
            //else
                this.TexturePath = "";
            string choice = this.ObjectIndex.Choice;
            this.ObjectIndex.Choices.Clear();
            foreach (string helperName in this.ResourceHelperNames.Keys)
                this.ObjectIndex.Choices.Add(helperName);
            // in case the choice got erased by the last step i guess
            if (!(choice is null) && this.ResourceHelperNames.Forward.ContainsKey(choice))
                this.ObjectIndex.Choice = choice;
        });*/
    }

    //private string TexturePath = "";

    public IntSelectionField AssetID { get; set; }

    public StringSelectionField ActionType  { get; set; }
    public NumEntryField        ObjectIndex { get; set; }
    //public StringSelectionField ObjectIndex { get; set; }

    public ModelPreviewWidget ModelPreviewVM { get; set; }

    public static BiDict<string, uint> ActionTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Show", 0},
            {"Hide", 1},
        }
    );

}
