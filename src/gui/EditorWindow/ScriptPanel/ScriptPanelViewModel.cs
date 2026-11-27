using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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

    private ObservableCollection<string> _vanillaScriptNames;
    public ObservableCollection<string> VanillaScriptNames
    {
        get => _vanillaScriptNames;
        set => this.RaiseAndSetIfChanged(ref _vanillaScriptNames, value);
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

    private bool _hasVanillaFiles;
    public bool HasVanillaFiles
    {
        get => _hasVanillaFiles;
        set
        {
            this.RaiseAndSetIfChanged(ref _hasVanillaFiles, value);
            OnPropertyChanged(nameof(HasVanillaFiles));
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

    private string _selectedVanillaScriptName;
    public string SelectedVanillaScriptName
    {
        get => _selectedVanillaScriptName;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedVanillaScriptName, value);
            OnPropertyChanged(nameof(_selectedVanillaScriptName));
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

    private ObservableCollection<string> _vanillaScriptExtNames;
    public ObservableCollection<string> VanillaScriptExtNames
    {
        get => _vanillaScriptExtNames;
        set
        {
            this.RaiseAndSetIfChanged(ref _vanillaScriptExtNames, value);
            OnPropertyChanged(nameof(VanillaScriptExtNames));
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

    private string _selectedVanillaDecompiledScriptName;
    public string SelectedVanillaDecompiledScriptName
    {
        get => _selectedVanillaDecompiledScriptName;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedVanillaDecompiledScriptName, value);
            OnPropertyChanged(nameof(SelectedVanillaDecompiledScriptName));
        }
    }


    public string SelectedScriptContent
    {
        get
        {
            if (!this.HasDecompiledFiles)
                return string.Empty;
            var scriptInfo = Config.ScriptManager.ModdedScripts.FirstOrDefault(s => s.path == this.SelectedCompiledScriptName, null);
            if (scriptInfo is null)
                return string.Empty;

            var path = Path.GetExtension(SelectedDecompiledScriptName) == ".flow" ? scriptInfo.flowPath : scriptInfo.msgPath;
            string localizedPath = Config.ScriptManager.GetLocalizedFilePath(path, Config.Language);
            return Config.ScriptManager.fileTexts[localizedPath];
        }
        set
        {
            if (!this.HasDecompiledFiles)
                return;
            var scriptInfo = Config.ScriptManager.ModdedScripts.FirstOrDefault(s => s.path == this.SelectedCompiledScriptName, null);
            if (scriptInfo is null)
                return;

            var path = Path.GetExtension(SelectedDecompiledScriptName) == ".flow" ? scriptInfo.flowPath : scriptInfo.msgPath;
            string localizedPath = Config.ScriptManager.GetLocalizedFilePath(path, Config.Language);
            Config.ScriptManager.fileTexts[localizedPath] = value;
        }
    }

    public string SelectedVanillaScriptContent
    {
        get
        {
            if (!this.HasVanillaFiles)
                return string.Empty;
            var scriptInfo = Config.ScriptManager.VanillaScripts.FirstOrDefault(s => s.path == SelectedVanillaScriptName, null);
            if (scriptInfo is null)
                return string.Empty;
            var filePath = Path.GetExtension(SelectedVanillaDecompiledScriptName) == ".flow" ? scriptInfo.flowPath : scriptInfo.msgPath;
            return Config.ScriptManager.fileTexts[filePath];
        }
        set
        {
            if (!this.HasVanillaFiles)
                return;
            var scriptInfo = Config.ScriptManager.VanillaScripts.FirstOrDefault(s => s.path == SelectedVanillaScriptName, null);
            if (scriptInfo is null)
                return;
            var filePath = Path.GetExtension(SelectedVanillaDecompiledScriptName) == ".flow" ? scriptInfo.flowPath : scriptInfo.msgPath;
            Config.ScriptManager.fileTexts[filePath] = value;
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
        this.BFPath = new StringEntryField("Custom BF path", this.Editable, (evt.EventBfPath is null) ? $"event_data/script/e{(100*(evt.MajorId/100)):000}/e{evt.MajorId:000}_{evt.MinorId:000}.bf" : evt.EventBfPath.Replace("\0", ""), 48);
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
        _vanillaScriptNames = new ObservableCollection<string>();
        _vanillaScriptExtNames = new ObservableCollection<string>();
        this.IsMsg = new Dictionary<string, bool>();

        foreach (var vanillaScript in this.Config.ScriptManager.VanillaScripts)
        {
            _vanillaScriptNames.Add(vanillaScript.path);
            IsMsg[vanillaScript.path] = vanillaScript.scriptKind == "BMD"; // it's proooooooobably fine for vanilla and modded to share this? since if two files have same name and path then they should have same script kind anyway -ocean
        }
        foreach (var moddedScript in this.Config.ScriptManager.ModdedScripts)
        {
            _scriptNames.Add(moddedScript.path);
            IsMsg[moddedScript.path] = moddedScript.scriptKind == "BMD";
        }
        if (this.ScriptNames.Count > 0)
        {
            this.SelectedCompiledScriptName = this.ScriptNames[0];
            this.UpdateSubfiles();
        }
        if (VanillaScriptNames.Count > 0)
        {
            SelectedVanillaScriptName = VanillaScriptNames[0];
            UpdateVanillaSubfiles();
        }

    }

    public void Dispose()
    {
        foreach (IDisposable subscription in this.subscriptions)
            subscription.Dispose();
        this.subscriptions.Clear();

        this.ScriptNames.Clear();
        this.ScriptExtNames.Clear();
        VanillaScriptNames.Clear();
        VanillaScriptExtNames.Clear();
        this.IsMsg.Clear();
        this.Config = null;
    }

    public void UpdateSubfiles()
    {
        var moddedScript = Config.ScriptManager.ModdedScripts.First(s => s.path == this.SelectedCompiledScriptName);
        if (!moddedScript.isEmulated)
        {
            if (moddedScript.scriptKind == "BF" && !File.Exists(moddedScript.flowPath))
            {
                Config.ScriptManager.TryDecompileBF(Path.Combine(Config.VanillaExtractionPath, moddedScript.path), moddedScript.flowPath, out var log);
                Config.ScriptManager.ScriptErrors[moddedScript.path][ScriptManager.GetLangCodeFromLang(Config.Language)] = log;
            }
            else if (moddedScript.scriptKind == "BMD" && !File.Exists(moddedScript.msgPath))
            {
                Config.ScriptManager.TryDecompileBMD(Path.Combine(Config.VanillaExtractionPath, moddedScript.path), moddedScript.msgPath, out var log);
                Config.ScriptManager.ScriptErrors[moddedScript.path][ScriptManager.GetLangCodeFromLang(Config.Language)] = log;
            }
        }
        bool moddedMsgExists = File.Exists(moddedScript.msgPath);
        bool moddedFlowExists = File.Exists(moddedScript.flowPath);
        this.HasDecompiledFiles = moddedMsgExists || moddedFlowExists;

        this.ScriptExtNames.Clear();
        if (this.HasDecompiledFiles)
        {
            if (moddedFlowExists)
                this.ScriptExtNames.Add(".flow");
            if (moddedMsgExists)
                this.ScriptExtNames.Add(".msg");
            this.SelectedDecompiledScriptName = this.ScriptExtNames[0];
        }
        else
        {
            this.SelectedDecompiledScriptName = null;
        }

        this.CompilationLogs = Config.ScriptManager.ScriptErrors[moddedScript.path][ScriptManager.GetLangCodeFromLang(Config.Language)];
    }

    public void UpdateLog()
    {
        var moddedScript = Config.ScriptManager.ModdedScripts.First(s => s.path == this.SelectedCompiledScriptName);
        this.CompilationLogs = Config.ScriptManager.ScriptErrors[moddedScript.path][ScriptManager.GetLangCodeFromLang(Config.Language)];
    }

    // there might be a cleaner option than having this as a separate function but this seems less annoying than refreshing both textboxes every time
    public void UpdateVanillaSubfiles()
    {
        var vanillaScript = Config.ScriptManager.VanillaScripts.First(s => s.path == this.SelectedVanillaScriptName);
        string log = string.Empty;
        if (vanillaScript.scriptKind == "BF" && !File.Exists(vanillaScript.flowPath))
        {
            Config.ScriptManager.TryDecompileBF(Path.Combine(Config.VanillaExtractionPath, vanillaScript.path), Config.ScriptManager.GetEncodingFromLang(vanillaScript.language), vanillaScript.flowPath, out log);
        }
        else if (vanillaScript.scriptKind == "BMD" && !File.Exists(vanillaScript.msgPath))
        {
            Config.ScriptManager.TryDecompileBMD(Path.Combine(Config.VanillaExtractionPath, vanillaScript.path), Config.ScriptManager.GetEncodingFromLang(vanillaScript.language), vanillaScript.msgPath, out log);
        }

        bool vanillaMsgExists = File.Exists(vanillaScript.msgPath);
        bool vanillaFlowExists = File.Exists(vanillaScript.flowPath);
        this.HasVanillaFiles = vanillaMsgExists || vanillaFlowExists;

        this.VanillaScriptExtNames.Clear();
        if (this.HasVanillaFiles)
        {
            if (vanillaFlowExists)
                this.VanillaScriptExtNames.Add(".flow");
            if (vanillaMsgExists)
                this.VanillaScriptExtNames.Add(".msg");
            this.SelectedVanillaDecompiledScriptName = this.VanillaScriptExtNames[0];
        }
        else
        {
            this.SelectedVanillaDecompiledScriptName = null;
        }

        //this.CompilationLogs = log;
    }

    public void Compile()
    {
        if (this.IsMsg[this.SelectedCompiledScriptName])
        {
            this.CompilationLogs = this.Config.CompileMessage(this.SelectedCompiledScriptName);
        }
        else
        {
            this.CompilationLogs = this.Config.CompileScript(this.SelectedCompiledScriptName);
        }
    }

}
