using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using ReactiveUI;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels;

public class Asset : ViewModelBase
{

    public Asset(SerialObject obj, bool readOnly)
    {
        this.Obj = obj;
        this.Editable = !readOnly;

        this.ObjectID = new NumEntryField("Asset ID", false, obj.Id, 0, 9999, 1);
        this.ObjectType = new StringSelectionField("Type", false, Asset.ObjectTypes.Backward[obj.Type], Asset.ObjectTypes.Keys);

        this.UniqueID = new NumEntryField("Unique ID", this.Editable, obj.ResourceUniqueId, 0, 9999, 1);
        this.UnkFlag1 = new BoolChoiceField("Unknown Flag #1", this.Editable, obj.Flags[31]);
        this.UnkFlag2 = new BoolChoiceField("Unknown Flag #2", this.Editable, (obj.UnkBool != 0));

        string categoryTitle = null;
        string majorIdTitle = null;
        string minorIdTitle = null;
        string subIdTitle = null;
        string isCommonTitle = null;
        string extId1Title = null;
        string extId2Title = null;
        string extId3Title = null;

        switch (this.ObjectType.Choice)
        {
            case "Null":
                break;
            case "Field":
                categoryTitle = "Category (Unused)";
                majorIdTitle  = "Major ID";
                minorIdTitle  = "Minor ID";
                subIdTitle    = "Sub ID";
                isCommonTitle = "Use Field's Generic Resources?";
                extId1Title   = "Player Field Animations ID";
                // TODO: if isCommon is false, these should be hidden
                extId2Title   = "Crowd Major ID";
                extId3Title   = "Crowd Minor ID";
                break;
            case "Env":
                categoryTitle = "Category (Unused)";
                majorIdTitle  = "Major ID";
                minorIdTitle  = "Minor ID";
                subIdTitle    = "Sub ID";
                isCommonTitle = "Use Non-Field-Specific Env?";
                extId1Title   = "Unused #1";
                extId2Title   = "Unused #2";
                extId3Title   = "Unused #3";
                break;
            case "Texture":
                categoryTitle = "Category (Unused)";
                majorIdTitle  = "Major ID";
                minorIdTitle  = "Minor ID";
                subIdTitle    = "Sub ID";
                isCommonTitle = "Unused #1";
                extId1Title   = "Unused #2";
                extId2Title   = "Unused #3";
                extId3Title   = "Unused #4";
                break;
            case "Sprite":
                break;
            case "Camera":
                categoryTitle = "Category (Unused)";
                majorIdTitle  = "Major ID";
                minorIdTitle  = "Minor ID";
                subIdTitle    = "Sub ID";
                isCommonTitle = "Use Default Field Camera?";
                extId1Title   = "Unused #1";
                extId2Title   = "Unused #2";
                extId3Title   = "Unused #3";
                break;
            case "Movie":
                categoryTitle = "Category (Unused)";
                majorIdTitle  = "Movie ID";
                minorIdTitle  = "Unused #1";
                subIdTitle    = "Unused #2";
                isCommonTitle = "Unknown #1";
                extId1Title   = "Unknown #2";
                extId2Title   = "Unused #3";
                extId3Title   = "Unused #4";
                break;
            case "EventCamera":
                categoryTitle = "Category (Unused)";
                majorIdTitle  = "Major ID";
                minorIdTitle  = "Minor ID";
                subIdTitle    = "Sub ID";
                isCommonTitle = "Unused #1";
                extId1Title   = "Unused #2";
                extId2Title   = "Unused #3";
                extId3Title   = "Unused #4";
                break;
            case "Enemy":
                categoryTitle = "Category (Unused)";
                majorIdTitle  = "Enemy ID";
                minorIdTitle  = "Unused #1";
                subIdTitle    = "Unused #2";
                // hm, doesn't actually get used... but could it? i mean, why not?
                isCommonTitle = "Use Common Animations?";
                extId1Title   = "Base Animations ID";
                extId2Title   = "Unused #3";
                extId3Title   = "Unused #4";
                break;
            case "SymShadow":
                categoryTitle = "Category (Unused)";
                majorIdTitle  = "Overworld Shadow ID";
                minorIdTitle  = "Unused #1";
                subIdTitle    = "Unused #2";
                isCommonTitle = "Unused #3";
                extId1Title   = "Unused #4";
                extId2Title   = "Unused #5";
                extId3Title   = "Unused #6";
                break;
            case "Item":
                categoryTitle = "Category (Unused)";
                majorIdTitle  = "Major ID";
                minorIdTitle  = "Minor ID";
                // subId, isCommon, extId1, & extId2 CAN vary, but...
                // ...it happens so rarely... what does it meannn
                // this gets used in e.g. the jazz cloughb, but like... i'm not convinced
                // (that any anims actually come from it)
                //extId1Title   = "Base Animations ID";
                subIdTitle    = "Possibly Unused #1";
                isCommonTitle = "Possibly Unused #2";
                extId1Title   = "Possibly Unused #3";
                extId2Title   = "Possibly Unused #4";
                extId3Title   = "Unused #1";
                break;
            case "ResourceTableNPC":
                categoryTitle = "Category (Unused)";
                majorIdTitle = "Resource Table Entry Number";
                minorIdTitle  = "Unused #1";
                subIdTitle    = "Unused #2";
                // minorId, isCommon, & extId1 CAN vary, but... rarely...
                isCommonTitle = "Possibly Unused #1";
                // used only in beta
                extId1Title   = "Possibly Unused #2";
                // used only in royal
                extId2Title   = "Possibly Unused #3";
                extId3Title   = "Unused #3";
                break;
            case "Effect":
                categoryTitle = "Category (Unused)";
                majorIdTitle = "Effect Major ID";
                minorIdTitle = "Effect Minor ID";
                // subId varies, but i just don't know what it could accomplish
                subIdTitle    = "Possibly Unused #1";
                // isCommon CAN happen... uses field EPL, maybe? or something with the PAKs?
                // .....doesn't seem to be......
                isCommonTitle = "Possibly Unused #2";
                // baseAnim is 0 once......
                // but howwww.
                extId1Title   = "Possibly Unused #3";
                extId2Title   = "Unused #1";
                extId3Title   = "Unused #2";
                break;
            case "Character":
                categoryTitle = "Category";
                majorIdTitle  = "Character ID";
                minorIdTitle  = "Outfit ID";
                subIdTitle    = "Sub ID";
                isCommonTitle = "Use Common Animations?";
                extId1Title   = "Base Animations ID";
                extId2Title   = "Extra Base Animations ID";
                extId3Title   = "Extra Additive Animations ID";
                break;
            case "FieldCharacter":
                categoryTitle = "Category (Unused)";
                // why does this need to exist when there's a Field "Category"??? idfk man
                majorIdTitle  = "Character ID";
                minorIdTitle  = "Outfit ID";
                subIdTitle    = "Sub ID";
                isCommonTitle = "Use Common Animations?";
                extId1Title   = "Base Animations ID";
                extId2Title   = "Extra Base Animations ID";
                // this doesn't get used but i think it'd work like Character anims, anyway
                extId3Title   = "Extra Additive Animations ID";
                break;
            case "FieldObject":
                categoryTitle = "Category (Unused)";
                majorIdTitle  = "Major ID";
                minorIdTitle  = "Minor ID";
                subIdTitle    = "Unused #1";
                isCommonTitle = "Unused #2";
                extId1Title   = "Unused #3";
                extId2Title   = "Unused #4";
                extId3Title   = "Unused #5";
                break;
            case "Persona":
                // only varies in beta P5
                //categoryTitle = "Category";
                categoryTitle = "Category (Unused)";
                majorIdTitle  = "Persona ID";
                //minorId can be 1 in royal but what does that mean...
                minorIdTitle  = "Possibly Unused #1";
                subIdTitle    = "Unused #1";
                isCommonTitle = "Unused #2";
                extId1Title   = "Base Animations ID";
                extId2Title   = "Unused #3";
                extId3Title   = "Unused #4";
                break;
            default:
                break;
        }

        if (!(categoryTitle is null))
            this.Category = new StringSelectionField(categoryTitle, this.Editable, this.ObjectCategories.Backward[obj.ResourceCategory], this.ObjectCategories.Keys);

        if (!(majorIdTitle is null))
            this.MajorID = new NumEntryField(majorIdTitle, this.Editable, obj.ResourceMajorId, 0, 9999, 1);

        if (!(minorIdTitle is null))
            this.MinorID = new NumEntryField(minorIdTitle, this.Editable, obj.ResourceMinorId, 0, 255, 1);

        if (!(subIdTitle is null))
            this.SubID = new NumEntryField(subIdTitle, this.Editable, obj.ResourceSubId, 0, 255, 1);

        if (!(isCommonTitle is null))
            this.IsCommon = new BoolChoiceField(isCommonTitle, this.Editable, obj.Flags[0]);

        if (!(extId1Title is null))
            this.BaseAnimID = new NumEntryField(extId1Title, this.Editable, obj.BaseMotionNo, -1, 9999, 1);

        if (!(extId2Title is null))
            this.ExtBaseAnimID = new NumEntryField(extId2Title, this.Editable, obj.ExtBaseMotionNo, -1, 9999, 1);

        if (!(extId3Title is null))
            this.ExtAddAnimID = new NumEntryField(extId3Title, this.Editable, obj.ExtAddMotionNo, -1, 9999, 1);

        // TODO: add this in only for applicable asset types
        // i don't feel like doing it now lol
        //this.BaseAnimPreview = new AnimationWidget(config, new IntSelectionField("Asset ID", false, obj.Id, new List<int>(){obj.Id}), new AnimationStruct(startingFrame:null, interpolatedFrames:null), new Bitfield(1), null, enabledInd:0, extInd:1);

    }

