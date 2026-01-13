using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using GFDLibrary;
using GFDLibrary.Models;

using ReactiveUI;

using static EVTUI.Utils;

namespace EVTUI.ViewModels;

public class AssetViewModel : ViewModelBase
{
    private List<IDisposable> subscriptions;

    public AssetViewModel(DataManager config, SerialObject obj)
    {
        this.subscriptions = new List<IDisposable>();

        this.Config = config;
        this.Obj = obj;
        this.Editable = !this.Config.ReadOnly;

        this.ObjectID = new NumEntryField("Asset ID", false, obj.Id, 0, 9999, 1);
        this.subscriptions.Add(this.WhenAnyValue(x => x.ObjectID.Value).Subscribe(x => this.Obj.Id = (int)this.ObjectID.Value));
        this.ObjectType = new StringSelectionField("Type", false, AssetViewModel.ObjectTypes.Backward[obj.Type], AssetViewModel.ObjectTypes.Keys);
        this.subscriptions.Add(this.WhenAnyValue(x => x.ObjectType.Choice).Subscribe(x => this.Obj.Type = AssetViewModel.ObjectTypes.Forward[this.ObjectType.Choice]));

        this.UniqueID = new NumEntryField("Unique ID", this.Editable, obj.ResourceUniqueId, 0, 9999, 1);
        this.subscriptions.Add(this.WhenAnyValue(x => x.UniqueID.Value).Subscribe(x => this.Obj.ResourceUniqueId = (int)this.UniqueID.Value));
        this.UnkFlag1 = new BoolChoiceField("Unknown Flag #1", this.Editable, obj.Flags[31]);
        this.subscriptions.Add(this.WhenAnyValue(x => x.UnkFlag1.Value).Subscribe(x => this.Obj.Flags[31] = this.UnkFlag1.Value));
        this.UnkFlag2 = new BoolChoiceField("Unknown Flag #2", this.Editable, (obj.UnkBool != 0));
        this.subscriptions.Add(this.WhenAnyValue(x => x.UnkFlag2).Subscribe(x => this.Obj.UnkBool = Convert.ToInt32(this.UnkFlag2.Value)));

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
        {
            this.Category = new StringSelectionField(categoryTitle, this.Editable, AssetViewModel.ObjectCategories.Backward[obj.ResourceCategory], AssetViewModel.ObjectCategories.Keys);
            this.subscriptions.Add(this.WhenAnyValue(x => x.Category.Choice).Subscribe(x => this.Obj.ResourceCategory = AssetViewModel.ObjectCategories.Forward[this.Category.Choice]));
        }

        if (!(majorIdTitle is null))
        {
            this.MajorID = new NumEntryField(majorIdTitle, this.Editable, obj.ResourceMajorId, 0, 9999, 1);
            this.subscriptions.Add(this.WhenAnyValue(x => x.MajorID.Value).Subscribe(x => 
            {
                // i don't understand why this is necessary but it is! whatever!!!
                if (this.Obj.ResourceMajorId != this.MajorID.Value)
                {
                    this.Obj.ResourceMajorId = (int)this.MajorID.Value;
                    this.UpdatePaths();
                    this.UpdateActiveModelCache();
                }
            }));
        }

        if (!(minorIdTitle is null))
        {
            this.MinorID = new NumEntryField(minorIdTitle, this.Editable, obj.ResourceMinorId, 0, 255, 1);
            this.subscriptions.Add(this.WhenAnyValue(x => x.MinorID.Value).Subscribe(x => 
            {
                if (this.Obj.ResourceMinorId != this.MinorID.Value)
                {
                    this.Obj.ResourceMinorId = (short)this.MinorID.Value;
                    this.UpdatePaths();
                    this.UpdateActiveModelCache();
                }
            }));
        }

        if (!(subIdTitle is null))
        {
            this.SubID = new NumEntryField(subIdTitle, this.Editable, obj.ResourceSubId, 0, 255, 1);
            this.subscriptions.Add(this.WhenAnyValue(x => x.SubID.Value).Subscribe(x => 
            {
                if (this.Obj.ResourceSubId != this.SubID.Value)
                {
                    this.Obj.ResourceSubId = (short)this.SubID.Value;
                    this.UpdatePaths();
                    this.UpdateActiveModelCache();
                }
            }));
        }

        if (!(isCommonTitle is null))
        {
            this.IsCommon = new BoolChoiceField(isCommonTitle, this.Editable, obj.Flags[0]);
            this.subscriptions.Add(this.WhenAnyValue(x => x.IsCommon.Value).Subscribe(x => 
            {
                if (this.Obj.Flags[0] != this.IsCommon.Value)
                {
                    this.Obj.Flags[0] = this.IsCommon.Value;
                    this.UpdateAllAnimPaths();
                    this.UpdateActiveModelCache();
                }
            }));
        }

        if (!(extId1Title is null))
        {
            this.BaseAnimID = new NumEntryField(extId1Title, this.Editable, obj.BaseMotionNo, -1, 9999, 1);
            this.subscriptions.Add(this.WhenAnyValue(x => x.BaseAnimID.Value).Subscribe(x => 
            {
                if (this.Obj.BaseMotionNo != this.BaseAnimID.Value)
                {
                    this.Obj.BaseMotionNo = (int)this.BaseAnimID.Value;
                    this.UpdateAllAnimPaths();
                    this.UpdateActiveModelCache();
                }
            }));
        }

        if (!(extId2Title is null))
        {
            this.ExtBaseAnimID = new NumEntryField(extId2Title, this.Editable, obj.ExtBaseMotionNo, -1, 9999, 1);
            this.subscriptions.Add(this.WhenAnyValue(x => x.ExtBaseAnimID.Value).Subscribe(x => 
            {
                if (this.Obj.ExtBaseMotionNo != this.ExtBaseAnimID.Value)
                {
                    this.Obj.ExtBaseMotionNo = (int)this.ExtBaseAnimID.Value;
                    this.UpdateAllAnimPaths();
                    this.UpdateActiveModelCache();
                }
            }));
        }

        if (!(extId3Title is null))
        {
            this.ExtAddAnimID = new NumEntryField(extId3Title, this.Editable, obj.ExtAddMotionNo, -1, 9999, 1);
            this.subscriptions.Add(this.WhenAnyValue(x => x.ExtAddAnimID.Value).Subscribe(x => 
            {
                if (this.Obj.ExtAddMotionNo != this.ExtAddAnimID.Value)
                {
                    this.Obj.ExtAddMotionNo = (int)this.ExtAddAnimID.Value;
                    this.UpdateAllAnimPaths();
                    this.UpdateActiveModelCache();
                }
            }));
        }

        this.ActiveModels = new Dictionary<string, ModelPack>();
        this.BaseAnimPaths = new ObservableCollection<string>();
        this.ExtBaseAnimPaths = new ObservableCollection<string>();
        this.AddAnimPaths = new ObservableCollection<string>();
        this.ExtAddAnimPaths = new ObservableCollection<string>();

        this.SubModelPaths = new Dictionary<int, ObservableCollection<string>>();
        this.TextureBinPaths = new Dictionary<int, ObservableCollection<string>>();
        this.ActiveSubModelPaths = new Dictionary<int, string>();
        this.ActiveTextureBinPaths = new Dictionary<int, string>();
        this.ActiveAttachmentPaths = new Dictionary<int, Dictionary<int, string>>();

        this.UpdatePaths();

        // TODO: add this in only for applicable asset types
        // i don't feel like doing it now lol
        //this.BaseAnimPreview = new AnimationWidget(config, new IntSelectionField("Asset ID", false, obj.Id, new List<int>(){obj.Id}), new AnimationStruct(startingFrame:null, interpolatedFrames:null), new Bitfield(1), null, enabledInd:0, extInd:1);

    }

