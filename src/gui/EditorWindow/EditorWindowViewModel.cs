﻿using System;

using EVTUI;

namespace EVTUI.ViewModels;

public class EditorWindowViewModel : ViewModelBase
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public DataManager            Config          { get; }
    public AudioPanelViewModel    audioPanelVM    { get; }
    public TimelinePanelViewModel timelinePanelVM { get; }
    public ScriptPanelViewModel   scriptPanelVM   { get; }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public EditorWindowViewModel(DataManager dataManager)
    {
        this.Config          = dataManager;
        this.audioPanelVM    = new AudioPanelViewModel(this.Config);
        this.timelinePanelVM = new TimelinePanelViewModel(this.Config);
        this.scriptPanelVM   = new ScriptPanelViewModel(this.Config);
    }

    public void ClearCache()
    {
        this.Config.ClearCache();
    }

    public void SaveMod(string which)
    {
        switch (which)
        {
            case "EVT":
                this.Config.SaveModdedFiles(true, false, false, false);
                break;
            case "ECS":
                this.Config.SaveModdedFiles(false, true, false, false);
                break;
            case "BMD":
                this.Config.SaveModdedFiles(false, false, true, false);
                break;
            case "BF":
                this.Config.SaveModdedFiles(false, false, false, true);
                break;
            case null:
                this.Config.SaveModdedFiles(true, true, true, true);
                break;
            default:
                break;
        }
    }

}
