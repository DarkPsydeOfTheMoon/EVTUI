using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI;

namespace EVTUI.ViewModels;

public class ScriptPanelViewModel : ViewModelBase
{
    /////////////////////////////
    // *** PRIVATE MEMBERS *** //
    /////////////////////////////
    private List<IDisposable> subscriptions;
    private Dictionary<string, bool> IsMsg;

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public DataManager Config;
    public bool Editable { get; set; }

    // from header
    public BoolChoiceField InitScriptEnabled { get; set; }
    public BoolChoiceField EmbedBMD          { get; set; }
    public BoolChoiceField EmbedBF           { get; set; }
    public NumEntryField InitScriptIndex     { get; set; }
    public StringEntryField BMDPath          { get; set; }
    public StringEntryField BFPath           { get; set; }

    private ObservableCollection<string> _scriptNames;
    public ObservableCollection<string> ScriptNames
    {
        get => _scriptNames;
        set => this.RaiseAndSetIfChanged(ref _scriptNames, value);
    }

    private bool _hasCompiledFiles;
    public bool HasDecompiledFiles
    {
        get => _hasCompiledFiles;
        set
        {
            this.RaiseAndSetIfChanged(ref _hasCompiledFiles, value);
            OnPropertyChanged(nameof(HasDecompiledFiles));
        }
    }

