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
    private Dictionary<string, bool> IsMsg;

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public DataManager Config;
    // needed for calc binding, I guess......
    public bool ReadOnly { get { return this.Config.ReadOnly; } }

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

    private string _compilationLogs;
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
        this.Config = Config;
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
