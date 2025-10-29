using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class Msg_ : Generic
{
    public Msg_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Dialogue Turn";

        // there are more fields (0, 1, 2; 4, 5, 8) but I don't know what they do yet....
        this.HasMessage = new BoolChoiceField("Includes Message?", this.Editable, (((this.CommandData.MessageMode) & 1) == 1));
        this.HasSelection = new BoolChoiceField("Includes Selection?", this.Editable, (((this.CommandData.MessageMode >> 1) & 1) == 1));
        this.IsSubtitle = new BoolChoiceField("Is Subtitle?", this.Editable, (((this.CommandData.MessageMode >> 2) & 1) == 1));

        int msgIndex = config.ScriptManager.GetTurnIndex(this.CommandData.MessageMajorId, this.CommandData.MessageMinorId, this.CommandData.MessageSubId);
        string msgId = config.ScriptManager.GetTurnName(msgIndex);
        this.MessageID = new StringSelectionField("Message ID", this.Editable, msgId, config.ScriptManager.MsgNames);
        if (!(config.ScriptManager.ActiveBMD is null))
        {
            if (config.ScriptManager.MsgNames.Contains(this.MessageID.Choice))
                this.MessageBlock = new MessagePreview(config, msgIndex);
            this.WhenAnyValue(x => x.MessageID.Choice).Subscribe(x =>
            {
                int newMsgIndex = config.ScriptManager.GetTurnIndex(this.MessageID.Choice);
                if (config.ScriptManager.MsgNames.Contains(config.ScriptManager.GetTurnName(newMsgIndex)))
                    this.MessageBlock = new MessagePreview(config, newMsgIndex);
            });

            int selIndex = config.ScriptManager.GetTurnIndex(this.CommandData.SelectMajorId, this.CommandData.SelectMinorId, this.CommandData.SelectSubId);
            string selId = config.ScriptManager.GetTurnName(selIndex);
            this.SelectionID = new StringSelectionField("Selection ID", this.Editable, selId, config.ScriptManager.SelNames);
            if (config.ScriptManager.SelNames.Contains(this.SelectionID.Choice))
                _selectionBlock = new SelectionPreview(config, selIndex);
            this.WhenAnyValue(x => x.SelectionID.Choice).Subscribe(x =>
            {
                int newSelIndex = config.ScriptManager.GetTurnIndex(this.SelectionID.Choice);
                if (config.ScriptManager.SelNames.Contains(config.ScriptManager.GetTurnName(newSelIndex)))
                    this.SelectionBlock = new SelectionPreview(config, newSelIndex);
            });
        }
    }

    public BoolChoiceField      HasMessage   { get; set; }
    public BoolChoiceField      HasSelection { get; set; }
    public BoolChoiceField      IsSubtitle   { get; set; }
    public StringSelectionField MessageID    { get; set; }
    public StringSelectionField SelectionID  { get; set; }

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

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.CommandData.MessageMode = (Convert.ToInt32(this.IsSubtitle.Value) << 2) + (Convert.ToInt32(this.HasSelection.Value) << 1) + Convert.ToInt32(this.HasMessage.Value);
        if (!(this.MessageID is null))
        {
            string[] msgPieces = this.MessageID.Choice.Split("_");
            if (msgPieces.Length == 4)
            {
                this.CommandData.MessageMajorId = Int16.Parse(msgPieces[1]);
                this.CommandData.MessageMinorId = byte.Parse(msgPieces[2]);
                this.CommandData.MessageSubId = byte.Parse(msgPieces[3]);
            }
        }

        if (!(this.SelectionID is null))
        {
            string[] selPieces = this.SelectionID.Choice.Split("_");
            if (selPieces.Length == 4)
            {
                this.CommandData.SelectMajorId = Int16.Parse(selPieces[1]);
                this.CommandData.SelectMinorId = byte.Parse(selPieces[2]);
                this.CommandData.SelectSubId = byte.Parse(selPieces[3]);
            }
        }
        if (!(this.MessageBlock is null))
            this.MessageBlock.SaveChanges();
        if (!(this.SelectionBlock is null))
            this.SelectionBlock.SaveChanges();
    }
}

public class MessagePreview : ReactiveObject
{
    public MessagePreview(DataManager config, int index)
    {
        this.Editable = !config.ReadOnly;
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

    public static List<string> MessagePrefixes = new List<string>{"UNK",     "DVL",   "MSG", "MND",     "PFM",    "SEL",    "TRV"   };
    public static List<string> MessageTypes    = new List<string>{"Unknown", "Enemy", "NPC", "Thought", "System", "Select", "Trivia"};

    public bool    Editable { get; }

    public StringSelectionField MessageType { get; set; }
    public StringSelectionField Speaker     { get; set; }

    public ObservableCollection<PagePreview> Pages { get; set; }

    public void SaveChanges()
    {
        foreach (PagePreview page in this.Pages)
            page.SaveChanges();
    }
}

public class PagePreview : ReactiveObject
{
    public PagePreview(DataManager config, int turnIndex, int pageIndex)
    {
        this.Editable = !config.ReadOnly;
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
    }

    public StringEntryField     Dialogue    { get; set; }

    public string? Source   { get; set; }
    public uint?   CueID    { get; set; }
    public bool    Editable { get; }

	// TODO: make this like... an observable/reactive derived property... idk how that works
    //public bool    HasVoiceLine { get { return (!(this.Source is null) && !(this.CueID is null)); } }
    private bool _hasVoiceLine;
    public bool HasVoiceLine
    {
        get => _hasVoiceLine;
        set => this.RaiseAndSetIfChanged(ref _hasVoiceLine, value);
    }

    public void SaveChanges() {}
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

    public void SaveChanges()
    {
        foreach (OptionPreview option in this.Options)
            option.SaveChanges();
    }
}

public class OptionPreview : ReactiveObject
{
    public OptionPreview(DataManager config, int turnIndex, int pageIndex)
    {
        this.Editable = !config.ReadOnly;
        this.Update(config, turnIndex, pageIndex);
    }

    public void Update(DataManager config, int turnIndex, int pageIndex)
    {
        string text = config.ScriptManager.GetTurnText(turnIndex, pageIndex);
        this.Dialogue = new StringEntryField($"Option #{pageIndex+1}", this.Editable, text, null);
    }

    public StringEntryField     Dialogue    { get; set; }

    public bool    Editable { get; }

    public void SaveChanges() {}
}
