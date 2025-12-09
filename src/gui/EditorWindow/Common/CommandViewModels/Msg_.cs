using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

using ReactiveUI;

using ImageMagick;

namespace EVTUI.ViewModels.TimelineCommands;

public class Msg_ : Generic
{
    public Msg_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Message (by IDs)";

        this.DisplayAsSubtitle = new BoolChoiceField("Display As Subtitle?", this.Editable, this.CommandData.Flags[2]);
        this.WhenAnyValue(_ => _.DisplayAsSubtitle.Value).Subscribe(_ => this.CommandData.Flags[2] = this.DisplayAsSubtitle.Value);
        //this.ReferenceByIndex = new BoolChoiceField("Reference By Index?", this.Editable, this.CommandData.Flags[31]);
        //this.WhenAnyValue(_ => _.ReferenceByIndex.Value).Subscribe(_ => this.CommandData.Flags[31] = this.ReferenceByIndex.Value);

        // message
        this.MessageEnabled = new BoolChoiceField("Enable Message?", this.Editable, this.CommandData.Flags[0]);
        this.WhenAnyValue(_ => _.MessageEnabled.Value).Subscribe(_ => this.CommandData.Flags[0] = this.MessageEnabled.Value);
        this.EnableMessageCoordinates = new BoolChoiceField("Directly Specify Message Coordinates?", this.Editable, this.CommandData.Flags[5]);
        this.WhenAnyValue(_ => _.EnableMessageCoordinates.Value).Subscribe(_ => this.CommandData.Flags[5] = this.EnableMessageCoordinates.Value);
        this.MessageCoordinateType = new StringSelectionField("Coordinate Type", this.Editable, this.MessageCoordinateTypes.Backward[this.CommandData.MessageCoordinateType], this.MessageCoordinateTypes.Keys);
        this.WhenAnyValue(_ => _.MessageCoordinateType.Choice).Subscribe(_ => this.CommandData.MessageCoordinateType = this.MessageCoordinateTypes.Forward[this.MessageCoordinateType.Choice]);
        this.MessageX = new NumRangeField("X Coordinate", this.Editable, this.CommandData.MessageCoordinates[0], -9999, 9999, 1);
        this.WhenAnyValue(_ => _.MessageX.Value).Subscribe(_ => this.CommandData.MessageCoordinates[0] = (float)this.MessageX.Value);
        this.MessageY = new NumRangeField("Y Coordinate", this.Editable, this.CommandData.MessageCoordinates[1], -9999, 9999, 1);
        this.WhenAnyValue(_ => _.MessageY.Value).Subscribe(_ => this.CommandData.MessageCoordinates[1] = (float)this.MessageY.Value);

        // selection
        this.SelectionEnabled = new BoolChoiceField("Enable Selection?", this.Editable, this.CommandData.Flags[1]);
        this.WhenAnyValue(_ => _.SelectionEnabled.Value).Subscribe(_ => this.CommandData.Flags[1] = this.SelectionEnabled.Value);
        this.SelectionStorage = new NumEntryField("Local Data Storage ID", this.Editable, this.CommandData.EvtLocalDataIdSelStorage, 0, 99, 1);
        this.WhenAnyValue(_ => _.SelectionStorage.Value).Subscribe(_ => this.CommandData.EvtLocalDataIdSelStorage = (uint)this.SelectionStorage.Value);

        // entries
        this.Entries = new ObservableCollection<MsgEntry>();
        for (int i=0; i<14; i++)
            this.Entries.Add(new MsgEntry(this.CommandData.Entries, this.CommandData.EntryFlags, i, this.Editable));

        // unknown
        this.UnkBool1 = new BoolChoiceField("Unknown #1", this.Editable, this.CommandData.Flags[4]);
        this.WhenAnyValue(_ => _.UnkBool1.Value).Subscribe(_ => this.CommandData.Flags[4] = this.UnkBool1.Value);
        this.UnkBool2 = new BoolChoiceField("Unknown #2", this.Editable, this.CommandData.Flags[8]);
        this.WhenAnyValue(_ => _.UnkBool2.Value).Subscribe(_ => this.CommandData.Flags[8] = this.UnkBool2.Value);