    protected DataManager Config;

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

    public bool IsModel { get => AssetViewModel.modelTypes.Contains(this.ObjectType.Choice); }

    protected static HashSet<string> modelTypes = new HashSet<string>() {"Field", "Enemy", "SymShadow", "Item", "ResourceTableNPC", "Character", "FieldCharacter", "FieldObject", "Persona"};

    public ObservableCollection<string> ModelPaths;
    public ObservableCollection<string> BaseAnimPaths;
    public ObservableCollection<string> ExtBaseAnimPaths;
    public ObservableCollection<string> AddAnimPaths;
    public ObservableCollection<string> ExtAddAnimPaths;

    public string ActiveModelPath;
    public string ActiveBaseAnimPath;
    public string ActiveExtBaseAnimPath;
    public string ActiveAddAnimPath;
    public string ActiveExtAddAnimPath;

    // fields only... i cba to make a separate class for them. yet.
    //public ObservableCollection<string> MapPaths;
    public Dictionary<int, ObservableCollection<string>> SubModelPaths;
    public Dictionary<int, ObservableCollection<string>> TextureBinPaths;
    public MAP ActiveMap;
    public Dictionary<int, string> ActiveSubModelPaths;
    public Dictionary<int, string> ActiveTextureBinPaths;
    public Dictionary<int, Dictionary<int, string>> ActiveAttachmentPaths;

