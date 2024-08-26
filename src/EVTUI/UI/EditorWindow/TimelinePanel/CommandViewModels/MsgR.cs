using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class MsgR : Generic
{
    public MsgR(DataManager config, SerialCommand command, object commandData) : base(config, command, commandData)
    {
        this.LongName = "Dialogue Turn";
        string msgId = config.ScriptManager.GetTurnName(this.CommandData.MessageIndex);
        this.MessageID = new StringSelectionField("Message ID", this.Editable, msgId, config.ScriptManager.MsgNames);
        if (((this.CommandData.MessageMode >> 1) & 1) == 1)
        {
            string selId = config.ScriptManager.GetTurnName(this.CommandData.SelIndex);
            this.SelectionID = new StringSelectionField("Selection ID", this.Editable, selId, config.ScriptManager.SelNames);
        }
        this.MessageBlock = new MessagePreview(config, this.CommandData.MessageIndex);
        this.WhenAnyValue(x => x.MessageID.Choice).Subscribe(x =>
        {
            int newMsgIndex = config.ScriptManager.GetTurnIndex(this.MessageID.Choice);
            this.MessageBlock.Update(config, newMsgIndex);
        });
        this.Config = config;
    }

    private DataManager Config;

    public StringSelectionField MessageID    { get; set; }
    public StringSelectionField SelectionID  { get; set; }

    public MessagePreview       MessageBlock { get; set; }

    public new void SaveChanges()
    {
        base.SaveChanges();
        this.CommandData.MessageIndex = this.Config.ScriptManager.GetTurnIndex(this.MessageID.Choice);
        if (!(this.SelectionID is null))
            this.CommandData.SelIndex = this.Config.ScriptManager.GetTurnIndex(this.SelectionID.Choice);
    }
}
