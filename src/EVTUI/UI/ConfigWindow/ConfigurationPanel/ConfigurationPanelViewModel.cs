using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;

using Avalonia.Threading;

namespace EVTUI.ViewModels;

public class DisplayableProject
{
    public DisplayableProject(Project project, int ind)
    {
        Name = project.Mutable.Name;
        GamePath = project.Immutable.Game.Path;
        ModPath = project.Immutable.Mod.Path;
        Ind = ind;
    }

    public string Name     { get; set; }
    public string GamePath { get; set; }
    public string ModPath  { get; set; }
    public int Ind         { get; set; }
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
    public string ConfigType;

    public ObservableCollection<DisplayableProject> ProjectList { get; }
    public DisplayableProject ProjectSelection { get; set; }

    public ObservableCollection<DisplayableDirectory> CpkDirList { get; }
    public DisplayableDirectory CpkDirSelection { get; set; }
    public HashSet<string> CpkDirSet;
    public HashSet<string> ModDirSet;
    public Event EventSelection { get; set; }

    public int? EventMajorId { get; set; } = 0;
    public int? EventMinorId { get; set; } = 0;

    public string CpkPath;
    public string DisplayCPKPath
    {
        get 
        {
            if (this.CpkPath is null)
                return "(none)";
            else
                return this.CpkPath;
        }
        set
        {
            this.CpkPath = value; 
            OnPropertyChanged(nameof(DisplayCPKPath));
        }
    }

    public string? ModPath;
    public string DisplayModPath
    {
        get 
        {
            if (this.ModPath is null)
                return "(none)";
            else
                return this.ModPath;
        }
        set
        {
            this.ModPath = value; 
            OnPropertyChanged(nameof(DisplayModPath));
        }
    }

    // all but the name are just placeholders for now
    public string ModName { get; set; } = "";
    public string ModType { get; set; } = "P5R PC (Steam)";
    public string GameType { get; set; } = "Reloaded";
    public bool UseAwbEmu { get; set; } = false;
    public bool UseBfEmu { get; set; } = false;
    public bool UseBgmEmu { get; set; } = false;
    public List<String> ModLoadOrder { get; set; } = new List<string>(["<PRIMARY_MOD_PLACEHOLDER>"]);

    public string DisplayLoadedEvent
    {
        get 
        {
            if (this.Config.ActiveEventId is null)
                return "(none)";
            else
                return this.Config.ActiveEventId;
        }
    }

    public ObservableCollection<Event> EventList
    {
        get
        {
            if (this.ConfigType == "read-only")
                return new ObservableCollection<Event>(this.Config.AllReadOnlyEvents);
            else if (!(this.Config.ActiveProject is null))
                return new ObservableCollection<Event>(this.Config.ActiveProject.History.Events);
            else
                return null;
        }
    }

    public bool AnyRecentProjects
    {
        get
        {
            if (this.ConfigType == "read-only")
                return false;
            else
                return (this.Config.AllProjects.Count > 0);
        }
    }
    public bool NoRecentProjects { get { return !this.AnyRecentProjects; } }

    public bool AnyRecentCpkDirs
    {
        get
        {
            if (this.CpkDirList is null)
                 return false;
            else
                 return (this.CpkDirList.Count > 0);
        }
    }
    public bool NoRecentCpkDirs { get { return !this.AnyRecentCpkDirs; } }

    public bool AnyRecentEvents
    {
        get
        {
            if (this.ConfigType == "read-only")
                return (this.Config.AllReadOnlyEvents.Count > 0);
            else if (!(this.Config.ActiveProject is null))
                return (this.Config.ActiveProject.History.Events.Count > 0);
            else
                return false;
        }
    }
    public bool NoRecentEvents { get { return !this.AnyRecentEvents; } }

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