        this.UnkBool3 = new BoolChoiceField("Unknown #3", this.Editable, this.CommandData.EntryFlags[20]);
        this.WhenAnyValue(_ => _.UnkBool3.Value).Subscribe(_ => this.CommandData.Flags[20] = this.UnkBool3.Value);
        this.UnkBool4 = new BoolChoiceField("Unknown #4", this.Editable, this.CommandData.EntryFlags[21]);
        this.WhenAnyValue(_ => _.UnkBool4.Value).Subscribe(_ => this.CommandData.Flags[21] = this.UnkBool4.Value);
        this.UnkBool5 = new BoolChoiceField("Unknown #5", this.Editable, this.CommandData.EntryFlags[22]);
        this.WhenAnyValue(_ => _.UnkBool5.Value).Subscribe(_ => this.CommandData.Flags[22] = this.UnkBool5.Value);
        this.UnkBool6 = new BoolChoiceField("Unknown #6", this.Editable, this.CommandData.EntryFlags[23]);
        this.WhenAnyValue(_ => _.UnkBool6.Value).Subscribe(_ => this.CommandData.Flags[23] = this.UnkBool6.Value);
        this.UnkBool7 = new BoolChoiceField("Unknown #7", this.Editable, this.CommandData.EntryFlags[24]);
        this.WhenAnyValue(_ => _.UnkBool7.Value).Subscribe(_ => this.CommandData.Flags[24] = this.UnkBool7.Value);

        this.UnkEnum = new NumEntryField("Unknown #8", this.Editable, this.CommandData.UnkEnum, 0, 7, 1);
        this.WhenAnyValue(_ => _.UnkEnum.Value).Subscribe(_ => this.CommandData.UnkEnum = (uint)this.UnkEnum.Value);

        this.UnkFloat = new NumRangeField("Unknown #9", this.Editable, this.CommandData.UnkFloat, -99999, 99999, 1);
        this.WhenAnyValue(_ => _.UnkFloat.Value).Subscribe(_ => this.CommandData.UnkFloat = (float)this.UnkFloat.Value);

        this.RoyalUnkFloat1 = new NumRangeField("Unknown #10 (Royal-only)", this.Editable, this.CommandData.RoyalUnkFloats[0], 0, 500, 1);
        this.WhenAnyValue(_ => _.RoyalUnkFloat1.Value).Subscribe(_ => this.CommandData.RoyalUnkFloats[0] = (float)this.RoyalUnkFloat1.Value);
        this.RoyalUnkFloat2 = new NumRangeField("Unknown #11 (Royal-only)", this.Editable, this.CommandData.RoyalUnkFloats[1], 0, 500, 1);
        this.WhenAnyValue(_ => _.RoyalUnkFloat2.Value).Subscribe(_ => this.CommandData.RoyalUnkFloats[1] = (float)this.RoyalUnkFloat2.Value);