    public Dictionary<string, ModelPack> ActiveModels;
    //public AnimationWidget BaseAnimPreview { get; set; }

    public SerialObject Obj { get; set; }

    public void Dispose()
    {
        foreach (IDisposable subscription in this.subscriptions)
            subscription.Dispose();
        this.subscriptions.Clear();
        this.ActiveModels.Clear();
        this.Config = null;
        this.Obj = null;
    }

    public void UpdatePaths()
    {
        //this.ActiveModels.Clear();
        this.UpdateModelPaths();
        if (this.ModelPaths.Count > 0)
        {
            if (this.ActiveModelPath != this.ModelPaths[0])
            {
                this.ActiveModelPath = this.ModelPaths[0];
                if (!this.ActiveModels.ContainsKey(this.ActiveModelPath))
                    this.ActiveModels[this.ActiveModelPath] = null;
            }
        }
        else
            this.ActiveModelPath = null;

        this.UpdateAllAnimPaths();

        if (this.ObjectType.Choice == "Field")
            this.UpdateFieldPaths();
    }

    public void UpdateModelPaths()
    {
        this.ModelPaths = new ObservableCollection<string>();
        string[] prefix = null;
        string suffix = null;
        switch (this.ObjectType.Choice)
        {
            case "Character":
            case "FieldCharacter":
                if (this.MinorID.Value == 0)
                {
                   prefix = new string[] { "model", "character", $"{this.MajorID.Value:0000}" };
                   suffix = $"c{this.MajorID.Value:0000}_00[0-9]_{this.SubID.Value:00}\\.gmd";
                }
                else
                    if (this.SubID.Value == 0)
                    {
                        prefix = new string[] { "model", "character", $"{this.MajorID.Value:0000}" };
                        suffix = $"c{this.MajorID.Value:0000}_{this.MinorID.Value:000}_[0-9][0-9]\\.gmd";
                    }
                    else
                        prefix = new string[] { "model", "character", $"{this.MajorID.Value:0000}", $"c{this.MajorID.Value:0000}_{this.MinorID.Value:000}_{this.SubID.Value:00}.gmd" };
                break;
            case "Enemy":
                prefix = new string[] { "model", "character", "enemy", $"{this.MajorID.Value:0000}", $"em{this.MajorID.Value:0000}.gmd" };
                break;
            case "Persona":
                prefix = new string[] { "model", "character", "persona", $"{this.MajorID.Value:0000}", $"ps{this.MajorID.Value:0000}.gmd" };
                break;
            case "Item":
                prefix = new string[] { "model", "item", $"it{this.MajorID.Value:0000}_{this.MinorID.Value:000}.gmd" };
                break;
            case "FieldObject":
                prefix = new string[] { "model", "field_tex", "object", $"m{this.MajorID.Value:000}_{this.MinorID.Value:000}.gmd" };
                break;
            case "SymShadow":
                prefix = new string[] { "model", "character", "enemy", "symbol", $"sym{this.MajorID.Value:000}.gmd" };
                break;
            default:
                Trace.TraceWarning($"Unknown asset type: {this.ObjectType.Choice}");
                break;
        }

        if (!(prefix is null))
            foreach (string path in this.Config.ExtractExactFiles(prefix, suffix))
                this.ModelPaths.Add(path);
    }