    public bool Editable { get; set; }

    public NumEntryField        ObjectID      { get; set; }
    public StringSelectionField ObjectType    { get; set; }
    public StringSelectionField Category      { get; set; }
    public NumEntryField        UniqueID      { get; set; }
    public NumEntryField        MajorID       { get; set; }
    public NumEntryField        MinorID       { get; set; }
    public NumEntryField        SubID         { get; set; }
    public NumEntryField        BaseAnimID    { get; set; }
    public NumEntryField        ExtBaseAnimID { get; set; }
    public NumEntryField        ExtAddAnimID  { get; set; }
    public BoolChoiceField      IsCommon      { get; set; }
    public BoolChoiceField      UnkFlag1      { get; set; }
    public BoolChoiceField      UnkFlag2      { get; set; }

    public bool IncludeSubordinateResources { get => !(this.BaseAnimID is null) || !(this.ExtBaseAnimID is null) || !(this.ExtAddAnimID is null) || !(this.IsCommon is null); }

    //public AnimationWidget BaseAnimPreview { get; set; }

    public SerialObject Obj { get; set; }

    public void SaveChanges()
    {
        this.Obj.Id = (int)this.ObjectID.Value;

        this.Obj.Type = Asset.ObjectTypes.Forward[this.ObjectType.Choice];
        this.Obj.ResourceUniqueId = (int)this.UniqueID.Value;

        if (!(this.Category is null))
            this.Obj.ResourceCategory = this.ObjectCategories.Forward[this.Category.Choice];

        if (!(this.MajorID is null))
            this.Obj.ResourceMajorId = (int)this.MajorID.Value;

        if (!(this.MinorID is null))
            this.Obj.ResourceMinorId = (short)this.MinorID.Value;

        if (!(this.SubID is null))
            this.Obj.ResourceSubId = (short)this.SubID.Value;

        if (!(this.BaseAnimID is null))
            this.Obj.BaseMotionNo = (int)this.BaseAnimID.Value;

        if (!(this.ExtBaseAnimID is null))
            this.Obj.ExtBaseMotionNo = (int)this.ExtBaseAnimID.Value;

        if (!(this.ExtAddAnimID is null))
            this.Obj.ExtAddMotionNo = (int)this.ExtAddAnimID.Value;
 
        if (!(this.IsCommon is null))
            this.Obj.Flags[0] = this.IsCommon.Value;

        this.Obj.Flags[31] = this.UnkFlag1.Value;
        this.Obj.UnkBool = Convert.ToInt32(this.UnkFlag2.Value);
    }

