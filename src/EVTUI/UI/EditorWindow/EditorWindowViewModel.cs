namespace EVTUI.ViewModels;

public class EditorWindowViewModel : ViewModelBase
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public DataManager Config;
    public AudioPanelViewModel audioPanelVM { get; }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public EditorWindowViewModel(DataManager dataManager)
    {
        this.Config        = dataManager;
        this.audioPanelVM  = new AudioPanelViewModel(this.Config);
    }

}
