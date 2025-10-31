using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using ReactiveUI;
//using ReactiveUI.Fody.Helpers;

namespace EVTUI.ViewModels;

public class DisplayableProject : ReactiveObject
{
    public DisplayableProject(DataManager config, int ind)
    {
        Project project = config.AllProjects[ind];
        Config   = config;
        _name     = project.Name;
        _notes   = project.Notes;
        GamePath = project.Game;
        ModPath  = project.Mod.Path;
        Ind      = ind;
    }

    private DataManager Config;

    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            this.RaiseAndSetIfChanged(ref _name, value);
            this.Config.ProjectManager.SetProjectName(this.Ind, _name);
        }
    }

    private string _notes;
    public string Notes
    {
        get => _notes;
        set
        {
            this.RaiseAndSetIfChanged(ref _notes, value);
            this.Config.ProjectManager.SetProjectNotes(this.Ind, _notes);
        }
    }

    public string GamePath { get; set; }
    public string ModPath  { get; set; }
    public int    Ind      { get; set; }
}

public class DisplayableGame : ReactiveObject
{
    public DisplayableGame(DataManager config, int ind)
    {
        GameSettings game = config.AllGames[ind];
        Config = config;
        Type   = game.Type;
        _notes = game.Notes;
        Path   = game.Path;
        Ind    = ind;
    }

    private DataManager Config;

    public string Type  { get; set; }

    private string _notes;
    public string Notes
    {
        get => _notes;
        set
        {
            this.RaiseAndSetIfChanged(ref _notes, value);
            this.Config.ProjectManager.SetGameNotes(this.Ind, _notes);
        }
    }

    public string Path  { get; set; }
    public int    Ind   { get; set; }
}

public class DisplayableEvent : ReactiveObject
{
    public DisplayableEvent(DataManager config, GameSettings game, Project project, int majorid, int minorid, bool hasProjPin, bool hasGamePin)
    {
        this.Game = game;
        this.Proj = project;
        Config  = config;
        MajorId = majorid;
        MinorId = minorid;
        foreach (ExpandedEvent evt in game.Events.Notes)
            if (evt.MajorId == majorid && evt.MinorId == minorid)
                _gameNotes  = evt.Text;
        if (!(project is null))
            foreach (ExpandedEvent evt in project.Events.Notes)
                if (evt.MajorId == majorid && evt.MinorId == minorid)
                    _projNotes  = evt.Text;
        _hasProjPin = hasProjPin;
        _hasGamePin = hasGamePin;
    }

    private DataManager Config;

    public int MajorId { get; set; }
    public int MinorId { get; set; }

    private bool _hasProjPin;
    public bool HasProjPin
    {
        get => _hasProjPin;
        set
        {
            this.RaiseAndSetIfChanged(ref _hasProjPin, value);
            this.Config.ProjectManager.SetProjectPin(this.Proj, this.MajorId, this.MinorId, _hasProjPin);
        }
    }

    private bool _hasGamePin;
    public bool HasGamePin
    {
        get => _hasGamePin;
        set
        {
            this.RaiseAndSetIfChanged(ref _hasGamePin, value);
            this.Config.ProjectManager.SetGamePin(this.Game, this.MajorId, this.MinorId, _hasGamePin);
        }
    }

    private string _gameNotes;
    public string GameNotes
    {
        get => _gameNotes;
        set
        {
            this.RaiseAndSetIfChanged(ref _gameNotes, value);
            this.Config.ProjectManager.SetGameEventNotes(this.Game, this.MajorId, this.MinorId, _gameNotes);
        }
    }

    private string _projNotes;
    public string ProjNotes
    {
        get => _projNotes;
        set
        {
            if (!(this.Proj is null))
            {
                this.RaiseAndSetIfChanged(ref _projNotes, value);
                this.Config.ProjectManager.SetProjectEventNotes(this.Proj, this.MajorId, this.MinorId, _projNotes);
            }
        }
    }

    private GameSettings Game;
    private Project      Proj;

}

