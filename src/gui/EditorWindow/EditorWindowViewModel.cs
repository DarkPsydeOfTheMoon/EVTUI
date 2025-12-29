using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using ReactiveUI;

using EVTUI;

namespace EVTUI.ViewModels;

public class CommonViewModels : ReactiveObject
{
    private List<IDisposable> subscriptions;

    public CommonViewModels(DataManager dataManager)
    {
        this.subscriptions = new List<IDisposable>();

        this.Render = new GFDRenderingPanelViewModel(dataManager);

        this.Assets = new ObservableCollection<AssetViewModel>();
        this.AssetsByID = new Dictionary<int, AssetViewModel>();
        //foreach (SerialObject obj in dataManager.EventManager.SerialEvent.Objects)
        Parallel.ForEach(dataManager.EventManager.SerialEvent.Objects, obj =>
        {
            var asset = new AssetViewModel(dataManager, obj);
            lock (this.AssetsByID) { this.AssetsByID[obj.Id] = asset; }
            lock (this.Assets) { this.Assets.Add(this.AssetsByID[obj.Id]); }
            if (asset.ObjectType.Choice == "Field")
                this.Render.AddTextures(asset.ActiveTextureBinPaths);
        });

        this.Timeline = new TimelineViewModel(dataManager);

        this.subscriptions.Add(this.WhenAnyValue(x => x.Render.ReadyToRender).Subscribe(x =>
        {
            if (x)
            {
                foreach (AssetViewModel asset in this.Assets)
                //Parallel.ForEach(this.Assets, asset =>
                {
                    if (asset.ObjectType.Choice == "Character" || asset.ObjectType.Choice == "Field" || asset.ObjectType.Choice == "Item" || asset.ObjectType.Choice == "Persona" || asset.ObjectType.Choice == "Enemy" || asset.ObjectType.Choice == "SymShadow" || asset.ObjectType.Choice == "FieldObject")
                    {
                        this.Render.AddModel(asset);
                        if (asset.ObjectType.Choice != "Field")
                            this.Render.PositionModel(asset, this.Timeline);
                    }
                } //);
                this.Render.PlaceCamera(this.Timeline);
            }
            else if (!(this.Render.sceneManager is null))
                this.Render.sceneManager.teardown();
        }));
    }

    public void Dispose()
    {
        foreach (IDisposable subscription in this.subscriptions)
            subscription.Dispose();
        this.subscriptions.Clear();

        foreach (AssetViewModel asset in this.Assets)
            asset.Dispose();
        this.Assets.Clear();
        this.AssetsByID.Clear();

        this.Timeline.Dispose();

        this.Render.Dispose();
    }

    public ObservableCollection<AssetViewModel> Assets     { get; set; }
    public Dictionary<int, AssetViewModel>      AssetsByID { get; set; }

    public TimelineViewModel          Timeline { get; set; }
    public GFDRenderingPanelViewModel Render   { get; set; }
}

public class EditorWindowViewModel : ViewModelBase
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public DataManager            Config          { get; private set; }
    public CommonViewModels       CommonVMs       { get; private set; }
    public AssetsPanelViewModel   assetsPanelVM   { get; private set; }
    public TimelinePanelViewModel timelinePanelVM { get; private set; }
    public ScriptPanelViewModel   scriptPanelVM   { get; private set; }
    public AudioPanelViewModel    audioPanelVM    { get; private set; }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public EditorWindowViewModel(DataManager dataManager, Clipboard clipboard)
    {
        this.Config          = dataManager;
        this.CommonVMs       = new CommonViewModels(this.Config);
        this.assetsPanelVM   = new AssetsPanelViewModel(this.Config, this.CommonVMs);
        this.timelinePanelVM = new TimelinePanelViewModel(this.Config, this.CommonVMs, clipboard);
        this.scriptPanelVM   = new ScriptPanelViewModel(this.Config);
        this.audioPanelVM    = new AudioPanelViewModel(this.Config);
    }

    public void Dispose()
    {
        this.audioPanelVM.Dispose();
        this.scriptPanelVM.Dispose();
        this.timelinePanelVM.Dispose();
        this.assetsPanelVM.Dispose();
        this.CommonVMs.Dispose();
        this.Config.Reset();
        this.audioPanelVM = null;
        this.scriptPanelVM = null;
        this.timelinePanelVM = null;
        this.assetsPanelVM = null;
        this.Config = null;
    }

    public async Task SaveMod(string which)
    {
        switch (which)
        {
            case "EVT":
                await this.Config.SaveModdedFiles(true, false, false, false);
                break;
            case "ECS":
                await this.Config.SaveModdedFiles(false, true, false, false);
                break;
            case "BMD":
                await this.Config.SaveModdedFiles(false, false, true, false);
                break;
            case "BF":
                await this.Config.SaveModdedFiles(false, false, false, true);
                break;
            case null:
                await this.Config.SaveModdedFiles(true, true, true, true);
                break;
            default:
                break;
        }
    }

}
