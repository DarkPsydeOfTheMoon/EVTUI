using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using ReactiveUI;

using EVTUI;

namespace EVTUI.ViewModels;

public class CommonViewModels : ReactiveObject
{
    public CommonViewModels(DataManager dataManager)
    {
        this.Assets = new ObservableCollection<AssetViewModel>();
        foreach (SerialObject obj in dataManager.EventManager.SerialEvent.Objects)
            this.Assets.Add(new AssetViewModel(dataManager, obj));

        this.Timeline = new TimelineViewModel(dataManager);

        this.Render = new GFDRenderingPanelViewModel(dataManager);
        this.WhenAnyValue(x => x.Render.ReadyToRender).Subscribe(x =>
        {
            if (x)
            {
                foreach (AssetViewModel asset in this.Assets)
                    //if (asset.IsModel)
                    if (asset.ObjectType.Choice == "Character" || asset.ObjectType.Choice == "Field" || asset.ObjectType.Choice == "Item")
                        this.Render.AddModel(asset, this.Timeline);
                this.Render.PlaceCamera(this.Timeline);
            }
            else
                this.Render.sceneManager.teardown();
        });
    }

    public ObservableCollection<AssetViewModel> Assets   { get; set; }
    public TimelineViewModel                    Timeline { get; set; }
    public GFDRenderingPanelViewModel           Render   { get; set; }
}

public class EditorWindowViewModel : ViewModelBase
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public DataManager            Config          { get; }
    public CommonViewModels       CommonVMs       { get; }
    public AssetsPanelViewModel   assetsPanelVM   { get; }
    public TimelinePanelViewModel timelinePanelVM { get; }
    public ScriptPanelViewModel   scriptPanelVM   { get; }
    public AudioPanelViewModel    audioPanelVM    { get; }

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
