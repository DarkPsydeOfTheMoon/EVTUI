namespace EVTUI.ViewModels;

public class ConfigWindowViewModel : ViewModelBase
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public DataManager Config;
    public ConfigurationPanelViewModel ConfigPanelVM  { get; }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public ConfigWindowViewModel(DataManager dataManager, string configtype)
    {
        this.Config         = dataManager;
        this.ConfigPanelVM  = new ConfigurationPanelViewModel(this.Config, configtype);
    }

}
