using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace EVTUI;

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

    public static List<string> GameTypes { get; } = new List<string> {"P5R PC (Steam)", "P5 Dev Build"};
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
    public string        ModdedFileDir;
    public string        EmulatedFileDir;

    public ulong AdxKey { get => (this.ActiveGame is null) ? 0 : ProjectManager.KeyCodes[this.ActiveGame.Type]; }
    public string CpkDecryptionFunctionName { get => (this.ActiveGame is null || !this.ActiveGame.Type.StartsWith("P5R")) ? null : "P5R"; }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public ProjectManager(User userData)
    {
        this.UserData = userData;
    }

    private void SaveUserCache()
    {
        UserCache.SaveToYaml(this.UserData);
    }

    public bool HasFramework(string name)
    {
        lock (this.UserData)
        {
            return this.ActiveProject.Frameworks.ContainsKey(name) && this.ActiveProject.Frameworks[name];
        }
    }

    public bool ModPathAlreadyUsed(string modPath)
    {
        lock (this.UserData)
        {
            foreach (Project oldProject in this.UserData.Projects)
                if (oldProject.Mod.Path == modPath)
                    return true;
            return false;
        }
    }

    public bool TryUpdateProjects(NewProjectConfig config)
    {
        lock (this.UserData)
        {
            // create the game object if it doesn't already exist
            if (!(this.UserData.Games).Exists(game => game.Path == config.GamePath))
            {
                GameSettings newGame = new GameSettings();
                newGame.Path = config.GamePath;
                newGame.Type = config.GameType;
                newGame.Notes = "";
                newGame.Events = new EventCollections();
                this.UserData.Games.Insert(0, newGame);
            }

            // just gonna treat these as one for error catching purposes... sure hope i don't regret this later when debugging
            if (this.ModPathAlreadyUsed(config.ModPath) || !Directory.Exists(config.GamePath))
                return false;

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
    }

    // there's probably a better way to handle these errors...
    public void LoadProject(int projInd)
    {
        lock (this.UserData)
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

            this.ModdedFileDir = this.ActiveProject.Mod.Path;
            if (this.HasFramework("P5REssentials"))
                this.ModdedFileDir = Path.Combine(this.ModdedFileDir, "P5REssentials", "CPK");
            if (!Directory.Exists(this.ModdedFileDir))
                Directory.CreateDirectory(this.ModdedFileDir);

            if (this.HasFramework("BFEmulator") || this.HasFramework("BMDEmulator"))
            {
                this.EmulatedFileDir = Path.Combine(this.ActiveProject.Mod.Path, "FEmulator");
                if (!Directory.Exists(this.EmulatedFileDir))
                    Directory.CreateDirectory(this.EmulatedFileDir);
                if (this.HasFramework("BFEmulator") && !Directory.Exists(Path.Combine(this.EmulatedFileDir, "BF")))
                    Directory.CreateDirectory(Path.Combine(this.EmulatedFileDir, "BF"));
                if (this.HasFramework("BMDEmulator") && !Directory.Exists(Path.Combine(this.EmulatedFileDir, "BMD")))
                    Directory.CreateDirectory(Path.Combine(this.EmulatedFileDir, "BMD"));
            }
            else
                this.EmulatedFileDir = null;

            this.SaveUserCache();
        }
    }

    public void LoadGameReadOnly(int gameInd)
    {
        lock (this.UserData)
        {
            Trace.Assert(gameInd >= 0 && gameInd < this.UserData.Games.Count && this.UserData.Games.Count > 0, "User game path data is corrupted.");

            if (gameInd > 0)
            {
                GameSettings game = this.UserData.Games[gameInd];
                this.UserData.Games.RemoveAt(gameInd);
                this.UserData.Games.Insert(0, game);
                this.SaveUserCache();
            }

            this.ActiveGame      = this.UserData.Games[0];
            this.ActiveProject   = null;
            this.ModdedFileDir   = null;
            this.EmulatedFileDir = null;
        }
    }

    public void LoadEvent(int majorId, int minorId)
    {
        lock (this.UserData)
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
    }

    public void SetProjectName(int ind, string name)
    {
        lock (this.UserData)
        {
            this.UserData.Projects[ind].Name = name;
            this.SaveUserCache();
        }
    }

    public void SetProjectNotes(int ind, string notes)
    {
        lock (this.UserData)
        {
            this.UserData.Projects[ind].Notes = notes;
            this.SaveUserCache();
        }
    }

    public void SetProjectFramework(int ind, string key, bool val)
    {
        lock (this.UserData)
        {
            this.UserData.Projects[ind].Frameworks[key] = val;
            this.SaveUserCache();
        }
    }

    public void SetProjectLoadOrder(int ind, List<string> loadOrder)
    {
        lock (this.UserData)
        {
            this.UserData.Projects[ind].LoadOrder = loadOrder;
            this.SaveUserCache();
        }
    }

    public void SetProjectPin(Project project, int majorId, int minorId, bool hasPin)
    {
        lock (this.UserData)
        {
            (project.Events.Pinned).RemoveAll(evt => evt.MajorId == majorId && evt.MinorId == minorId);
            if (hasPin)
            {
                SimpleEvent newEvent = new SimpleEvent();
                newEvent.MajorId = majorId;
                newEvent.MinorId = minorId;
                project.Events.Pinned.Insert(0, newEvent);
                this.SaveUserCache();
            }
        }
    }

    public void SetGamePin(GameSettings game, int majorId, int minorId, bool hasPin)
    {
        lock (this.UserData)
        {
            (game.Events.Pinned).RemoveAll(evt => evt.MajorId == majorId && evt.MinorId == minorId);
            if (hasPin)
            {
                SimpleEvent newEvent = new SimpleEvent();
                newEvent.MajorId = majorId;
                newEvent.MinorId = minorId;
                game.Events.Pinned.Insert(0, newEvent);
                this.SaveUserCache();
            }
        }
    }

    public void SetProjectEventNotes(int ind, int majorId, int minorId, string notes)
    {
        lock (this.UserData)
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
    }

    public void SetProjectEventNotes(Project project, int majorId, int minorId, string notes)
    {
        lock (this.UserData)
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
    }

    public void SetGameNotes(int ind, string notes)
    {
        lock (this.UserData)
        {
            this.UserData.Games[ind].Notes = notes;
            this.SaveUserCache();
        }
    }

    public void SetGameEventNotes(int ind, int majorId, int minorId, string notes)
    {
        lock (this.UserData)
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
    }

    public void SetGameEventNotes(GameSettings game, int majorId, int minorId, string notes)
    {
        lock (this.UserData)
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
    }

    public void UpdateReadOnlyCPKs(string path, string type, string notes)
    {
        lock (this.UserData)
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
    }

}