    private string _selectedCompiledScriptName;
    public string SelectedCompiledScriptName
    {
        get => _selectedCompiledScriptName;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedCompiledScriptName, value);
            OnPropertyChanged(nameof(SelectedCompiledScriptName));
        }
    }

    private ObservableCollection<string> _scriptExtNames;
    public ObservableCollection<string> ScriptExtNames
    {
        get => _scriptExtNames;
        set
        {
            this.RaiseAndSetIfChanged(ref _scriptExtNames, value);
            OnPropertyChanged(nameof(ScriptExtNames));
        }
    }

    private string _selectedDecompiledScriptName;
    public string SelectedDecompiledScriptName
    {
        get => _selectedDecompiledScriptName;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedDecompiledScriptName, value);
            OnPropertyChanged(nameof(SelectedDecompiledScriptName));
        }
    }

    public string SelectedScriptContent
    {
        get
        {
            if (this.HasDecompiledFiles)
                return this.Config.ScriptManager.ScriptTexts[(this.IsMsg[this.SelectedCompiledScriptName]) ? "BMD" : "BF"][this.SelectedCompiledScriptName][this.SelectedDecompiledScriptName];
            else
                return "";
        }
        set
        {
            if (this.HasDecompiledFiles)
                this.Config.ScriptManager.ScriptTexts[(this.IsMsg[this.SelectedCompiledScriptName]) ? "BMD" : "BF"][this.SelectedCompiledScriptName][this.SelectedDecompiledScriptName] = value;
        }
    }

    private string _compilationLogs = "";
    public string CompilationLogs
    {
        get => _compilationLogs;
        set
        {
            this.RaiseAndSetIfChanged(ref _compilationLogs, value);
            OnPropertyChanged(nameof(CompilationLogs));
        }
    }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public ScriptPanelViewModel(DataManager Config)
    {
        this.subscriptions = new List<IDisposable>();

        this.Config = Config;
        this.Editable = !this.Config.ReadOnly;

        // from header
        EVT evt = (EVT)this.Config.EventManager.SerialEvent;
        this.EmbedBMD = new BoolChoiceField("Use custom BMD path?", this.Editable, evt.Flags[12]);
        this.BMDPath = new StringEntryField("Custom BMD path", this.Editable, (evt.EventBmdPath is null) ? $"event_data/message/e{(100*(evt.MajorId/100)):000}/e{evt.MajorId:000}_{evt.MinorId:000}.bmd" : evt.EventBmdPath.Replace("\0", ""), 48);
        this.EmbedBF = new BoolChoiceField("Use custom BF path?", this.Editable, evt.Flags[14]);
        this.BFPath = new StringEntryField("Custom BF path", this.Editable, (evt.EventBfPath is null) ? $"event_data/message/e{(100*(evt.MajorId/100)):000}/e{evt.MajorId:000}_{evt.MinorId:000}.bf" : evt.EventBfPath.Replace("\0", ""), 48);
        this.InitScriptEnabled = new BoolChoiceField("Enable Init Script", this.Editable, evt.Flags[1]);
        this.InitScriptIndex = new NumEntryField("Init Script Index", this.Editable, (int)evt.InitScriptIndex, 0, 255, 1);

        this.subscriptions.Add(this.WhenAnyValue(x => x.EmbedBMD.Value).Subscribe(x => evt.Flags[12] = this.EmbedBMD.Value));
        this.subscriptions.Add(this.WhenAnyValue(x => x.BMDPath.Text).Subscribe(x => evt.EventBmdPath = this.BMDPath.Text));
        this.subscriptions.Add(this.WhenAnyValue(x => x.EmbedBF.Value).Subscribe(x => evt.Flags[14] = this.EmbedBF.Value));
        this.subscriptions.Add(this.WhenAnyValue(x => x.BFPath.Text).Subscribe(x => evt.EventBfPath = this.BFPath.Text));
        this.subscriptions.Add(this.WhenAnyValue(x => x.InitScriptEnabled.Value).Subscribe(x => evt.Flags[1] = this.InitScriptEnabled.Value));
        this.subscriptions.Add(this.WhenAnyValue(x => x.InitScriptIndex.Value).Subscribe(x => evt.InitScriptIndex = (byte)x));

        this._scriptNames = new ObservableCollection<string>();
        this._scriptExtNames = new ObservableCollection<string>();
        this.IsMsg = new Dictionary<string, bool>();
        foreach (string scriptType in this.Config.ScriptManager.ScriptList.Keys)
            foreach (string script in this.Config.ScriptManager.ScriptList[scriptType])
            {
                this._scriptNames.Add(script);
                this.IsMsg[script] = (scriptType == "BMD");
            }
        if (this.ScriptNames.Count > 0)
        {
            this.SelectedCompiledScriptName = this.ScriptNames[0];
            this.UpdateSubfiles();
        }
    }

    public void Dispose()
    {
        foreach (IDisposable subscription in this.subscriptions)
            subscription.Dispose();
        this.subscriptions.Clear();

        this.ScriptNames.Clear();
        this.ScriptExtNames.Clear();
        this.IsMsg.Clear();
        this.Config = null;
    }

    public void UpdateSubfiles()
    {
        this.HasDecompiledFiles = (this.Config.ScriptManager.ScriptTexts[(this.IsMsg[this.SelectedCompiledScriptName]) ? "BMD" : "BF"][this.SelectedCompiledScriptName].Count > 0);

        this.ScriptExtNames.Clear();
        if (this.HasDecompiledFiles)
        {
            foreach (string key in this.Config.ScriptManager.ScriptTexts[(this.IsMsg[this.SelectedCompiledScriptName]) ? "BMD" : "BF"][this.SelectedCompiledScriptName].Keys)
                this.ScriptExtNames.Add(key);
            this.SelectedDecompiledScriptName = this.ScriptExtNames[0];
        }
        else
        {
            this.SelectedDecompiledScriptName = null;
        }

        this.CompilationLogs = this.Config.ScriptManager.ScriptErrors[(this.IsMsg[this.SelectedCompiledScriptName]) ? "BMD" : "BF"][this.SelectedCompiledScriptName];
    }

    public void Compile()
    {
        if (this.IsMsg[this.SelectedCompiledScriptName])
            this.CompilationLogs = this.Config.CompileMessage(this.SelectedCompiledScriptName);
        else
            this.CompilationLogs = this.Config.CompileScript(this.SelectedCompiledScriptName);
    }

}