    // TODO: fetch prefixes by game
    public void UpdateFieldPaths()
    {
        this.SubModelPaths.Clear();
        this.TextureBinPaths.Clear();
        this.ActiveMap = null;

        string[] mapPrefix = new string[] { "field", "map", $"d{this.MajorID.Value:000}_{this.MinorID.Value:000}.map" };
        List<string> mapPaths = this.Config.ExtractExactFiles(mapPrefix);
        if (mapPaths.Count > 0)
        {
            Console.WriteLine(mapPaths[0]);
            // i guess i should make multiple versions available to select from... TODO
            this.ActiveMap = new MAP();
            this.ActiveMap.Read(mapPaths[0]);
            for (int subId=0; subId<this.ActiveMap.Entries.Length; subId++)
            {
                string[] modelPrefix = new string[] { "model", "field_tex" };
                string modelSuffix = $"f{this.ActiveMap.Entries[subId].MajorID:000}_{this.ActiveMap.Entries[subId].MinorID:000}_[0-9]\\.gfs";
                string[] texturePrefix = new string[] { "model", "field_tex", "textures" };
                string textureSuffix = $"tex{this.ActiveMap.Entries[subId].MajorID:000}_{this.ActiveMap.Entries[subId].MinorID:000}_[0-9][0-9]_00\\.bin";

                foreach (string path in this.Config.ExtractExactFiles(modelPrefix, modelSuffix))
                {
                    if (!this.SubModelPaths.ContainsKey(subId))
                        this.SubModelPaths[subId] = new ObservableCollection<string>();
                    this.SubModelPaths[subId].Add(path);
                    Console.WriteLine($"{subId}: {path}, {this.SubModelPaths[subId].Count}");
                }

                foreach (string path in this.Config.ExtractExactFiles(texturePrefix, textureSuffix))
                {
                    if (!this.TextureBinPaths.ContainsKey(subId))
                        this.TextureBinPaths[subId] = new ObservableCollection<string>();
                    this.TextureBinPaths[subId].Add(path);
                    Console.WriteLine($"{subId}: {path}, {this.TextureBinPaths[subId].Count}");
                }
            }
        }
        else
        {
            // actually doesn't seem like sub ID is used in field loading...
            // possibly used for fld "div" / room ID checks, though
            string[] modelPrefix = new string[] { "model", "field_tex" };
            string modelSuffix = $"f{this.MajorID.Value:000}_{this.MinorID.Value:000}_[0-9]\\.gfs";
            string[] texturePrefix = new string[] { "model", "field_tex", "textures" };
            string textureSuffix = $"tex{this.MajorID.Value:000}_{this.MinorID.Value:000}_[0-9][0-9]_00\\.bin";

            foreach (string path in this.Config.ExtractExactFiles(modelPrefix, modelSuffix))
            {
                int subId = Int32.Parse(path.Substring(path.Length-5, 1));
                if (!this.SubModelPaths.ContainsKey(subId))
                    this.SubModelPaths[subId] = new ObservableCollection<string>();
                this.SubModelPaths[subId].Add(path);
                Console.WriteLine($"{subId}: {path}, {this.SubModelPaths[subId].Count}");
            }

            foreach (string path in this.Config.ExtractExactFiles(texturePrefix, textureSuffix))
            {
                int subId = Int32.Parse(path.Substring(path.Length-9, 2));
                if (!this.TextureBinPaths.ContainsKey(subId))
                    this.TextureBinPaths[subId] = new ObservableCollection<string>();
                this.TextureBinPaths[subId].Add(path);
                Console.WriteLine($"{subId}: {path}, {this.TextureBinPaths[subId].Count}");
            }
        }

        this.ActiveSubModelPaths.Clear();
        this.ActiveTextureBinPaths.Clear();
        this.ActiveAttachmentPaths.Clear();

        foreach (int subId in this.SubModelPaths.Keys)
        {
            this.ActiveSubModelPaths[subId] = (this.SubModelPaths[subId] is null || this.SubModelPaths[subId].Count == 0) ? null : this.SubModelPaths[subId][0];
            if (!this.ActiveModels.ContainsKey(this.ActiveSubModelPaths[subId]))
                this.ActiveModels[this.ActiveSubModelPaths[subId]] = GFDLibrary.Api.FlatApi.LoadModel(this.ActiveSubModelPaths[subId]);
            this.ActiveAttachmentPaths[subId] = new Dictionary<int, string>();
            Parallel.ForEach(this.ActiveModels[this.ActiveSubModelPaths[subId]].Model.Nodes, node =>
            {
                if (node.Properties.ContainsKey("fldLayoutOfModel_major") && node.Properties.ContainsKey("fldLayoutOfModel_minor"))
                {
                    int majorId = (int)node.Properties["fldLayoutOfModel_major"].GetValue();
                    int minorId = (int)node.Properties["fldLayoutOfModel_minor"].GetValue();
                    // TODO: this logic may not be game-accurate... but if i change it, it also needs to be fixed in scenemanager and glmodel code!
                    int resId = (node.Properties.ContainsKey("fldLayoutOfModel_resId")) ? (int)node.Properties["fldLayoutOfModel_resId"].GetValue() : 0;
                    string[] pattern = new string[] { "model", "field_tex", "object", $"m{majorId:000}_{minorId:000}.gmd" };
                    List<string> matches = this.Config.ExtractExactFiles(pattern);
                    if (matches.Count > 0)
                    {
                        lock (this.ActiveAttachmentPaths[subId]) { this.ActiveAttachmentPaths[subId][resId] = matches[0]; }
                        if (!this.ActiveModels.ContainsKey(this.ActiveAttachmentPaths[subId][resId]))
                            lock (this.ActiveModels) { this.ActiveModels[this.ActiveAttachmentPaths[subId][resId]] = null; }
                    }
                }
            });
        }

        foreach (int subId in this.TextureBinPaths.Keys)
            this.ActiveTextureBinPaths[subId] = (this.TextureBinPaths[subId] is null || this.TextureBinPaths[subId].Count == 0) ? null : this.TextureBinPaths[subId][0];
    }

