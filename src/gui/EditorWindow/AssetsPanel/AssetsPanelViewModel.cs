using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ReactiveUI;

namespace EVTUI.ViewModels;

public class Asset : ViewModelBase
{

    public Asset(SerialObject obj, bool readOnly)
    {
        this.Editable = !readOnly;

        this.ObjectID = new NumEntryField("Asset ID", false, obj.Id, 0, 9999, 1);
        this.ObjectType = new StringSelectionField("Type", this.Editable, Enum.GetName(typeof(ObjectTypes), obj.Type), new List<string>(Enum.GetNames(typeof(ObjectTypes))));
        this.Category = new StringSelectionField("Category", this.Editable, Enum.GetName(typeof(ObjectCategories), obj.ResourceCategory), new List<string>(Enum.GetNames(typeof(ObjectCategories))));
        this.UniqueID = new NumEntryField("Unique ID", this.Editable, obj.ResourceUniqueId, 0, 9999, 1);
        this.MajorID = new NumEntryField("Major ID", this.Editable, obj.ResourceMajorId, 0, 9999, 1);
        this.MinorID = new NumEntryField("Minor ID", this.Editable, obj.ResourceMinorId, 0, 255, 1);
        this.SubID = new NumEntryField("Sub ID", this.Editable, obj.ResourceSubId, 0, 255, 1);
        this.BaseAnimID = new NumEntryField("Base Animations ID", this.Editable, obj.BaseMotionNo, -1, 9999, 1);
        this.ExtBaseAnimID = new NumEntryField("Extra Base Animations ID", this.Editable, obj.ExtBaseMotionNo, -1, 9999, 1);
        this.ExtAddAnimID = new NumEntryField("Extra Additive Animations ID", this.Editable, obj.ExtAddMotionNo, -1, 9999, 1);
        this.IsCommon = new BoolChoiceField("Common?", this.Editable, ((obj.Flags & 1) == 1));
        this.UnkFlag1 = new BoolChoiceField("Unknown Flag #1", this.Editable, (((obj.Flags >> 31) & 1) == 1));
        this.UnkFlag2 = new BoolChoiceField("Unknown Flag #2", this.Editable, (obj.UnkBool != 0));
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

    private enum ObjectTypes : int
    {
        Null             = 0x00000000,
        Field            = 0x00000003,
        Env              = 0x00000004,
        Texture          = 0x00000005,
        Sprite           = 0x00000006,
        Camera           = 0x00000007,
        Movie            = 0x00000008,
        EventCamera      = 0x00000009,
        Enemy            = 0x00000301,
        SymShadow        = 0x00000401,
        Item             = 0x00000601,
        ResourceTableNPC = 0x00020101,
        Effect           = 0x01000002,
        Character        = 0x01000101,
        FieldCharacter   = 0x02000101,
        FieldObject      = 0x02000701,
        Persona          = 0x04000201,
    }

    private enum ObjectCategories : int
    {
        Null   = 0,
        Event  = 1,
        Field  = 2,
        Battle = 3,
        Script = 4,
        Other  = 5,
    }

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
