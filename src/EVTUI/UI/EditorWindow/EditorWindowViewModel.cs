namespace EVTUI.ViewModels;

public class EditorWindowViewModel : ViewModelBase
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public DataManager Config;

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public EditorWindowViewModel(DataManager dataManager)
    {
        this.Config = dataManager;
    }

}