        // shenanigans lol
        int msgIndex = config.ScriptManager.GetTurnIndex((short)this.CommandData.MessageMajorId, this.CommandData.MessageMinorId, this.CommandData.MessageSubId);
        string msgId = config.ScriptManager.GetTurnName(msgIndex);
        this.MessageID = new StringSelectionField("Message ID", this.Editable, msgId, config.ScriptManager.MsgNames.Where(x => msgPatt.IsMatch(x)).ToList(), info: "Messages must match the pattern XXX_NNN_N_N, where X are capital letters and N are numbers");
        if (!(config.ScriptManager.ActiveBMD is null))
        {
            if (config.ScriptManager.MsgNames.Contains(this.MessageID.Choice))
                this.MessageBlock = new MessagePreview(config, msgIndex);
            this.WhenAnyValue(x => x.MessageID.Choice).Subscribe(x =>
            {
                int newMsgIndex = config.ScriptManager.GetTurnIndex(this.MessageID.Choice);
                if (config.ScriptManager.MsgNames.Contains(config.ScriptManager.GetTurnName(newMsgIndex)))
                    this.MessageBlock = new MessagePreview(config, newMsgIndex);
                string[] msgPieces = this.MessageID.Choice.Split("_");
                if (msgPieces.Length == 4)
                {
                    this.CommandData.MessageMajorId = UInt16.Parse(msgPieces[1]);
                    this.CommandData.MessageMinorId = byte.Parse(msgPieces[2]);
                    this.CommandData.MessageSubId = byte.Parse(msgPieces[3]);
                }
            });

            int selIndex = config.ScriptManager.GetTurnIndex((short)this.CommandData.SelectMajorId, this.CommandData.SelectMinorId, this.CommandData.SelectSubId);
            string selId = config.ScriptManager.GetTurnName(selIndex);
            this.SelectionID = new StringSelectionField("Selection ID", this.Editable, selId, config.ScriptManager.SelNames.Where(x => selPatt.IsMatch(x)).ToList(), info: "Selections must match the pattern SEL_NNN_N_N, where N are numbers");
            if (config.ScriptManager.SelNames.Contains(this.SelectionID.Choice))
                _selectionBlock = new SelectionPreview(config, selIndex);
            this.WhenAnyValue(x => x.SelectionID.Choice).Subscribe(x =>
            {
                int newSelIndex = config.ScriptManager.GetTurnIndex(this.SelectionID.Choice);
                if (config.ScriptManager.SelNames.Contains(config.ScriptManager.GetTurnName(newSelIndex)))
                    this.SelectionBlock = new SelectionPreview(config, newSelIndex);
                string[] selPieces = this.SelectionID.Choice.Split("_");
                if (selPieces.Length == 4)
                {
                    this.CommandData.SelectMajorId = UInt16.Parse(selPieces[1]);
                    this.CommandData.SelectMinorId = byte.Parse(selPieces[2]);
                    this.CommandData.SelectSubId = byte.Parse(selPieces[3]);
                }
            });
        }
    }

    private static Regex msgPatt = new Regex("^[A-Z]+_[0-9][0-9][0-9]_[0-9]_[0-9]$");
    private static Regex selPatt = new Regex("^[A-Z]+_[0-9][0-9][0-9]_[0-9]_[0-9]$");

    public BoolChoiceField DisplayAsSubtitle { get; set; }
    public BoolChoiceField ReferenceByIndex  { get; set; }

    // message
    public BoolChoiceField      MessageEnabled           { get; set; }
    public BoolChoiceField      EnableMessageCoordinates { get; set; }
    public StringSelectionField MessageCoordinateType    { get; set; }
    public NumRangeField        MessageX                 { get; set; }
    public NumRangeField        MessageY                 { get; set; }

    // selection
    public BoolChoiceField SelectionEnabled { get; set; }
    public NumEntryField   SelectionStorage { get; set; }

    // entries
    public ObservableCollection<MsgEntry> Entries { get; set; }

    // unknown
    public BoolChoiceField UnkBool1       { get; set; }
    public BoolChoiceField UnkBool2       { get; set; }
    public BoolChoiceField UnkBool3       { get; set; }
    public BoolChoiceField UnkBool4       { get; set; }
    public BoolChoiceField UnkBool5       { get; set; }
    public BoolChoiceField UnkBool6       { get; set; }
    public BoolChoiceField UnkBool7       { get; set; }
    public NumEntryField   UnkEnum        { get; set; }
    public NumRangeField   UnkFloat       { get; set; }
    public NumRangeField   RoyalUnkFloat1 { get; set; }
    public NumRangeField   RoyalUnkFloat2 { get; set; }

    // shenanigans lol
    public StringSelectionField MessageID   { get; set; }
    public StringSelectionField SelectionID { get; set; }

    private MessagePreview _messageBlock;
    public MessagePreview MessageBlock
    {
        get => _messageBlock;
        set => this.RaiseAndSetIfChanged(ref _messageBlock, value);
    }

    private SelectionPreview _selectionBlock;
    public SelectionPreview SelectionBlock
    {
        get => _selectionBlock;
        set => this.RaiseAndSetIfChanged(ref _selectionBlock, value);
    }
}

