using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class MsgR : Generic
{
    public MsgR(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Dialogue Turn";

        // there are more fields (0, 1, 2; 4, 5, 8) but I don't know what they do yet....
        this.HasMessage = new BoolChoiceField("Includes Message?", this.Editable, (((this.CommandData.MessageMode) & 1) == 1));
        this.HasSelection = new BoolChoiceField("Includes Selection?", this.Editable, (((this.CommandData.MessageMode >> 1) & 1) == 1));
        this.IsSubtitle = new BoolChoiceField("Is Subtitle?", this.Editable, (((this.CommandData.MessageMode >> 2) & 1) == 1));

        string msgId = config.ScriptManager.GetTurnName(this.CommandData.MessageIndex);
        this.MessageID = new StringSelectionField("Message ID", this.Editable, msgId, config.ScriptManager.MsgNames);
        if (config.ScriptManager.MsgNames.Contains(this.MessageID.Choice))
            this.MessageBlock = new MessagePreview(config, this.CommandData.MessageIndex);
        this.WhenAnyValue(x => x.MessageID.Choice).Subscribe(x =>
        {
            int newMsgIndex = config.ScriptManager.GetTurnIndex(this.MessageID.Choice);
            if (config.ScriptManager.MsgNames.Contains(config.ScriptManager.GetTurnName(newMsgIndex)))
                this.MessageBlock = new MessagePreview(config, newMsgIndex);
        });

        string selId = config.ScriptManager.GetTurnName(this.CommandData.SelIndex);
        this.SelectionID = new StringSelectionField("Selection ID", this.Editable, selId, config.ScriptManager.SelNames);
        if (config.ScriptManager.SelNames.Contains(this.SelectionID.Choice))
            _selectionBlock = new SelectionPreview(config, this.CommandData.SelIndex);
        this.WhenAnyValue(x => x.SelectionID.Choice).Subscribe(x =>
        {
            int newSelIndex = config.ScriptManager.GetTurnIndex(this.SelectionID.Choice);
            if (config.ScriptManager.SelNames.Contains(config.ScriptManager.GetTurnName(newSelIndex)))
                this.SelectionBlock = new SelectionPreview(config, newSelIndex);
        });

        this.Config = config;
    }

    private DataManager Config;

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
            this.CommandData.MessageIndex = this.Config.ScriptManager.GetTurnIndex(this.MessageID.Choice);
        if (!(this.SelectionID is null))
            this.CommandData.SelIndex = this.Config.ScriptManager.GetTurnIndex(this.SelectionID.Choice);
        this.MessageBlock.SaveChanges();
        this.SelectionBlock.SaveChanges();
    }
}
