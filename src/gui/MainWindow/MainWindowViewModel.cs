namespace EVTUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    //public DataManager Config;
    public LandingPageViewModel LandingPageVM { get; }
 
    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    //public MainWindowViewModel(DataManager dataManager)
    public MainWindowViewModel(User userData)
    {
        //this.Config        = dataManager;
        //this.LandingPageVM = new LandingPageViewModel(this.Config);
        this.LandingPageVM = new LandingPageViewModel(userData);
    }

}
