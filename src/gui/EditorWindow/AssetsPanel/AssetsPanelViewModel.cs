using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EVTUI.ViewModels;

public class AssetsPanelViewModel : ViewModelBase
{

    //////////////////////////////
    // *** REACTIVE MEMBERS *** //
    //////////////////////////////
    private (bool IsById, bool IsAscending) _sortMode = (true, true);
    public (bool IsById, bool IsAscending) SortMode
    {
        get => _sortMode;
        set
        {
            if (_sortMode != value)
            {
                _sortMode = value;
                this.SortAssets();
            }
        }
    }

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public DataManager Config { get; }

    public ObservableCollection<AssetViewModel> Assets { get; set; }

    public List<string> AddableTypes { get => AssetViewModel.ObjectTypes.Keys.ToList(); }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public AssetsPanelViewModel(DataManager dataManager, CommonViewModels commonVMs)
    {
        this.Config = dataManager;
        this.Assets = commonVMs.Assets;
        this.SortAssets();
    }

    public void SortAssets()
    {
        if (this.SortMode.IsAscending)
            if (this.SortMode.IsById)
                this.Assets = new ObservableCollection<AssetViewModel>(this.Assets.OrderBy(a => a.ObjectID.Value));
            else
                this.Assets = new ObservableCollection<AssetViewModel>(this.Assets.OrderBy(a => a.ObjectType.Choice));
        else
            if (this.SortMode.IsById)
                this.Assets = new ObservableCollection<AssetViewModel>(this.Assets.OrderByDescending(a => a.ObjectID.Value));
            else
                this.Assets = new ObservableCollection<AssetViewModel>(this.Assets.OrderByDescending(a => a.ObjectType.Choice));
        OnPropertyChanged(nameof(Assets));
    }

    public void AddAsset(string type)
    {
        SerialObject newObj = this.Config.EventManager.SerialEvent.NewObject(AssetViewModel.ObjectTypes.Forward[type]);
        this.Assets.Add(new AssetViewModel(this.Config, newObj));
        this.SortAssets();
    }

    public void DuplicateAsset(AssetViewModel asset)
    {
        SerialObject newObj = this.Config.EventManager.SerialEvent.DuplicateObject(asset.Obj);
        this.Assets.Add(new AssetViewModel(this.Config, newObj));
        this.SortAssets();
    }

    public bool DeleteAsset(AssetViewModel asset)
    {
        bool success = this.Config.EventManager.SerialEvent.DeleteObject(asset.Obj);
        asset.Obj = null;
        foreach (AssetViewModel candidate in this.Assets)
            if (candidate == asset)
            {
                success = this.Assets.Remove(candidate);
                break;
            }
        OnPropertyChanged(nameof(Assets));
        return success;
    }

}
