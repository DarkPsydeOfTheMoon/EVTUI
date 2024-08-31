using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ReactiveUI;
//using ReactiveUI.Fody.Helpers;

namespace EVTUI.ViewModels;

public class DisplayableProject
{
    public DisplayableProject(Project project, int ind)
    {
        Name     = project.Mutable.Name;
        GamePath = project.Immutable.Game.Path;
        ModPath  = project.Immutable.Mod.Path;
        Ind      = ind;
    }

    public string Name     { get; set; }
    public string GamePath { get; set; }
    public string ModPath  { get; set; }
    public int    Ind      { get; set; }
}

public class DisplayableDirectory
{
    public DisplayableDirectory(string directory)
    {
        Directory = directory;
    }

    public string Directory { get; set; }
}

public class ConfigurationPanelViewModel : ViewModelBase
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public DataManager Config;
    public string      ConfigType;

    public HashSet<string> CpkDirSet;
    public HashSet<string> ModDirSet;

    public string CpkPath;
    public string DisplayCPKPath { get => (this.CpkPath is null) ? "(none)" : this.CpkPath; }

    public string? ModPath;
    public string DisplayModPath { get => (this.ModPath is null) ? "(none)" : this.ModPath; }

    // all but the name are just placeholders for now
    public string ModName   { get; set; } = "";
    public string ModType   { get; set; } = "Reloaded";
    public string GameType  { get; set; } = "P5R PC (Steam)";
    public bool   UseAwbEmu { get; set; } = false;
    public bool   UseBfEmu  { get; set; } = false;
    public bool   UseBgmEmu { get; set; } = false;
    public List<String> ModLoadOrder { get; set; } = new List<string>(["<PRIMARY_MOD_PLACEHOLDER>"]);

    ////////////////////////////////
    // *** OBSERVABLE MEMBERS *** //
    ////////////////////////////////
    public ObservableCollection<DisplayableDirectory> CpkDirList      { get; }
    public DisplayableDirectory                       CpkDirSelection { get; set; }
    public bool AnyRecentCpkDirs { get => (!(this.CpkDirList is null) && this.CpkDirList.Count > 0); }
    public bool NoRecentCpkDirs { get { return !this.AnyRecentCpkDirs; } }

    public ObservableCollection<DisplayableProject> ProjectList      { get; }
    public DisplayableProject                       ProjectSelection { get; set; }
    public bool AnyRecentProjects { get => (this.Config.AllProjects.Count > 0); }
    public bool NoRecentProjects { get => !this.AnyRecentProjects; }

    private ObservableCollection<Event> _eventList;
    public ObservableCollection<Event> EventList
    {
        get => _eventList;
        set => this.RaiseAndSetIfChanged(ref _eventList, value);
    }
    public bool AnyRecentEvents { get => (!(this.EventList is null) && this.EventList.Count > 0); }
    public bool NoRecentEvents  { get => !this.AnyRecentEvents; }

    private Event _eventSelection;
    public Event EventSelection
    {
        get => _eventSelection;
        set => this.RaiseAndSetIfChanged(ref _eventSelection, value);
    }
    public int? EventMajorId { get; set; } = 0;
    public int? EventMinorId { get; set; } = 0;

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public ConfigurationPanelViewModel(DataManager dataManager, string configtype)
    {
        this.Config = dataManager;
        this.ConfigType = configtype;

        this.ModDirSet = new HashSet<string>();
        foreach (Project project in this.Config.AllProjects)
            this.ModDirSet.Add(project.Immutable.Mod.Path);

        this.ProjectList = new ObservableCollection<DisplayableProject>();
        this.CpkDirList = new ObservableCollection<DisplayableDirectory>();
        _eventList = new ObservableCollection<Event>();

        if (this.ConfigType == "open-proj")
        {
            this.Config.ReadOnly = false;
            for (int i=0; i<this.Config.AllProjects.Count; i++)
                this.ProjectList.Add(new DisplayableProject(this.Config.AllProjects[i], i));
        }
        else
        {
            if (this.ConfigType == "new-proj")
                this.Config.ReadOnly = false;
            else if (this.ConfigType == "read-only")
                this.Config.ReadOnly = true;
            // I was originally splitting the project vs. read-only CPK histories, but eh
            // ...it's just more convenient to always show both in any recent files tables
            this.CpkDirSet = new HashSet<string>();
            foreach (Project project in this.Config.AllProjects)
            {
                if (!(this.CpkDirSet.Contains(project.Immutable.Game.Path)))
                {
                    this.CpkDirList.Add(new DisplayableDirectory(project.Immutable.Game.Path));
                    this.CpkDirSet.Add(project.Immutable.Game.Path);
                }
            }
            foreach (string cpkDir in this.Config.AllReadOnlyCPKs)
            {
                if (!(this.CpkDirSet.Contains(cpkDir)))
                {
                    this.CpkDirList.Add(new DisplayableDirectory(cpkDir));
                    this.CpkDirSet.Add(cpkDir);
                }
            }
        }

        this.WhenAnyValue(x => x.ProjectList).Subscribe(x =>
        {
            this.ProjectSelection = null;
            if (this.ProjectList.Count > 0)
                this.ProjectSelection = this.ProjectList[0];
            OnPropertyChanged(nameof(ProjectList));
            OnPropertyChanged(nameof(ProjectSelection));
            OnPropertyChanged(nameof(AnyRecentProjects));
            OnPropertyChanged(nameof(NoRecentProjects));
        });

        this.WhenAnyValue(x => x.CpkDirList).Subscribe(x =>
        {
            this.CpkDirSelection = null;
            if (this.CpkDirList.Count > 0)
                this.CpkDirSelection = this.CpkDirList[0];
            OnPropertyChanged(nameof(CpkDirList));
            OnPropertyChanged(nameof(CpkDirSelection));
            OnPropertyChanged(nameof(AnyRecentCpkDirs));
            OnPropertyChanged(nameof(NoRecentCpkDirs));
        });

        this.WhenAnyValue(x => x.EventList).Subscribe(x =>
        {
            this.EventSelection = null;
            if (this.EventList.Count > 0)
                this.EventSelection = this.EventList[0];
            OnPropertyChanged(nameof(EventList));
            OnPropertyChanged(nameof(EventSelection));
            OnPropertyChanged(nameof(AnyRecentEvents));
            OnPropertyChanged(nameof(NoRecentEvents));
        });
        if (this.ConfigType == "read-only")
            this.EventList = new ObservableCollection<Event>(this.Config.AllReadOnlyEvents);

    }
    
    public bool TrySetCPKs(string cpkdir)
    {
        List<string> cpks = this.Config.GetCPKsFromPath(cpkdir);
        if (cpks.Count <= 0)
            return false;

        this.CpkPath =  cpkdir;
        OnPropertyChanged(nameof(DisplayCPKPath));
        this.Config.CpkList = cpks;
        return true;
    }

    public (int Status, string Message) TrySetModDir(string maybedir)
    {
        if (this.Config.ProjectManager.ModPathAlreadyUsed(maybedir))
            return (1, "Selected mod directory is used in another project and cannot be reused.");
        this.ModPath = maybedir;
        OnPropertyChanged(nameof(DisplayModPath));
        return (0, null);
    }

    public (int Status, string Message) TryCreateProject()
    {
        if (this.ModName is null || this.ModName == "")
            return (1, "Project name hasn't been set.");
        if (this.CpkPath is null || this.CpkPath == "")
            return (1, "Game (CPK) folder hasn't been set.");
        if (this.ModPath is null || this.ModPath == "")
            return (1, "Mod folder hasn't been set.");

        bool projectSuccess = this.Config.ProjectManager.TryUpdateProjects(this.CpkPath, this.GameType, this.ModPath, this.ModType, this.ModName, (new Dictionary<string, bool>{{"AWBEmulator", this.UseAwbEmu}, {"BFEmulator", this.UseBfEmu}, {"BGME", this.UseBgmEmu}}), this.ModLoadOrder);
        if (!projectSuccess)
            return (1, "Something is wrong with the provided folders. Project could not be created.");

        this.Config.LoadProject(0);

        if (this.Config.ActiveProject is null)
            return (1, "Failed to load project for some reason.");

        this.EventList = new ObservableCollection<Event>(this.Config.ActiveProject.History.Events);
        return (0, $"Loaded project \"{this.Config.ActiveProject.Mutable.Name}\"!");
    }

    public (int Status, string Message) TryUseCPKDir(string cpkdir)
    {
        if (cpkdir is null)
        {
            if (this.CpkDirSelection is null)
                return (1, "No CPK folder selected.");
            if (this.CpkDirSelection.Directory is null)
                return (1, "CPK folder selection is invalid.");
            cpkdir = this.CpkDirSelection.Directory;
        }

        if (!this.TrySetCPKs(cpkdir))
            return (1, "No CPKs in selected folder.");

        // set CpkPath...?
        if (this.Config.ReadOnly)
            this.Config.ProjectManager.UpdateReadOnlyCPKs(cpkdir);
        return (0, null);
    }

    public (int Status, string Message) TryLoadProject()
    {
        if (this.ProjectSelection is null)
            return (1, "No project selected.");
        if (this.ProjectSelection.GamePath is null)
            return (1, "Project has no game path set.");

        if (!this.TrySetCPKs(this.ProjectSelection.GamePath))
            return (1, "No CPKs in selected folder.");

        this.Config.LoadProject(this.ProjectSelection.Ind);
        if (this.Config.ActiveProject is null)
            return (1, "No project loaded.");

        this.EventList = new ObservableCollection<Event>(this.Config.ActiveProject.History.Events);
        return (0, $"Loaded project \"{this.Config.ActiveProject.Mutable.Name}\"!");
    }

    public (int Status, string Message) TryLoadEvent(bool fromSelection)
    {
        if (fromSelection)
        {
            if (this.EventSelection is null)
                return (1, "No event selected.");
            this.EventMajorId = this.EventSelection.MajorId;
            this.EventMinorId = this.EventSelection.MinorId;
            OnPropertyChanged(nameof(this.EventMajorId));
            OnPropertyChanged(nameof(this.EventMinorId));
        }

        try
        {
            bool validLoadAttempt = this.Config.LoadEvent((int)this.EventMajorId, (int)this.EventMinorId);
            if (!validLoadAttempt)
                return (1, "Must have a loaded project or be in read-only mode to load an event.");
            else if (!this.Config.EventLoaded)
                return (1, $"Event E{this.EventMajorId:000}_{this.EventMinorId:000} does not exist and could not be loaded.");
        }
        catch (Exception ex)
        {
            return (1, "Failed to extract EVT due to unhandled exception: '" + ex.ToString() + "'");
        }

        if (this.Config.ActiveEventId is null)
            return (1, "No event loaded.");

        return (0, $"Loaded event {this.Config.ActiveEventId}!");
    }

}