        if (this.ConfigType == "open-proj")
        {
            this.Config.ReadOnly = false;
            List<DisplayableProject> projectList = new List<DisplayableProject>();
            for (int i=0; i<this.Config.AllProjects.Count; i++)
                projectList.Add(new DisplayableProject(this.Config.AllProjects[i], i));
            this.ProjectList = new ObservableCollection<DisplayableProject>(projectList);
        }
        else
        {
            if (this.ConfigType == "new-proj")
                this.Config.ReadOnly = false;
            else if (this.ConfigType == "read-only")
                this.Config.ReadOnly = true;
            // I was originally splitting the project vs. read-only CPK histories, but eh
            // ...it's just more convenient to always show both in any recent files tables
            List<DisplayableDirectory> cpkList = new List<DisplayableDirectory>();
            this.CpkDirSet = new HashSet<string>();
            foreach (Project project in this.Config.AllProjects)
            {
                if (!(this.CpkDirSet.Contains(project.Immutable.Game.Path)))
                {
                    cpkList.Add(new DisplayableDirectory(project.Immutable.Game.Path));
                    this.CpkDirSet.Add(project.Immutable.Game.Path);
                }
            }
            foreach (string cpkDir in this.Config.AllReadOnlyCPKs)
            {
                if (!(this.CpkDirSet.Contains(cpkDir)))
                {
                    cpkList.Add(new DisplayableDirectory(cpkDir));
                    this.CpkDirSet.Add(cpkDir);
                }
            }
            this.CpkDirList = new ObservableCollection<DisplayableDirectory>(cpkList);
        }
    }
    
    public ICommand UseSelectedCpkDir { get { return ReactiveCommand.CreateFromTask(async () => 
    {
        await Task.Run(() => SetCPKsHelper(true));
    });}}

    public ICommand SetCPKs { get { return ReactiveCommand.CreateFromTask(async () => 
    {
        await Task.Run(() => SetCPKsHelper(false));
    });}}

    public async void SetCPKsHelper(bool fromSelection)
    {
        var cpks = new List<string>();  
        while (cpks.Count <= 0)
        {
            // Yeah, you shouldn't use exceptions for control flow, whatever...
            string? cpkdir = null;
            if (fromSelection)
            {
                if (this.ConfigType == "open-proj")
                {
                    if (this.ProjectSelection is null)
                    {
                        await Dispatcher.UIThread.InvokeAsync(async () =>
                            { await DisplayMessage.Handle("No project selected."); });
                        return;
                    }
                    else
                        cpkdir = this.ProjectSelection.GamePath;
                }
                else if (this.CpkDirSelection is null)
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                        { await DisplayMessage.Handle("No folder selected."); });
                    return;
                }
                else
                    cpkdir = this.CpkDirSelection.Directory;
            }
            else
                cpkdir = await GetCPKDirectoryFromView.Handle(Unit.Default);
            try
            {
                cpks = this.Config.GetCPKsFromPath(cpkdir);
                if (cpks.Count <= 0)
                {
                    bool tryagain = await Dispatcher.UIThread.InvokeAsync(async () =>
                        { return await DisplayMessage.Handle("No CPKs in selected folder."); });
                    if (tryagain)
                        continue;
                    else
                        break;
                }
                this.DisplayCPKPath = cpkdir;
                this.Config.CpkList = cpks;
            }
            catch (NullReferenceException)     { break; }
            catch (DirectoryNotFoundException) { await Dispatcher.UIThread.InvokeAsync(async () =>
                { await DisplayMessage.Handle("'" + cpkdir + "' is not a directory."); } ); }
        }
        return;
    }

    public ICommand SetModDir { get { return ReactiveCommand.CreateFromTask(async () => 
    {
        string? maybedir = null;
        while (true)
        {
            maybedir = await GetModDirectoryFromView.Handle(Unit.Default);
            if (maybedir is null)
                return;
            else if (this.Config.ProjectManager.ModPathAlreadyUsed(maybedir))
                await this.DisplayMessage.Handle("Selected mod directory is used in another project and cannot be reused.");
            else
                break;
        }
        this.ModPath = maybedir;
        OnPropertyChanged(nameof(DisplayModPath));
    });}}

    public ICommand CreateProject { get { return ReactiveCommand.CreateFromTask(async () => 
    {
        if (this.ModName is null || this.ModName == "")
        {
            await this.DisplayMessage.Handle("Project name hasn't been set.");
            return;
        }
        if (this.CpkPath is null || this.CpkPath == "")
        {
            await this.DisplayMessage.Handle("Game (CPK) folder hasn't been set.");
            return;
        }
        if (this.ModPath is null || this.ModPath == "")
        {
            await this.DisplayMessage.Handle("Mod folder hasn't been set.");
            return;
        }
        bool projectSuccess = this.Config.ProjectManager.TryUpdateProjects(this.CpkPath, this.GameType, this.ModPath, this.ModType, this.ModName, (new Dictionary<string, bool>{{"AWBEmulator", this.UseAwbEmu}, {"BFEmulator", this.UseBfEmu}, {"BGME", this.UseBgmEmu}}), this.ModLoadOrder);
        if (!projectSuccess)
        {
            await this.DisplayMessage.Handle("Something is wrong with the provided folders. Project could not be created.");
            return;
        }
        this.Config.LoadProject(0);
        OnPropertyChanged(nameof(this.EventList));
        OnPropertyChanged(nameof(this.AnyRecentEvents));
        OnPropertyChanged(nameof(this.NoRecentEvents));
        await this.DisplayMessage.Handle($"Loaded project \"{this.Config.ActiveProject.Mutable.Name}\"!");
        await this.OpenEventConfig.Handle(Unit.Default);
    });}}

    public ICommand SetProject { get { return ReactiveCommand.CreateFromTask(async () => 
    {
        if (this.ProjectSelection is null)
        {
            await DisplayMessage.Handle("No project selected.");
            return;
        }
        this.Config.LoadProject(this.ProjectSelection.Ind);
        SetCPKsHelper(true);
        OnPropertyChanged(nameof(this.EventList));
        OnPropertyChanged(nameof(this.AnyRecentEvents));
        OnPropertyChanged(nameof(this.NoRecentEvents));
        await this.DisplayMessage.Handle($"Loaded project \"{this.Config.ActiveProject.Mutable.Name}\"!");
        await this.OpenEventConfig.Handle(Unit.Default);
    });}}

    public ICommand InitReadOnly { get { return ReactiveCommand.CreateFromTask(async () => 
    {
        if (this.CpkPath is null || this.CpkPath == "")
        {
            await this.DisplayMessage.Handle("Game (CPK) folder hasn't been set.");
            return;
        }
        // other issues should already be handled by SetCpks, unless shit's really fucked
        this.Config.ProjectManager.UpdateReadOnlyCPKs(this.CpkPath);
        await this.OpenEventConfig.Handle(Unit.Default);
    });}}

    public ICommand UseSelectedEvent { get { return ReactiveCommand.CreateFromTask(async () => 
    {
        await Task.Run(() => SetEVTHelper(true));
    });}}

    public ICommand SetEVT { get { return ReactiveCommand.CreateFromTask(async () => 
    {
        await Task.Run(() => SetEVTHelper(false));
    });}}

    public async void SetEVTHelper(bool fromSelection)
    {
        if (fromSelection)
        {
            if (this.EventSelection is null)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                    { await DisplayMessage.Handle("No event selected."); });
                return;
            }
            this.EventMajorId = this.EventSelection.MajorId;
            this.EventMinorId = this.EventSelection.MinorId;
            OnPropertyChanged(nameof(this.EventMajorId));
            OnPropertyChanged(nameof(this.EventMinorId));
        }

        try
        {
            bool validLoadAttempt = this.Config.LoadEvent((int)this.EventMajorId, (int)this.EventMinorId);
			if (!validLoadAttempt)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                    { await this.DisplayMessage.Handle("Must have a loaded project or be in read-only mode to load an event."); });
                return;
            }
            else if (!this.Config.EventLoaded)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                    { await this.DisplayMessage.Handle($"Event E{this.EventMajorId:000}_{this.EventMinorId:000} does not exist and could not be loaded."); });
                OnPropertyChanged(nameof(this.DisplayLoadedEvent));
                return;
            }
        }
        catch (Exception ex)
        {
            // I think this should only throw if some really wacky OS stuff happens.
            await Dispatcher.UIThread.InvokeAsync(async () =>
                { await this.DisplayMessage.Handle("Failed to extract EVT due to unhandled exception: '" + ex.ToString() + "'"); });
            return;
        }

        OnPropertyChanged(nameof(this.DisplayLoadedEvent));
        await Dispatcher.UIThread.InvokeAsync(async () =>
            { await this.DisplayMessage.Handle($"Loaded event {this.Config.ActiveEventId}!"); });
    }

    public ICommand FinishConfigStartEdit { get { return ReactiveCommand.CreateFromTask(async () =>
    {
        if (this.Config.ActiveEventId is null)
        {
            await this.DisplayMessage.Handle("No event loaded.");
            return;
        }
        await this.FinishConfig.Handle(0);
        return;
    });}}

    // View Interactions
    public Interaction<Unit,   string?> GetCPKDirectoryFromView { get; } = new Interaction<Unit,   string?>();
    public Interaction<Unit,   string?> GetModDirectoryFromView { get; } = new Interaction<Unit,   string?>();
    public Interaction<string, bool   > DisplayMessage          { get; } = new Interaction<string, bool   > ();
    public Interaction<Unit,   bool   > OpenEventConfig         { get; } = new Interaction<Unit,   bool>();
    public Interaction<int?,   bool   > FinishConfig            { get; } = new Interaction<int?,   bool>();

}
