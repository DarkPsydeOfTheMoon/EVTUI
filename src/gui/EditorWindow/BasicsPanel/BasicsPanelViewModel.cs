using System;
using System.Collections.Generic;

using ReactiveUI;

using EVTUI;

namespace EVTUI.ViewModels;

public class BasicsPanelViewModel : ViewModelBase
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public DataManager Config { get; }

    // labels
    public NumEntryField        MajorID { get; set; }
    public NumEntryField        MinorID { get; set; }
    public StringSelectionField Rank    { get; set; }
    public NumEntryField        Level   { get; set; }

    // flags
    public BoolChoiceField InitScriptEnabled           { get; set; }
    public BoolChoiceField UnkFlag1                    { get; set; }
    public BoolChoiceField CinemascopeEnabled          { get; set; }
    public BoolChoiceField CinemascopeAnimationEnabled { get; set; }
    public BoolChoiceField EmbedBMD                    { get; set; }
    public BoolChoiceField EmbedBF                     { get; set; }
    public BoolChoiceField UnkFlag2                    { get; set; }

    // frames
    public NumEntryField FrameCount               { get; set; }
    public NumEntryField FrameRate                { get; set; }
    public NumEntryField StartingFrame            { get; set; }
    public NumEntryField CinemascopeStartingFrame { get; set; }

    // indices
    public NumEntryField InitScriptIndex { get; set; }
    public NumEntryField InitEnvAssetID  { get; set; }
    public NumEntryField DebugEnvAssetID { get; set; }

    // paths
    public StringEntryField BMDPath { get; set; }
    public StringEntryField BFPath  { get; set; }

    public bool Editable { get; set; }

    public enum Ranks : byte
    {
        None = 0,
        S    = 1,
        A    = 2,
        B    = 3,
        C    = 4,
        D    = 5
    }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public BasicsPanelViewModel(DataManager dataManager)
    {
        this.Config = dataManager;

        this.Editable = !this.Config.ReadOnly;

        EVT evt = (EVT)this.Config.EventManager.SerialEvent;

        // labels
        this.MajorID = new NumEntryField("Major ID", this.Editable, evt.MajorId, 0, 999, 1);
        this.MinorID = new NumEntryField("Minor ID", this.Editable, evt.MinorId, 0, 999, 1);
        this.Rank = new StringSelectionField("Rank", this.Editable, Enum.GetName(typeof(Ranks), evt.Rank), new List<string>(Enum.GetNames(typeof(Ranks))));
        this.Level = new NumEntryField("Level", this.Editable, evt.Level, 0, 3, 1);

        this.WhenAnyValue(x => x.MajorID.Value).Subscribe(x => evt.MajorId = (short)x);
        this.WhenAnyValue(x => x.MinorID.Value).Subscribe(x => evt.MinorId = (short)x);
        this.WhenAnyValue(x => x.Rank.Choice).Subscribe(x => evt.Rank = (byte)Enum.Parse(typeof(Ranks), x));
        this.WhenAnyValue(x => x.Level.Value).Subscribe(x => evt.Level = (byte)x);

        // scripts
        this.EmbedBMD = new BoolChoiceField("Use custom BMD path?", this.Editable, evt.Flags[12]);
        this.BMDPath = new StringEntryField("Custom BMD path", this.Editable, (evt.EventBmdPath is null) ? $"event_data/message/e{(100*(evt.MajorId/100)):000}/e{evt.MajorId:000}_{evt.MinorId:000}.bmd" : evt.EventBmdPath.Replace("\0", ""), 48);
        this.EmbedBF = new BoolChoiceField("Use custom BF path?", this.Editable, evt.Flags[14]);
        this.BFPath = new StringEntryField("Custom BF path", this.Editable, (evt.EventBfPath is null) ? $"event_data/message/e{(100*(evt.MajorId/100)):000}/e{evt.MajorId:000}_{evt.MinorId:000}.bf" : evt.EventBfPath.Replace("\0", ""), 48);
        this.InitScriptEnabled = new BoolChoiceField("Enable Init Script", this.Editable, evt.Flags[1]);
        this.InitScriptIndex = new NumEntryField("Init Script Index", this.Editable, (int)evt.InitScriptIndex, 0, 255, 1);

        this.WhenAnyValue(x => x.EmbedBMD.Value).Subscribe(x => evt.Flags[12] = this.EmbedBMD.Value);
        this.WhenAnyValue(x => x.BMDPath.Text).Subscribe(x => evt.EventBmdPath = this.BMDPath.Text);
        this.WhenAnyValue(x => x.EmbedBF.Value).Subscribe(x => evt.Flags[14] = this.EmbedBF.Value);
        this.WhenAnyValue(x => x.BFPath.Text).Subscribe(x => evt.EventBfPath = this.BFPath.Text);
        this.WhenAnyValue(x => x.InitScriptEnabled.Value).Subscribe(x => evt.Flags[1] = this.InitScriptEnabled.Value);
        this.WhenAnyValue(x => x.InitScriptIndex.Value).Subscribe(x => evt.InitScriptIndex = (byte)x);

        // cinemascope
        this.CinemascopeEnabled = new BoolChoiceField("Enable Cinemascope", this.Editable, evt.Flags[8]);
        this.CinemascopeAnimationEnabled = new BoolChoiceField("Enable Cinemascope Animation", this.Editable, evt.Flags[9]);
        this.CinemascopeStartingFrame = new NumEntryField("Cinemascope Starting Frame", this.Editable, (int)evt.CinemascopeStartingFrame, 0, 9999, 1);

        this.WhenAnyValue(x => x.CinemascopeEnabled.Value).Subscribe(x => evt.Flags[8] = this.CinemascopeEnabled.Value);
        this.WhenAnyValue(x => x.CinemascopeAnimationEnabled.Value).Subscribe(x => evt.Flags[9] = this.CinemascopeEnabled.Value);
        this.WhenAnyValue(x => x.CinemascopeStartingFrame.Value).Subscribe(x => evt.CinemascopeStartingFrame = (short)x);

        // env
        this.InitEnvAssetID = new NumEntryField("Init ENV ID", this.Editable, (int)evt.InitEnvAssetID, 0, 9999, 1);
        this.DebugEnvAssetID = new NumEntryField("Debug ENV ID", this.Editable, (int)evt.InitDebugEnvAssetID, 0, 9999, 1);

        this.WhenAnyValue(x => x.InitEnvAssetID.Value).Subscribe(x => evt.InitEnvAssetID = (int)x);
        this.WhenAnyValue(x => x.DebugEnvAssetID.Value).Subscribe(x => evt.InitDebugEnvAssetID = (int)x);

        // other flags
        this.UnkFlag1 = new BoolChoiceField("Unknown Flag #1", this.Editable, evt.Flags[6]);
        this.UnkFlag2 = new BoolChoiceField("Unknown Flag #2", this.Editable, evt.Flags[16]);

        this.WhenAnyValue(x => x.UnkFlag1.Value).Subscribe(x => evt.Flags[6] = this.UnkFlag1.Value);
        this.WhenAnyValue(x => x.UnkFlag2.Value).Subscribe(x => evt.Flags[16] = this.UnkFlag2.Value);

    }

}
