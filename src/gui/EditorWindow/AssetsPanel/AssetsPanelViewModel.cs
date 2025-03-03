using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ReactiveUI;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels;

public class Asset : ViewModelBase
{

    public Asset(SerialObject obj, bool readOnly)
    {
        _obj = obj;
        this.Editable = !readOnly;

        this.ObjectID = new NumEntryField("Asset ID", false, obj.Id, 0, 9999, 1);
        this.ObjectType = new StringSelectionField("Type", false, this.ObjectTypes.Backward[obj.Type], this.ObjectTypes.Keys);

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
                majorIdTitle  = "Major ID";
                minorIdTitle  = "Minor ID";
                subIdTitle    = "Sub ID";
                isCommonTitle = "Use Field's Generic Resources?";
                extId1Title   = "Player Field Animations ID";
                extId2Title   = "Crowd Major ID";
                extId3Title   = "Crowd Minor ID";
                break;
            case "Env":
                categoryTitle = "Category";
                majorIdTitle  = "Major ID";
                minorIdTitle  = "Minor ID";
                subIdTitle    = "Sub ID";
                isCommonTitle = "Use Non-Field-Specific Env?";
                break;
            case "Texture":
                majorIdTitle  = "Major ID";
                minorIdTitle  = "Minor ID";
                subIdTitle    = "Sub ID";
                break;
            case "Sprite":
                break;
            case "Camera":
                majorIdTitle  = "Major ID";
                minorIdTitle  = "Minor ID";
                subIdTitle    = "Sub ID";
                isCommonTitle = "Use Default Field Camera?";
                break;
            case "Movie":
                majorIdTitle  = "Movie ID";
                isCommonTitle = "Unknown #1";
                extId1Title   = "Unknown #2";
                break;
            case "EventCamera":
                majorIdTitle  = "Major ID";
                minorIdTitle  = "Minor ID";
                subIdTitle    = "Sub ID";
                break;
            case "Enemy":
                majorIdTitle  = "Enemy ID";
                isCommonTitle = "Use Common Animations?";
                extId1Title   = "Base Animations ID";
                break;
            case "SymShadow":
                majorIdTitle  = "Overworld Shadow ID";
                break;
            case "Item":
                majorIdTitle  = "Major ID";
                minorIdTitle  = "Minor ID";
                // subId, isCommon, extId1, & extId2 CAN vary, but...
                // ...it happens so rarely and idk what it means so i'm gonna say. eh.
                break;
            case "ResourceTableNPC":
                majorIdTitle = "Resource Table Entry Number";
                // minorId, isCommon, & extId1 CAN vary, but...
                // ...it happens so rarely and idk what it means so i'm gonna say. eh.
                break;
            case "Effect":
                majorIdTitle = "Effect Major ID";
                minorIdTitle = "Effect Minor ID";
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
                majorIdTitle  = "Major ID";
                minorIdTitle  = "Minor ID";
                break;
            case "Persona":
                // only varies in beta P5
                //categoryTitle = "Category";
                majorIdTitle  = "Persona ID";
                extId1Title   = "Base Animations ID";
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

    //public AnimationWidget BaseAnimPreview { get; set; }

    protected SerialObject _obj;

    public void SaveChanges()
    {
        _obj.Id = (int)this.ObjectID.Value;

        _obj.Type = this.ObjectTypes.Forward[this.ObjectType.Choice];
        _obj.ResourceUniqueId = (int)this.UniqueID.Value;

        if (!(this.Category is null))
            _obj.ResourceCategory = this.ObjectCategories.Forward[this.Category.Choice];

        if (!(this.MajorID is null))
            _obj.ResourceMajorId = (int)this.MajorID.Value;

        if (!(this.MinorID is null))
            _obj.ResourceMinorId = (short)this.MinorID.Value;

        if (!(this.SubID is null))
            _obj.ResourceSubId = (short)this.SubID.Value;

        if (!(this.BaseAnimID is null))
            _obj.BaseMotionNo = (int)this.BaseAnimID.Value;

        if (!(this.ExtBaseAnimID is null))
            _obj.ExtBaseMotionNo = (int)this.ExtBaseAnimID.Value;

        if (!(this.ExtAddAnimID is null))
            _obj.ExtAddMotionNo = (int)this.ExtAddAnimID.Value;
 
        if (!(this.IsCommon is null))
            _obj.Flags[0] = this.IsCommon.Value;

        _obj.Flags[31] = this.UnkFlag1.Value;
        _obj.UnkBool = Convert.ToInt32(this.UnkFlag2.Value);
    }

    public BiDict<string, int> ObjectTypes = new BiDict<string, int>
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

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public DataManager Config { get; }

    public ObservableCollection<Asset> Assets { get; set; }

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
    }

}
