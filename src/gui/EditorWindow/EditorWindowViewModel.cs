using System;

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

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public EditorWindowViewModel(DataManager dataManager)
    {
        this.Config          = dataManager;
        this.audioPanelVM    = new AudioPanelViewModel(this.Config);
        this.timelinePanelVM = new TimelinePanelViewModel(this.Config);
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
                this.Config.SaveModdedFiles(true, false);
                break;
            case "ECS":
                this.Config.SaveModdedFiles(false, true);
                break;
            case null:
                this.Config.SaveModdedFiles(true, true);
                break;
            default:
                break;
        }
    }

}