    public static BiDict<string, int> ObjectTypes = new BiDict<string, int>
    (
        new Dictionary<string, int>
        {
            {"Null",             0x00000000},
            {"Field",            0x00000003},
            {"Env",              0x00000004},
            {"Texture",          0x00000005},
            {"Sprite",           0x00000006},
            {"Camera",           0x00000007},
            {"Movie",            0x00000008},
            {"EventCamera",      0x00000009},
            {"Enemy",            0x00000301},
            {"SymShadow",        0x00000401},
            {"Item",             0x00000601},
            {"ResourceTableNPC", 0x00020101},
            {"Effect",           0x01000002},
            {"Character",        0x01000101},
            {"FieldCharacter",   0x02000101},
            {"FieldObject",      0x02000701},
            {"Persona",          0x04000201},
        }
    );

    public BiDict<string, int> ObjectCategories = new BiDict<string, int>
    (
        new Dictionary<string, int>
        {
            {"Null",   0},
            {"Event",  1},
            {"Field",  2},
            {"Battle", 3},
            {"Script", 4},
            {"Other",  5},
        }
    );

}

public class AssetsPanelViewModel : ViewModelBase
{

    //////////////////////////////
    // *** REACTIVE MEMBERS *** //
    //////////////////////////////
    private (bool IsById, bool IsAscending) _sortMode = (true, true);
    public (bool IsById, bool IsAscending) SortMode
    {
        get => _sortMode;
        set
        {
            if (_sortMode != value)
            {
                _sortMode = value;
                this.SortAssets();
            }
        }
    }

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public DataManager Config { get; }

