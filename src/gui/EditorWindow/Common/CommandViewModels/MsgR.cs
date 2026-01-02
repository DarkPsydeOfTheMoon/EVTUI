using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class MsgR : Generic
{
    public MsgR(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Message (by Index)";

        this.DisplayAsSubtitle = new BoolChoiceField("Display As Subtitle?", this.Editable, this.CommandData.Flags[2]);
        this.WhenAnyValue(_ => _.DisplayAsSubtitle.Value).Subscribe(_ => this.CommandData.Flags[2] = this.DisplayAsSubtitle.Value);

        // message
        this.MessageEnabled = new BoolChoiceField("Enable Message?", this.Editable, this.CommandData.Flags[0]);
        this.WhenAnyValue(_ => _.MessageEnabled.Value).Subscribe(_ => this.CommandData.Flags[0] = this.MessageEnabled.Value);
        this.EnableMessageCoordinates = new BoolChoiceField("Directly Specify Message Coordinates?", this.Editable, this.CommandData.Flags[5]);
        this.WhenAnyValue(_ => _.EnableMessageCoordinates.Value).Subscribe(_ => this.CommandData.Flags[5] = this.EnableMessageCoordinates.Value);
        this.MessageCoordinateType = new StringSelectionField("Coordinate Type", this.Editable, Generic.MessageCoordinateTypes.Backward[this.CommandData.MessageCoordinateType], Generic.MessageCoordinateTypes.Keys);
        this.WhenAnyValue(_ => _.MessageCoordinateType.Choice).Subscribe(_ => this.CommandData.MessageCoordinateType = Generic.MessageCoordinateTypes.Forward[this.MessageCoordinateType.Choice]);
        this.MessageX = new NumRangeField("X Coordinate", this.Editable, this.CommandData.MessageCoordinates[0], -9999, 9999, 1);
        this.WhenAnyValue(_ => _.MessageX.Value).Subscribe(_ => this.CommandData.MessageCoordinates[0] = (float)this.MessageX.Value);
        this.MessageY = new NumRangeField("Y Coordinate", this.Editable, this.CommandData.MessageCoordinates[1], -9999, 9999, 1);
        this.WhenAnyValue(_ => _.MessageY.Value).Subscribe(_ => this.CommandData.MessageCoordinates[1] = (float)this.MessageY.Value);

        // selection
        this.SelectionEnabled = new BoolChoiceField("Enable Selection?", this.Editable, this.CommandData.Flags[1]);
        this.WhenAnyValue(_ => _.SelectionEnabled.Value).Subscribe(_ => this.CommandData.Flags[1] = this.SelectionEnabled.Value);
        this.SelectionStorage = new NumEntryField("Local Data Storage ID", this.Editable, this.CommandData.EvtLocalDataIdSelStorage, 0, 99, 1);
        this.WhenAnyValue(_ => _.SelectionStorage.Value).Subscribe(_ => this.CommandData.EvtLocalDataIdSelStorage = (uint)this.SelectionStorage.Value);

        // unknown
        this.UnkBool1 = new BoolChoiceField("Unknown #1", this.Editable, this.CommandData.Flags[4]);
        this.WhenAnyValue(_ => _.UnkBool1.Value).Subscribe(_ => this.CommandData.Flags[4] = this.UnkBool1.Value);
        this.UnkBool2 = new BoolChoiceField("Unknown #3", this.Editable, this.CommandData.Flags[8]);
        this.WhenAnyValue(_ => _.UnkBool2.Value).Subscribe(_ => this.CommandData.Flags[8] = this.UnkBool2.Value);
        this.UnkFloat = new NumRangeField("Unknown #10", this.Editable, this.CommandData.UnkFloat, -99999, 99999, 1);
        this.WhenAnyValue(_ => _.UnkFloat.Value).Subscribe(_ => this.CommandData.UnkFloat = (float)this.UnkFloat.Value);

        // shenanigans lol
        string msgId = config.ScriptManager.GetTurnName((int)this.CommandData.MessageIndex);
        this.MessageID = new StringSelectionField("Message ID", this.Editable, msgId, config.ScriptManager.MsgNames);
        if (config.ScriptManager.MsgNames.Contains(this.MessageID.Choice))
            this.MessageBlock = new MessagePreview(config, (int)this.CommandData.MessageIndex);
        this.WhenAnyValue(x => x.MessageID.Choice).Subscribe(x =>
        {
            int newMsgIndex = config.ScriptManager.GetTurnIndex(this.MessageID.Choice);
            if (config.ScriptManager.MsgNames.Contains(config.ScriptManager.GetTurnName(newMsgIndex)))
                this.MessageBlock = new MessagePreview(config, newMsgIndex);
            this.CommandData.MessageIndex = (uint)config.ScriptManager.GetTurnIndex(this.MessageID.Choice);
        });

        string selId = config.ScriptManager.GetTurnName((int)this.CommandData.SelectIndex);
        this.SelectionID = new StringSelectionField("Selection ID", this.Editable, selId, config.ScriptManager.SelNames);
        if (config.ScriptManager.SelNames.Contains(this.SelectionID.Choice))
            _selectionBlock = new SelectionPreview(config, (int)this.CommandData.SelectIndex);
        this.WhenAnyValue(x => x.SelectionID.Choice).Subscribe(x =>
        {
            int newSelectIndex = config.ScriptManager.GetTurnIndex(this.SelectionID.Choice);
            if (config.ScriptManager.SelNames.Contains(config.ScriptManager.GetTurnName(newSelectIndex)))
                this.SelectionBlock = new SelectionPreview(config, newSelectIndex);
            this.CommandData.SelectIndex = (uint)config.ScriptManager.GetTurnIndex(this.SelectionID.Choice);
        });
    }

    public new void Dispose()
    {
        if (!(this.MessageBlock is null))
            this.MessageBlock.Dispose();
        if (!(this.SelectionBlock is null))
            this.SelectionBlock.Dispose();
        this.MessageBlock = null;
        this.SelectionBlock = null;
        base.Dispose();
    }

    public BoolChoiceField DisplayAsSubtitle { get; set; }

    // message
    public BoolChoiceField      MessageEnabled           { get; set; }
    public BoolChoiceField      EnableMessageCoordinates { get; set; }
    public StringSelectionField MessageCoordinateType    { get; set; }
    public NumRangeField        MessageX                 { get; set; }
    public NumRangeField        MessageY                 { get; set; }

    // selection
    public BoolChoiceField SelectionEnabled { get; set; }
    public NumEntryField   SelectionStorage { get; set; }

    // unknown
    public BoolChoiceField UnkBool1 { get; set; }
    public BoolChoiceField UnkBool2 { get; set; }
    public NumRangeField   UnkFloat { get; set; }

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
