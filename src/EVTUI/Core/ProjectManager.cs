using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace EVTUI;

/*public class User
{
    public User(SerialUser serialUser)
    {
        this.Games = new Dictionary<string, GameWrapper>();
        foreach (ExpandedGameSettings game in serialUser.Games)
            this.Games[game.Path] = new Game(game);

        this.Projects = new Dictionary<string, ProjectWrapper>();
        foreach (SerialProject project in serialUser.Projects)
            this.Projects[project.Mod.Path] = new ProjectWrapper(project, this.Games[project.Game.Path]);

        this.Preferences = serialUser.Preferences;
    }

    public Dictionary<string, ProjectWrapper> Projects    { get; set; }
    public Dictionary<string, GameWrapper>    Games       { get; set; }
    public Dictionary<string, object>         Preferences { get; set; }

    public SerialUser Serialize()
    {
        SerialUser ret = new SerialUser();
        return ret;
    }
}

public class GameWrapper
{
    public GameWrapper(ExpandedGameSettings game)
    {
        this.GameSettings  = game;

        this.EventNotes = new Dictionary<(int MajorId, int MinorId), string>();
        foreach (ExpandedEvent evt in project.Events.Notes)
            this.EventNotes[(evt.MajorId, evt.MinorId)] = evt.Text;
    }

    public ExpandedGameSettings                           GameSettings { get; set; }
    public Dictionary<(int MajorId, int MinorId), string> EventNotes   { get; set; }

    public ExpandedGameSettings Serialize()
    {
        ExpandedGameSettings ret = new ExpandedGameSettings();
        return ret;
    }
}

public class ProjectWrapper
{
    public ProjectWrapper(SerialProject project, ExpandedGameSettings game)
    {
        this.GameSettings  = game;
        this.Project       = project;

        this.EventNotes = new Dictionary<(int MajorId, int MinorId), string>();
        foreach (ExpandedEvent evt in project.Events.Notes)
            this.EventNotes[(evt.MajorId, evt.MinorId)] = evt.Text;
    }

    public ExpandedGameSettings                           GameSettings { get; set; }
    public SerialProject                                  Project      { get; set; }
    public Dictionary<(int MajorId, int MinorId), string> EventNotes   { get; set; }

    public SerialProject Serialize()
    {
        SerialProject ret = new SerialProject();
        return ret;
    }
}*/

/*public class DisplayableEvent
{
    public Event(int majorId, int minorId, bool gamePin, bool projPin, string gameNotes, string projNotes)
    {
        this.MajorId   = majorId;
        this.MinorId   = minorId;
        this.GamePin   = gamePin;
        this.ProjPin   = projPin;
        this.GameNotes = gameNotes;
        this.ProjNotes = projNotes;
    }

    public int    MajorId   { get; set; }
    public int    MinorId   { get; set; }
    public bool   GamePin   { get; set; }
    public bool   ProjPin   { get; set; }
    public string GameNotes { get; set; }
    public string ProjNotes { get; set; }
}*/

public class NewProjectConfig
{
    public NewProjectConfig()
    {
        this.Name = "";
        this.ModType = "Reloaded";
        this.GameType = "P5R PC (Steam)";
        this.Frameworks = new List<Framework>();
        this.Frameworks.Add(new Framework("P5REssentials", true));
        this.Frameworks.Add(new Framework("AWBEmulator", false));
        this.Frameworks.Add(new Framework("BFEmulator", false));
        this.Frameworks.Add(new Framework("BMDEmulator", false));
        this.Frameworks.Add(new Framework("Ryo", false));
        this.LoadOrder = new List<string>(["<PRIMARY_MOD_PLACEHOLDER>"]);
    }

    public string  GamePath { get; set; }
    public string? ModPath  { get; set; }

    public string Name      { get; set; }
    public string ModType   { get; set; }
    public string GameType  { get; set; }

    public List<Framework> Frameworks { get; set; }
    public List<string>    LoadOrder  { get; set; }
}

public class Framework
{
    public Framework(string name, bool used)
    {
        this.Name = name;
        this.Used = used;
    }

    public string Name { get; set; }
    public bool   Used { get; set; }
}

