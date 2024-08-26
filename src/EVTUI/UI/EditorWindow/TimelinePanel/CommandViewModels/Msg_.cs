using System;
using System.Collections.Generic;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class Msg_ : Generic
{
    public Msg_(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Dialogue Turn";
        int msgIndex = config.ScriptManager.GetTurnIndex(this.CommandData.MessageMajorId, this.CommandData.MessageMinorId, this.CommandData.MessageSubId);
        string msgId = config.ScriptManager.GetTurnName(msgIndex);
        this.MessageID = new StringSelectionField("Message ID", this.Editable, msgId, config.ScriptManager.MsgNames);
        if (((this.CommandData.MessageMode >> 1) & 1) == 1)
        {
            int selIndex = config.ScriptManager.GetTurnIndex(this.CommandData.SelectMajorId, this.CommandData.SelectMinorId, this.CommandData.SelectSubId);
            string selId = config.ScriptManager.GetTurnName(selIndex);
            this.SelectionID = new StringSelectionField("Selection ID", this.Editable, selId, config.ScriptManager.SelNames);
        }
        this.MessageBlock = new MessagePreview(config, msgIndex);
        this.WhenAnyValue(x => x.MessageID.Choice).Subscribe(x =>
        {
            int newMsgIndex = config.ScriptManager.GetTurnIndex(this.MessageID.Choice);
            this.MessageBlock.Update(config, newMsgIndex);
        });
    }

    public StringSelectionField MessageID    { get; set; }
    public StringSelectionField SelectionID  { get; set; }

    public MessagePreview       MessageBlock { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        string[] msgPieces = this.MessageID.Choice.Split("_");
        this.CommandData.MessageMajorId = Int16.Parse(msgPieces[1]);
        this.CommandData.MessageMinorId = byte.Parse(msgPieces[2]);
        this.CommandData.MessageSubId = byte.Parse(msgPieces[3]);

        if (!(this.SelectionID is null))
        {
            string[] selPieces = this.SelectionID.Choice.Split("_");
            this.CommandData.SelectMajorId = Int16.Parse(selPieces[1]);
            this.CommandData.SelectMinorId = byte.Parse(selPieces[2]);
            this.CommandData.SelectSubId = byte.Parse(selPieces[3]);
        }
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
        string text = config.ScriptManager.GetTurnText(index);
        if (speaker == "")
            speaker = "(UNNAMED)";
        this.MessageType = new StringSelectionField("Message Type", this.Editable, MessagePreview.MessageTypes[MessagePreview.MessagePrefixes.IndexOf(msgId.Substring(0, 3))], MessagePreview.MessageTypes);
        this.Speaker = new StringSelectionField("Speaker Name", this.Editable, speaker, speakerNames);
        this.Dialogue = new StringEntryField("Dialogue", this.Editable, text);
        List<(string Source, uint CueId)?> tuples = config.ScriptManager.GetTurnVoices(index);
        if (tuples.Count == 0 || tuples[0] is null)
        {
            this.Source = null;
            this.CueID = null;
            this.HasVoiceLine = false;
        }
        else
        {
            this.Source = (((string Source, uint CueId))tuples[0]).Source;
            this.CueID = (((string Source, uint CueId))tuples[0]).CueId;
            this.HasVoiceLine = true;
        }
    }

    public void Update(DataManager config, int index)
    {
        string speaker = config.ScriptManager.GetTurnSpeakerName(index);
        string msgId = config.ScriptManager.GetTurnName(index);
        string text = config.ScriptManager.GetTurnText(index);
        if (speaker == "")
            speaker = "(UNNAMED)";
        this.MessageType.Choice = MessagePreview.MessageTypes[MessagePreview.MessagePrefixes.IndexOf(msgId.Substring(0, 3))];
        this.Speaker.Choice = speaker;
        this.Dialogue.Text = text;
        List<(string Source, uint CueId)?> tuples = config.ScriptManager.GetTurnVoices(index);
        if (tuples.Count == 0 || tuples[0] is null)
        {
            this.Source = null;
            this.CueID = null;
            this.HasVoiceLine = false;
        }
        else
        {
            this.Source = (((string Source, uint CueId))tuples[0]).Source;
            this.CueID = (((string Source, uint CueId))tuples[0]).CueId;
            this.HasVoiceLine = true;
        }
    }

    public static List<string> MessagePrefixes = new List<string>{"DVL",   "MSG", "MND",     "PFM",    "SEL"   };
    public static List<string> MessageTypes    = new List<string>{"Enemy", "NPC", "Thought", "System", "Select"};

    public string? Source   { get; set; }
    public uint?   CueID    { get; set; }
    public bool    Editable { get; }

    //public bool    HasVoiceLine { get { return (!(this.Source is null) && !(this.CueID is null)); } }
    private bool _hasVoiceLine;
    public bool HasVoiceLine
    {
        get => _hasVoiceLine;
        set => this.RaiseAndSetIfChanged(ref _hasVoiceLine, value);
    }

    //public IntSelectionField    CueID       { get; set; }
    public StringSelectionField MessageType { get; set; }
    public StringSelectionField Speaker     { get; set; }
    public StringEntryField     Dialogue    { get; set; }

    public void SaveChanges() {}
}
