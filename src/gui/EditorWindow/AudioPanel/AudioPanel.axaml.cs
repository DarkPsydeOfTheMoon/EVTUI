using ReactiveUI.Avalonia;

using EVTUI.ViewModels;

namespace EVTUI.Views;

public partial class AudioPanel : ReactiveUserControl<AudioPanelViewModel>
{
    public AudioPanel()
    {
        InitializeComponent();
    }
}