    //public List<string> UpdateAnimPaths(bool isBlendAnims, bool isExtAnims)
    public List<string> UpdateAnimPaths(bool isBlendAnims, bool isExtAnims, int forceAnimId=-1)
    {
        string animType = (isExtAnims) ? "A" : "B";
        //int animId = (isExtAnims) ? (int)this.ExtBaseAnimID.Value : (int)this.BaseAnimID.Value;
        int animId = (forceAnimId > -1) ? forceAnimId : (isExtAnims) ? (int)this.ExtBaseAnimID.Value : (int)this.BaseAnimID.Value;
        string[] prefix = null;
        string suffix = null;
        string[] backoffPrefix = null;
        string backoffSuffix = null;
        List<string> ret = new List<string>();

        switch (this.ObjectType.Choice)
        {
            case "Character":
            case "FieldCharacter":
                string subtype = (this.ObjectType.Choice == "FieldCharacter") ? "field" : "event";
                if (this.IsCommon.Value)
                {
                    if (animId == -1)
                        return ret;
                    prefix = new string[] { "model", "character", "common_anim", $"{animType}CMN{animId:0000}.gap" };
                }
                else if (isBlendAnims)
                {
                    prefix = new string[] { "model", "character", $"{this.MajorID.Value:0000}", $"emt{this.MajorID.Value:0000}_{this.MinorID.Value:000}.gap" };
                    backoffPrefix = new string[] { "model", "character", $"{this.MajorID.Value:0000}", $"emt{this.MajorID.Value:0000}.gap" };
                }
                else
                {
                    if (animId == -1)
                        return ret;
                    prefix = new string[] { "model", "character", $"{this.MajorID.Value:0000}", subtype, $"{animType}{subtype.Substring(0, 1)}{this.MajorID.Value:0000}_{animId:000}.gap" };
                    if (forceAnimId == -1)
                        backoffPrefix = new string[] { "model", "character", $"{this.MajorID.Value:0000}", subtype, $"{animType}{subtype.Substring(0, 1)}{this.MajorID.Value:0000}_{(animId+30):000}.gap" };
                }
                break;
            case "Enemy":
                if (animId == -1)
                    return ret;
                prefix = new string[] { "model", "character", "enemy", $"{this.MajorID.Value:0000}", $"{animType}em{this.MajorID.Value:0000}_{animId:000}.gap" };
                break;
            case "Persona":
                if (animId == -1)
                    return ret;
                prefix = new string[] { "model", "character", "persona", $"{this.MajorID.Value:0000}", $"{animType}ps{this.MajorID.Value:0000}_{animId:000}.gap" };
                break;
            case "Item":
                // yes, it's the GMD itself. for items, that's where animations are also stored
                prefix = new string[] { "model", "item", $"it{this.MajorID.Value:0000}_{this.MinorID.Value:000}.gmd" };
                break;
            case "FieldObject":
                prefix = new string[] { "model", "field_tex", "object", $"m{this.MajorID.Value:000}_{this.MinorID.Value:000}.gmd" };
                break;
            case "SymShadow":
                prefix = new string[] { "model", "character", "enemy", "symbol", $"sym{this.MajorID.Value:000}.gmd" };
                break;
            default:
                Trace.TraceWarning($"Unknown asset type: {this.ObjectType.Choice}");
                break;
        }

        if (prefix == null)
            return new List<string>();
        else
        {
            List<string> candidates = this.Config.ExtractExactFiles(prefix, suffix);
            if (candidates.Count == 0 && backoffPrefix != null)
                candidates = this.Config.ExtractExactFiles(backoffPrefix, backoffSuffix);
            if (candidates.Count == 0)
            {
                if (forceAnimId == -1 && animId > 0)
                    candidates = this.UpdateAnimPaths(isBlendAnims, isExtAnims, forceAnimId:animId-1);
                else if (forceAnimId > 0)
                    candidates = this.UpdateAnimPaths(isBlendAnims, isExtAnims, forceAnimId:forceAnimId-1);
            }
            return candidates;
        }
    }