public class MsgEntry : ViewModelBase
{
    public MsgEntry(SerialMsgEntry[] entries, Bitfield32 flags, int i, bool editable)
    {
        this.Name = $"Entry #{i+1}";
        this.Editable = editable;

        this.Enabled = new BoolChoiceField("Enable Entry?", this.Editable, flags[i]);
        this.WhenAnyValue(_ => _.Enabled.Value).Subscribe(_ => flags[i] = this.Enabled.Value);

        this.UnkShort1 = new NumEntryField("Unknown #1", this.Editable, entries[i].UnkShort1, -1, 999, 1);
        this.WhenAnyValue(_ => _.UnkShort1.Value).Subscribe(_ => entries[i].UnkShort1 = (short)this.UnkShort1.Value);

        this.UnkShort2 = new NumEntryField("Unknown #2", this.Editable, entries[i].UnkShort2, 0, 999, 1);
        this.WhenAnyValue(_ => _.UnkShort2.Value).Subscribe(_ => entries[i].UnkShort2 = (ushort)this.UnkShort2.Value);

        this.UnkFloat = new NumRangeField("Unknown #3", this.Editable, entries[i].UnkFloat, 0, 2, 0.01);
        this.WhenAnyValue(_ => _.UnkFloat.Value).Subscribe(_ => entries[i].UnkFloat = (float)this.UnkFloat.Value);
    }

    public string Name     { get; }
    public bool   Editable { get; }

    public BoolChoiceField Enabled   { get; set; }
    public NumEntryField   UnkShort1 { get; set; }
    public NumEntryField   UnkShort2 { get; set; }
    public NumRangeField   UnkFloat  { get; set; }
}

public class MessagePreview : ReactiveObject
{
    public MessagePreview(DataManager config, int index)
    {
        //this.Editable = !config.ReadOnly;
        this.Editable = false;
        List<string> speakerNames = config.ScriptManager.SpeakerNames;
        string speaker = config.ScriptManager.GetTurnSpeakerName(index);
        string msgId = config.ScriptManager.GetTurnName(index);
        if (speaker == "")
            speaker = "(UNNAMED)";
        int prefInd = MessagePreview.MessagePrefixes.IndexOf(msgId.Substring(0, 3));
        this.MessageType = new StringSelectionField("Message Type", this.Editable, MessagePreview.MessageTypes[(prefInd < 0) ? 0 : prefInd], MessagePreview.MessageTypes);
        this.Speaker = new StringSelectionField("Speaker Name", this.Editable, speaker, speakerNames);

        this.Pages = new ObservableCollection<PagePreview>();
        for (int i=0; i<config.ScriptManager.GetTurnElemCount(index); i++)
            this.Pages.Add(new PagePreview(config, index, i));
    }

    // TODO: possibly get rid of Update logic now that entire object is observable...
    public void Update(DataManager config, int index)
    {
        string speaker = config.ScriptManager.GetTurnSpeakerName(index);
        string msgId = config.ScriptManager.GetTurnName(index);
        if (speaker == "")
            speaker = "(UNNAMED)";
        int prefInd = MessagePreview.MessagePrefixes.IndexOf(msgId.Substring(0, 3));
        this.MessageType.Choice = MessagePreview.MessageTypes[(prefInd < 0) ? 0 : prefInd];
        this.Speaker.Choice = speaker;

        this.Pages.Clear();
        for (int i=0; i<config.ScriptManager.GetTurnElemCount(index); i++)
            this.Pages.Add(new PagePreview(config, index, i));
    }

