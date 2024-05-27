using System.ComponentModel;
using ReactiveUI;

namespace EVTUI.ViewModels;

public class ViewModelBase : ReactiveObject, INotifyPropertyChanged
{
    // INotifyPropertyChanged Implementation
    new public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