    private void UpdateAllAnimPaths()
    {
        if (this.IsModel)
        {
            this.BaseAnimPaths.Clear();
            this.ExtBaseAnimPaths.Clear();
            this.AddAnimPaths.Clear();
            this.ExtAddAnimPaths.Clear();

            this.ActiveAddAnimPath = null;
            this.ActiveExtAddAnimPath = null;

            foreach (string path in this.UpdateAnimPaths(false, false))
                this.BaseAnimPaths.Add(path);
            foreach (string path in this.UpdateAnimPaths(false, true))
                this.ExtBaseAnimPaths.Add(path);
            foreach (string path in this.UpdateAnimPaths(true, false))
                this.AddAnimPaths.Add(path);
            foreach (string path in this.UpdateAnimPaths(true, true))
                this.ExtAddAnimPaths.Add(path);

            if (this.BaseAnimPaths.Count > 0)
            {
                if (this.ActiveBaseAnimPath != this.BaseAnimPaths[0])
                {
                    this.ActiveBaseAnimPath = this.BaseAnimPaths[0];
                    if (!this.ActiveModels.ContainsKey(this.ActiveBaseAnimPath))
                        this.ActiveModels[this.ActiveBaseAnimPath] = null;
                }
            }
            else
                this.ActiveBaseAnimPath = null;

            if (this.ExtBaseAnimPaths.Count > 0)
            {
                if (this.ActiveExtBaseAnimPath != this.ExtBaseAnimPaths[0])
                {
                    this.ActiveExtBaseAnimPath = this.ExtBaseAnimPaths[0];
                    if (!this.ActiveModels.ContainsKey(this.ActiveExtBaseAnimPath))
                        this.ActiveModels[this.ActiveExtBaseAnimPath] = null;
                }
            }
            else
                this.ActiveExtBaseAnimPath = null;

            if (this.AddAnimPaths.Count > 0)
            {
                if (this.ActiveAddAnimPath != this.AddAnimPaths[0])
                {
                    this.ActiveAddAnimPath = this.AddAnimPaths[0];
                    if (!this.ActiveModels.ContainsKey(this.ActiveAddAnimPath))
                        this.ActiveModels[this.ActiveAddAnimPath] = null;
                }
            }
            else
                this.ActiveAddAnimPath = null;

            if (this.ExtAddAnimPaths.Count > 0)
            {
                if (this.ActiveExtAddAnimPath != this.ExtAddAnimPaths[0])
                {
                    this.ActiveExtAddAnimPath = this.ExtAddAnimPaths[0];
                    if (!this.ActiveModels.ContainsKey(this.ActiveExtAddAnimPath))
                        this.ActiveModels[this.ActiveExtAddAnimPath] = null;
                }
            }
            else
                this.ActiveExtAddAnimPath = null;
        }
    }

    private void UpdateActiveModelCache()
    {
        Parallel.ForEach(this.ActiveModels.Keys, path =>
        {
            if (this.ActiveModels[path] is null)
                this.ActiveModels[path] = GFDLibrary.Api.FlatApi.LoadModel(path);
        });
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

    public static BiDict<string, int> ObjectCategories = new BiDict<string, int>
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

