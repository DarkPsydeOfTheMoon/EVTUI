namespace EVTUI.ViewModels;

public class ConfigWindowViewModel : ViewModelBase
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public DataManager Config;
    public ConfigurationPanelViewModel ConfigPanelVM  { get; private set; }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public ConfigWindowViewModel(DataManager dataManager, string configtype)
    {
        this.Config         = dataManager;
        this.ConfigPanelVM  = new ConfigurationPanelViewModel(this.Config, configtype);
    }

    public void Dispose()
    {
        this.Config = null;
        this.ConfigPanelVM.Dispose();
        this.ConfigPanelVM = null;
    }

}
