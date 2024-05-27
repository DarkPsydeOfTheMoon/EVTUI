namespace EVTUI.ViewModels;

public class EditorWindowViewModel : ViewModelBase
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public DataManager Config;
    public AudioPanelViewModel audioPanelVM { get; }
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

}