    public static List<string> MessagePrefixes = new List<string>{"UNK",     "DVL",   "MSG", "MND",     "PFM",            "SEL",    "SYS",    "TRV"   };
    public static List<string> MessageTypes    = new List<string>{"Unknown", "Enemy", "NPC", "Thought", "Persona Update", "Select", "System", "Trivia"};

    public bool Editable { get; }

    public StringSelectionField MessageType { get; set; }
    public StringSelectionField Speaker     { get; set; }

    public ObservableCollection<PagePreview> Pages { get; set; }
}

public class PagePreview : ReactiveObject
{
    public PagePreview(DataManager config, int turnIndex, int pageIndex)
    {
        //this.Editable = !config.ReadOnly;
        this.Editable = false;
        this.Update(config, turnIndex, pageIndex);
    }

    public void Update(DataManager config, int turnIndex, int pageIndex)
    {
        string text = config.ScriptManager.GetTurnText(turnIndex, pageIndex);
        (string Source, uint CueId)? voiceTuple = config.ScriptManager.GetTurnVoice(turnIndex, pageIndex);
        this.Dialogue = new StringEntryField($"Page #{pageIndex+1}", this.Editable, text, null);
        if (voiceTuple is null)
        {
            this.Source       = null;
            this.CueID        = null;
            this.HasVoiceLine = false;
        }
        else
        {
            this.Source       = (((string Source, uint CueId))voiceTuple).Source;
            this.CueID        = (((string Source, uint CueId))voiceTuple).CueId;
            this.HasVoiceLine = true;
        }

        this.BustupPath = config.ScriptManager.GetTurnBustupPath(turnIndex, pageIndex);
        if (!String.IsNullOrEmpty(this.BustupPath))
            this.Bustup = config.ScriptManager.GetMainBustupImage(this.BustupPath);

        this.CutinPath = config.ScriptManager.GetTurnCutinPath(turnIndex, pageIndex);
        if (!String.IsNullOrEmpty(this.CutinPath))
            this.Cutin = config.ScriptManager.GetMainCutinImage(this.CutinPath);
    }

    public StringEntryField     Dialogue    { get; set; }

    public string? Source   { get; set; }
    public uint?   CueID    { get; set; }
    public bool    Editable { get; }

    public string?     BustupPath { get; set; }
    public MagickImage Bustup     { get; set; }

    public string?     CutinPath { get; set; }
    public MagickImage Cutin     { get; set; }

	// TODO: make this like... an observable/reactive derived property... idk how that works
    //public bool    HasVoiceLine { get { return (!(this.Source is null) && !(this.CueID is null)); } }
    private bool _hasVoiceLine;
    public bool HasVoiceLine
    {
        get => _hasVoiceLine;
        set => this.RaiseAndSetIfChanged(ref _hasVoiceLine, value);
    }
}

public class SelectionPreview : ReactiveObject
{
    public SelectionPreview(DataManager config, int index)
    {
        this.Options = new ObservableCollection<OptionPreview>();
        for (int i=0; i<config.ScriptManager.GetTurnElemCount(index); i++)
            this.Options.Add(new OptionPreview(config, index, i));
    }

    public void Update(DataManager config, int index)
    {
        this.Options.Clear();
        for (int i=0; i<config.ScriptManager.GetTurnElemCount(index); i++)
            this.Options.Add(new OptionPreview(config, index, i));
    }

    public ObservableCollection<OptionPreview> Options { get; set; }
}

public class OptionPreview : ReactiveObject
{
    public OptionPreview(DataManager config, int turnIndex, int pageIndex)
    {
        //this.Editable = !config.ReadOnly;
        this.Editable = false;
        this.Update(config, turnIndex, pageIndex);
    }

    public void Update(DataManager config, int turnIndex, int pageIndex)
    {
        string text = config.ScriptManager.GetTurnText(turnIndex, pageIndex);
        this.Dialogue = new StringEntryField($"Option #{pageIndex+1}", this.Editable, text, null);
    }

    public StringEntryField Dialogue { get; set; }
    public bool             Editable { get; }
}