    public ObservableCollection<Asset> Assets { get; set; }

    public List<string> AddableTypes { get => Asset.ObjectTypes.Keys.ToList(); }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public AssetsPanelViewModel(DataManager dataManager)
    {
        this.Config = dataManager;

        this.Assets = new ObservableCollection<Asset>();
        EVT evt = (EVT)dataManager.EventManager.SerialEvent;
        foreach (SerialObject obj in evt.Objects)
            this.Assets.Add(new Asset(obj, this.Config.ReadOnly));
        this.SortAssets();
    }

    public void SortAssets()
    {
        if (this.SortMode.IsAscending)
            if (this.SortMode.IsById)
                this.Assets = new ObservableCollection<Asset>(this.Assets.OrderBy(a => a.ObjectID.Value));
            else
                this.Assets = new ObservableCollection<Asset>(this.Assets.OrderBy(a => a.ObjectType.Choice));
        else
            if (this.SortMode.IsById)
                this.Assets = new ObservableCollection<Asset>(this.Assets.OrderByDescending(a => a.ObjectID.Value));
            else
                this.Assets = new ObservableCollection<Asset>(this.Assets.OrderByDescending(a => a.ObjectType.Choice));
        OnPropertyChanged(nameof(Assets));
    }

    public void AddAsset(string type)
    {
        SerialObject newObj = this.Config.EventManager.SerialEvent.NewObject(Asset.ObjectTypes.Forward[type]);
        this.Assets.Add(new Asset(newObj, this.Config.ReadOnly));
        this.SortAssets();
    }

    public void DuplicateAsset(Asset asset)
    {
        SerialObject newObj = this.Config.EventManager.SerialEvent.DuplicateObject(asset.Obj);
        this.Assets.Add(new Asset(newObj, this.Config.ReadOnly));
        this.SortAssets();
    }

    public bool DeleteAsset(Asset asset)
    {
        bool success = this.Config.EventManager.SerialEvent.DeleteObject(asset.Obj);
        asset.Obj = null;
        foreach (Asset candidate in this.Assets)
            if (candidate == asset)
            {
                success = this.Assets.Remove(candidate);
                break;
            }
        OnPropertyChanged(nameof(Assets));
        return success;
    }

}