public class ConfigurationPanelViewModel : ViewModelBase
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public DataManager Config { get; }
    public string      ConfigType;

    public NewProjectConfig newProjectConfig { get; set; }

    public string DisplayCPKPath { get => (this.newProjectConfig.GamePath is null) ? "(none)" : this.newProjectConfig.GamePath; }
    public string DisplayModPath { get => (this.newProjectConfig.ModPath is null) ? "(none)" : this.newProjectConfig.ModPath; }

    ////////////////////////////////
    // *** OBSERVABLE MEMBERS *** //
    ////////////////////////////////
    public ObservableCollection<DisplayableGame> GameList      { get; }
    public DisplayableGame                       GameSelection { get; set; }
    public bool AnyRecentGames { get => (!(this.GameList is null) && this.GameList.Count > 0); }
    public bool NoRecentGames  { get { return !this.AnyRecentGames; } }

    public ObservableCollection<DisplayableProject> ProjectList      { get; set; }
    public DisplayableProject                       ProjectSelection { get; set; }
    public bool AnyRecentProjects { get => (this.Config.AllProjects.Count > 0); }
    public bool NoRecentProjects { get => !this.AnyRecentProjects; }

    private ObservableCollection<DisplayableEvent> _eventList;
    public ObservableCollection<DisplayableEvent> EventList
    {
        get => _eventList;
        set => this.RaiseAndSetIfChanged(ref _eventList, value);
    }
    public bool AnyRecentEvents { get => (!(this.EventList is null) && this.EventList.Count > 0); }
    public bool NoRecentEvents  { get => !this.AnyRecentEvents; }

    private DisplayableEvent _eventSelection;
    public DisplayableEvent EventSelection
    {
        get => _eventSelection;
        set => this.RaiseAndSetIfChanged(ref _eventSelection, value);
    }
    public int? EventMajorId { get; set; } = 0;
    public int? EventMinorId { get; set; } = 0;

    public bool ProjectLoaded { get => this.Config.ProjectLoaded; }

    public ObservableCollection<string> EventCollections { get; } = new ObservableCollection<string>{"Recent Events", "Pinned Events", "All Events"};
    private string _selectedCollection = "Recent Events";
    public string SelectedCollection
    {
        get => _selectedCollection;
        set => this.RaiseAndSetIfChanged(ref _selectedCollection, value);
    }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public ConfigurationPanelViewModel(DataManager dataManager, string configtype)
    {
        this.Config = dataManager;
        this.ConfigType = configtype;

        this.ProjectList = new ObservableCollection<DisplayableProject>();
        this.GameList    = new ObservableCollection<DisplayableGame>();
        _eventList = new ObservableCollection<DisplayableEvent>();

        if (this.ConfigType == "open-proj")
        {
            this.Config.ReadOnly = false;
            for (int i=0; i<this.Config.AllProjects.Count; i++)
                this.ProjectList.Add(new DisplayableProject(this.Config, i));
        }
        else
        {
            this.newProjectConfig = new NewProjectConfig();
            if (this.ConfigType == "new-proj")
                this.Config.ReadOnly = false;
            else if (this.ConfigType == "read-only")
                this.Config.ReadOnly = true;
            for (int i=0; i<this.Config.AllGames.Count; i++)
                this.GameList.Add(new DisplayableGame(this.Config, i));
        }

        this.WhenAnyValue(x => x.ProjectList).Subscribe(x =>
        {
            OnPropertyChanged(nameof(ProjectList));
            this.ProjectSelection = null;
            if (this.ProjectList.Count > 0)
                this.ProjectSelection = this.ProjectList[0];
            OnPropertyChanged(nameof(ProjectSelection));
            OnPropertyChanged(nameof(AnyRecentProjects));
            OnPropertyChanged(nameof(NoRecentProjects));
        });

        this.WhenAnyValue(x => x.GameList).Subscribe(x =>
        {
            OnPropertyChanged(nameof(GameList));
            this.GameSelection = null;
            if (this.GameList.Count > 0)
                this.GameSelection = this.GameList[0];
            OnPropertyChanged(nameof(GameSelection));
            OnPropertyChanged(nameof(AnyRecentGames));
            OnPropertyChanged(nameof(NoRecentGames));
        });

        this.WhenAnyValue(x => x.EventList).Subscribe(x =>
        {
            OnPropertyChanged(nameof(EventList));
            this.EventSelection = null;
            if (this.EventList.Count > 0)
                this.EventSelection = this.EventList[0];
            OnPropertyChanged(nameof(EventSelection));
            OnPropertyChanged(nameof(AnyRecentEvents));
            OnPropertyChanged(nameof(NoRecentEvents));
        });

        this.WhenAnyValue(x => x.SelectedCollection).Subscribe(x => this.DisplayEvents());
    }
    
    public bool TrySetCPKs(string cpkdir)
    {
        List<string> cpks = this.Config.GetCPKsFromPath(cpkdir);
        if (cpks.Count <= 0)
            return false;

        if (!(newProjectConfig is null))
        {
            this.newProjectConfig.GamePath = cpkdir;
            OnPropertyChanged(nameof(DisplayCPKPath));
        }
        this.Config.CpkList = cpks;
        return true;
    }

    public async Task<int> TrySetModDir(string maybedir)
    {
        if (await this.Config.ProjectManager.ModPathAlreadyUsed(maybedir))
            return 1;
        this.newProjectConfig.ModPath = maybedir;
        OnPropertyChanged(nameof(DisplayModPath));
        return 0;
    }

    public async Task<int> TryCreateProject()
    {
        if (this.newProjectConfig.Name is null || this.newProjectConfig.Name == "")
            return 1;
        if (this.newProjectConfig.GamePath is null || this.newProjectConfig.GamePath == "")
            return 2;
        if (this.newProjectConfig.ModPath is null || this.newProjectConfig.ModPath == "")
            return 3;

        if (!(await this.Config.ProjectManager.TryUpdateProjects(this.newProjectConfig)))
            return 4;

        await this.Config.LoadProject(0);
        if (this.Config.ProjectManager.ActiveProject is null)
            return 5;

        return 0;
    }

    public async Task<int> TryUseCPKDir(string cpkdir, string gametype)
    {
        if (cpkdir is null)
        {
            if (this.GameSelection is null)
                return 1;
            if (this.GameSelection.Path is null)
                return 2;
            if (!this.TrySetCPKs(this.GameSelection.Path))
                return 3;
            if (this.Config.ReadOnly)
                await this.Config.LoadGameReadOnly(this.GameSelection.Ind);
        }
        else
        {
            if (!this.TrySetCPKs(cpkdir))
                return 3;
            if (this.Config.ReadOnly)
                await this.Config.ProjectManager.UpdateReadOnlyCPKs(cpkdir, gametype, "");
        }
        return 0;
    }

    public async Task<int> TryLoadProject()
    {
        if (this.ProjectSelection is null)
            return 1;
        if (this.ProjectSelection.GamePath is null)
            return 2;

        if (!this.TrySetCPKs(this.ProjectSelection.GamePath))
            return 3;

        await this.Config.LoadProject(this.ProjectSelection.Ind);
        if (this.Config.ProjectManager.ActiveProject is null)
            return 4;

        return 0;
    }

    public async Task<int> TryDeleteProject(DisplayableProject project)
    {
        if (this.Config.CheckIfProjOpen(project.ModPath) || !(this.Config.ProjectManager.ActiveGame is null && this.Config.ProjectManager.ActiveProject is null && this.Config.ProjectManager.ActiveEvent is null))
            return 1;

        this.ProjectSelection = null;
        this.ProjectList.Remove(project);

        if (await this.Config.ProjectManager.DeleteProject(project.Ind))
            return 0;
        else
        {
            // in case we cleared it from the view but it didn't actually delete lol
            this.ProjectList.Clear();
            for (int i=0; i<this.Config.AllProjects.Count; i++)
                this.ProjectList.Add(new DisplayableProject(this.Config, i));
            return 2;
        }
    }

    public List<DisplayableEvent> ConstructDisplayableEvents(List<SimpleEvent> evts, List<SimpleEvent> projPins, List<SimpleEvent> gamePins)
    {
        List<DisplayableEvent> ret = new List<DisplayableEvent>();
        foreach (SimpleEvent evt in evts)
        {
            bool hasProjPin = false;
            if (!(projPins is null))
                foreach (SimpleEvent pin in projPins)
                    if (pin.MajorId == evt.MajorId && pin.MinorId == evt.MinorId)
                    {
                        hasProjPin = true;
                        break;
                    }
            bool hasGamePin = false;
            foreach (SimpleEvent pin in gamePins)
                if (pin.MajorId == evt.MajorId && pin.MinorId == evt.MinorId)
                {
                    hasGamePin = true;
                    break;
                }
            ret.Add(new DisplayableEvent(this.Config, this.Config.ProjectManager.ActiveGame, this.Config.ProjectManager.ActiveProject, evt.MajorId, evt.MinorId, hasProjPin, hasGamePin));
        }
        return ret;
    }

    public void DisplayEvents()
    {
        OnPropertyChanged(nameof(ProjectLoaded));

        ObservableCollection<DisplayableEvent> eventList = new ObservableCollection<DisplayableEvent>();
        if (this.SelectedCollection == "All Events")
            foreach (DisplayableEvent evt in this.ConstructDisplayableEvents(this.Config.ListAllEvents(), (this.Config.ProjectManager.ActiveProject is null) ? null : this.Config.ProjectManager.ActiveProject.Events.Pinned, this.Config.ProjectManager.ActiveGame.Events.Pinned))
                eventList.Add(evt);
        else if (!(this.Config.ProjectManager.ActiveProject is null))
        {
            if (this.SelectedCollection == "Recent Events")
                foreach (DisplayableEvent evt in this.ConstructDisplayableEvents(this.Config.ProjectManager.ActiveProject.Events.Recent, this.Config.ProjectManager.ActiveProject.Events.Pinned, this.Config.ProjectManager.ActiveGame.Events.Pinned))
                    eventList.Add(evt);
            else if (this.SelectedCollection == "Pinned Events")
            {
                Dictionary<(int MajorId, int MinorId), SimpleEvent> combinedPins = new Dictionary<(int MajorId, int MinorId), SimpleEvent>();
                foreach (SimpleEvent evt in this.Config.ProjectManager.ActiveProject.Events.Pinned)
                    combinedPins[(evt.MajorId, evt.MinorId)] = evt;
                foreach (SimpleEvent evt in this.Config.ProjectManager.ActiveGame.Events.Pinned)
                    combinedPins[(evt.MajorId, evt.MinorId)] = evt;
                foreach (DisplayableEvent evt in this.ConstructDisplayableEvents(combinedPins.Values.ToList(), this.Config.ProjectManager.ActiveProject.Events.Pinned, this.Config.ProjectManager.ActiveGame.Events.Pinned))
                    eventList.Add(evt);
            }
        }
        else if (!(this.Config.ProjectManager.ActiveGame is null))
        {
            if (this.SelectedCollection == "Recent Events")
                foreach (DisplayableEvent evt in this.ConstructDisplayableEvents(this.Config.ProjectManager.ActiveGame.Events.Recent, null, this.Config.ProjectManager.ActiveGame.Events.Pinned))
                    eventList.Add(evt);
            else if (this.SelectedCollection == "Pinned Events")
                foreach (DisplayableEvent evt in this.ConstructDisplayableEvents(this.Config.ProjectManager.ActiveGame.Events.Pinned, null, this.Config.ProjectManager.ActiveGame.Events.Pinned))
                    eventList.Add(evt);
        }
        this.EventList = eventList;
    }

    public async Task<int> TryLoadEvent(bool fromSelection)
    {
        if (fromSelection)
        {
            if (this.EventSelection is null)
                return 1;
            this.EventMajorId = this.EventSelection.MajorId;
            this.EventMinorId = this.EventSelection.MinorId;
            OnPropertyChanged(nameof(this.EventMajorId));
            OnPropertyChanged(nameof(this.EventMinorId));
        }

        try
        {
            if (!(await this.Config.LoadEvent((int)this.EventMajorId, (int)this.EventMinorId)))
                return 2;
            else if (!this.Config.EventLoaded)
                return 3;
        }
        catch (IOException ex)
        {
            Trace.TraceError(ex.ToString());
            return 4;
        }
        /*catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            return (1, "Failed to extract EVT due to unhandled exception: '" + ex.ToString() + "'");
        }*/

        if (this.Config.ActiveEventId is null)
            return 5;

        return 0;
    }

}
