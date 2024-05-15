using System;

namespace EVTUI.ViewModels;

public class LandingPageViewModel : ViewModelBase
{
 
    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public DataManager Config;

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public LandingPageViewModel(DataManager dataManager)
    {
        this.Config = dataManager;
    }

}