public class ProjectManager
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public User UserData;

    private static Dictionary<string, ulong> KeyCodes = new Dictionary<string, ulong>()
    {
        ["P5R PC (Steam)"] = 9923540143823782,
        ["P5 Dev Build"]   = 0,
    };

    // the below wrappers keep the "latest project/cpk/event is always index zero"
    // thing contained to this class, so if that's confusing and/or changes in the future
    // it doesn't need to affect a bunch of other classes.

    public GameSettings? ActiveGame;
    public Project?      ActiveProject;
    public SimpleEvent?  ActiveEvent;

    public ulong AdxKey { get => (this.ActiveGame is null) ? 0 : ProjectManager.KeyCodes[this.ActiveGame.Type]; }

    /*public Project? LatestProject
    {
        get
        {
            if (this.UserData.Projects.Count > 0)
                return this.UserData.Projects[0];
            else
                return null;
        }
    }

    public string? LatestProjectGamePath
    {
        get
        {
            if (this.UserData.Projects.Count > 0)
                return this.UserData.Projects[0].Game;
            else
                return null;
        }
    }

    public string? LatestProjectModPath
    {
        get
        {
            if (this.UserData.Projects.Count > 0)
                return this.UserData.Projects[0].Mod.Path;
            else
                return null;
        }
    }

    public SimpleEvent? LatestProjectEvent
    {
        get
        {
            if (this.UserData.Projects.Count > 0 && this.UserData.Projects[0].Events.Recent.Count > 0)
                return this.UserData.Projects[0].Events.Recent[0];
            else
                return null;
        }
    }

    public string? LatestReadOnlyGamePath
    {
        get
        {
            if (this.UserData.Games.Count > 0)
                return this.UserData.Games[0].Path;
            else
                return null;
        }
    }

    public SimpleEvent? LatestReadOnlyEvent
    {
        get
        {
            // TODO: this is not robust
            if (this.UserData.Games[0].Events.Recent.Count > 0)
                return this.UserData.Games[0].Events.Recent[0];
            else
                return null;
        }
    }*/

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public ProjectManager()
    {
        this.UserData = UserCache.InitializeOrLoadUser();

        // TODO: make these into unit tests
        //this.UpdateReadOnlyEvents(726, 84);
        //this.UpdateReadOnlyCPKs("/path/to/CPKs");
        //this.TryUpdateProjects("/path/to/CPKs", "P5R PC (Steam)", "/path/to/MyMod", "Reloaded", "My First Mod", new Dictionary<string, bool>{["AWBEmulator"]=false, ["BFEmulator"]=false, ["BGME"]=false}, new List<string>(["/path/to/OtherMod", "/path/to/AnotherMod", "<PRIMARY_MOD_PLACEHOLDER>"]));
        //this.TryUpdateProjects("/path/to/CPKs", "P5R PC (Steam)", "/path/to/MyOtherMod", "Reloaded", "My Second Mod", new Dictionary<string, bool>(), new List<string>(["/path/to/OtherMod", "/path/to/AnotherMod", "<PRIMARY_MOD_PLACEHOLDER>"]));
        //this.UpdateProjectEvents(0, 223, 1);
        //this.SetProjectName(0, "My 1st Mod");
        //this.SetProjectFramework(0, "BFEmulator", true);
        //this.SetProjectLoadOrder(0, new List<string>(["<PRIMARY_MOD_PLACEHOLDER>", "/path/to/YetAnotherMod"]));
        //if (!(this.ActiveProject is null))
        //    Console.WriteLine(this.ActiveProject.Name);
        //this.LoadProject(1);
        //if (!(this.ActiveProject is null))
        //    Console.WriteLine(this.ActiveProject.Name);
    }

    private void SaveUserCache()
    {
        UserCache.SaveToYaml(this.UserData);
    }

    public bool ModPathAlreadyUsed(string modPath)
    {
        foreach (Project oldProject in this.UserData.Projects)
            if (oldProject.Mod.Path == modPath)
                return true;
        return false;
    }

    //public bool TryUpdateProjects(string gamePath, string gameType, string modPath, string modType, string name, Dictionary<string, bool> frameworks, List<string> loadOrder)
    public bool TryUpdateProjects(NewProjectConfig config)
    {
        // just gonna treat these as one for error catching purposes... sure hope i don't regret this later when debugging
        if (this.ModPathAlreadyUsed(config.ModPath) || !Directory.Exists(config.GamePath))
            return false;

        //GameSettings game = new GameSettings();
        //game.Path = gamePath;
        //game.Type = gameType;

        ModSettings mod = new ModSettings();
        mod.Path = config.ModPath;
        mod.Type = config.ModType;

        Dictionary<string, bool> frameworks = new Dictionary<string, bool>();
        foreach (Framework framework in config.Frameworks)
            frameworks[framework.Name] = framework.Used;

        Project project    = new Project();
        project.Name       = config.Name;
        project.Game       = config.GamePath;
        project.Mod        = mod;
        project.Frameworks = frameworks;
        project.LoadOrder  = config.LoadOrder;
        project.Events     = new EventCollections();

        this.UserData.Projects.Insert(0, project);
        this.SaveUserCache();

        return true;
    }

    /*public void UpdateProjectEvents(int ind, int majorId, int minorId)
    {
        (this.UserData.Projects[ind].Events.Recent).RemoveAll(evt => evt.MajorId == majorId && evt.MinorId == minorId);
        SimpleEvent newEvent = new SimpleEvent();
        newEvent.MajorId = majorId;
        newEvent.MinorId = minorId;
        this.UserData.Projects[ind].Events.Recent.Insert(0, newEvent);
        this.SaveUserCache();
    }*/

    // there's probably a better way to handle these errors...
    public void LoadProject(int projInd)
    {
        Trace.Assert(projInd >= 0 && projInd < this.UserData.Projects.Count && this.UserData.Projects.Count > 0, "User project data is corrupted.");

        if (projInd > 0)
        {
            Project project = this.UserData.Projects[projInd];
            this.UserData.Projects.RemoveAt(projInd);
            this.UserData.Projects.Insert(0, project);
        }

        this.ActiveGame = null;
        for (int gameInd=0; gameInd<this.UserData.Games.Count; gameInd++)
            if (this.UserData.Games[gameInd].Path == this.UserData.Projects[0].Game)
            {
                if (gameInd > 0)
                {
                    GameSettings game = this.UserData.Games[gameInd];
                    this.UserData.Games.RemoveAt(gameInd);
                    this.UserData.Games.Insert(0, game);
                }
                this.ActiveGame = this.UserData.Games[0];
                this.ActiveProject = this.UserData.Projects[0];
                break;
            }

        Trace.Assert(!(this.ActiveGame is null) && !(this.ActiveProject is null), "Game specified for project doesn't exist.");

        this.SaveUserCache();
    }

    public void LoadGameReadOnly(int gameInd)
    {
        Trace.Assert(gameInd >= 0 && gameInd < this.UserData.Games.Count && this.UserData.Games.Count > 0, "User game path data is corrupted.");

        if (gameInd > 0)
        {
            GameSettings game = this.UserData.Games[gameInd];
            this.UserData.Games.RemoveAt(gameInd);
            this.UserData.Games.Insert(0, game);
            this.SaveUserCache();
        }

        this.ActiveGame = this.UserData.Games[0];
        this.ActiveProject = null;
    }

    public void LoadEvent(int majorId, int minorId)
    {
        if (!(this.ActiveProject is null))
        {
            (this.ActiveProject.Events.Recent).RemoveAll(evt => evt.MajorId == majorId && evt.MinorId == minorId);
            SimpleEvent newEvent = new SimpleEvent();
            newEvent.MajorId = majorId;
            newEvent.MinorId = minorId;
            this.ActiveProject.Events.Recent.Insert(0, newEvent);
            this.ActiveEvent = newEvent;
            this.SaveUserCache();
        }

        if (!(this.ActiveGame is null))
        {
            (this.ActiveGame.Events.Recent).RemoveAll(evt => evt.MajorId == majorId && evt.MinorId == minorId);
            SimpleEvent newEvent = new SimpleEvent();
            newEvent.MajorId = majorId;
            newEvent.MinorId = minorId;
            this.ActiveGame.Events.Recent.Insert(0, newEvent);
            this.ActiveEvent = newEvent;
            this.SaveUserCache();
        }
    }

    public void SetProjectName(int ind, string name)
    {
        this.UserData.Projects[ind].Name = name;
        this.SaveUserCache();
    }

    public void SetProjectNotes(int ind, string notes)
    {
        this.UserData.Projects[ind].Notes = notes;
        this.SaveUserCache();
    }

    public void SetProjectFramework(int ind, string key, bool val)
    {
        this.UserData.Projects[ind].Frameworks[key] = val;
        this.SaveUserCache();
    }

    public void SetProjectLoadOrder(int ind, List<string> loadOrder)
    {
        this.UserData.Projects[ind].LoadOrder = loadOrder;
        this.SaveUserCache();
    }

    public void SetProjectEventNotes(int ind, int majorId, int minorId, string notes)
    {
        (this.UserData.Projects[ind].Events.Notes).RemoveAll(evt => evt.MajorId == majorId && evt.MinorId == minorId);
        if (notes != "")
        {
            ExpandedEvent updatedEvent = new ExpandedEvent();
            updatedEvent.MajorId = majorId;
            updatedEvent.MinorId = minorId;
            updatedEvent.Text    = notes;
            this.UserData.Projects[ind].Events.Notes.Insert(0, updatedEvent);
        }
        this.SaveUserCache();
    }

    public void SetProjectEventNotes(Project project, int majorId, int minorId, string notes)
    {
        (project.Events.Notes).RemoveAll(evt => evt.MajorId == majorId && evt.MinorId == minorId);
        if (notes != "")
        {
            ExpandedEvent updatedEvent = new ExpandedEvent();
            updatedEvent.MajorId = majorId;
            updatedEvent.MinorId = minorId;
            updatedEvent.Text    = notes;
            project.Events.Notes.Insert(0, updatedEvent);
        }
        this.SaveUserCache();
    }

    public void SetGameNotes(int ind, string notes)
    {
        this.UserData.Games[ind].Notes = notes;
        this.SaveUserCache();
    }

    public void SetGameEventNotes(int ind, int majorId, int minorId, string notes)
    {
        (this.UserData.Games[ind].Events.Notes).RemoveAll(evt => evt.MajorId == majorId && evt.MinorId == minorId);
        if (notes != "")
        {
            ExpandedEvent updatedEvent = new ExpandedEvent();
            updatedEvent.MajorId = majorId;
            updatedEvent.MinorId = minorId;
            updatedEvent.Text    = notes;
            this.UserData.Games[ind].Events.Notes.Insert(0, updatedEvent);
        }
        this.SaveUserCache();
    }

    public void SetGameEventNotes(GameSettings game, int majorId, int minorId, string notes)
    {
        (game.Events.Notes).RemoveAll(evt => evt.MajorId == majorId && evt.MinorId == minorId);
        if (notes != "")
        {
            ExpandedEvent updatedEvent = new ExpandedEvent();
            updatedEvent.MajorId = majorId;
            updatedEvent.MinorId = minorId;
            updatedEvent.Text    = notes;
            game.Events.Notes.Insert(0, updatedEvent);
        }
        this.SaveUserCache();
    }

    public void UpdateReadOnlyCPKs(string path, string type, string notes)
    {
        (this.UserData.Games).RemoveAll(game => game.Path == path && game.Type == type);
        GameSettings newGame = new GameSettings();
        newGame.Path = path;
        newGame.Type = type;
        newGame.Notes = notes;
        newGame.Events = new EventCollections();
        this.UserData.Games.Insert(0, newGame);
        this.SaveUserCache();
        this.ActiveGame = newGame;
    }

    /*public void UpdateReadOnlyEvents(int majorId, int minorId)
    {
        (this.UserData.Games[0].Events.Recent).RemoveAll(evt => evt.MajorId == majorId && evt.MinorId == minorId);
        SimpleEvent newEvent = new SimpleEvent();
        newEvent.MajorId = majorId;
        newEvent.MinorId = minorId;
        this.UserData.Games[0].Events.Recent.Insert(0, newEvent);
        this.SaveUserCache();
    }*/

}
