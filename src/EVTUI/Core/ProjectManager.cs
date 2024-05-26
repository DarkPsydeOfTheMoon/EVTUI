using System;
using System.Collections.Generic;
using System.IO;

namespace EVTUI;

public class ProjectManager
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public User UserData;

    // the below wrappers keep the "latest project/cpk/event is always index zero"
    // thing contained to this class, so if that's confusing and/or changes in the future
    // it doesn't need to affect a bunch of other classes.

    public Project? LatestProject
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
                return this.UserData.Projects[0].Immutable.Game.Path;
            else
                return null;
        }
    }

    public string? LatestProjectModPath
    {
        get
        {
            if (this.UserData.Projects.Count > 0)
                return this.UserData.Projects[0].Immutable.Mod.Path;
            else
                return null;
        }
    }

    public Event? LatestProjectEvent
    {
        get
        {
            if (this.UserData.Projects.Count > 0 && this.UserData.Projects[0].History.Events.Count > 0)
                return this.UserData.Projects[0].History.Events[0];
            else
                return null;
        }
    }

    public string? LatestReadOnlyGamePath
    {
        get
        {
            if (this.UserData.ReadOnly.History.CPKs.Count > 0)
                return this.UserData.ReadOnly.History.CPKs[0];
            else
                return null;
        }
    }

    public Event? LatestReadOnlyEvent
    {
        get
        {
            if (this.UserData.ReadOnly.History.Events.Count > 0)
                return this.UserData.ReadOnly.History.Events[0];
            else
                return null;
        }
    }

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
        //    Console.WriteLine(this.ActiveProject.Mutable.Name);
        //this.LoadProject(1);
        //if (!(this.ActiveProject is null))
        //    Console.WriteLine(this.ActiveProject.Mutable.Name);
    }

    private void SaveUserCache()
    {
        UserCache.SaveToYaml(this.UserData);
    }

    public bool ModPathAlreadyUsed(string modPath)
    {
        foreach (Project oldProject in this.UserData.Projects)
            if (oldProject.Immutable.Mod.Path == modPath)
                return true;
        return false;
    }

    public bool TryUpdateProjects(string gamePath, string gameType, string modPath, string modType, string name, Dictionary<string, bool> frameworks, List<string> loadOrder)
    {
        // just gonna treat these as one for error catching purposes... sure hope i don't regret this later when debugging
        if (this.ModPathAlreadyUsed(modPath) || !Directory.Exists(gamePath))
            return false;

        GameSettings game = new GameSettings();
        game.Path = gamePath;
        game.Type = gameType;

        ModSettings mod = new ModSettings();
        mod.Path = modPath;
        mod.Type = modType;

        ImmutableSettings immutable = new ImmutableSettings();
        immutable.Game = game;
        immutable.Mod = mod;

        MutableSettings mutable = new MutableSettings();
        mutable.Name = name;
        mutable.Frameworks = frameworks;
        mutable.LoadOrder = loadOrder;

        ProjectHistory history = new ProjectHistory();
        history.Events = new List<Event>();

        Project project = new Project();
        project.Immutable = immutable;
        project.Mutable = mutable;
        project.History = history;

        this.UserData.Projects.Insert(0, project);
        this.SaveUserCache();

        return true;
    }

    public void UpdateProjectEvents(int ind, int majorId, int minorId)
    {
        (this.UserData.Projects[ind].History.Events).RemoveAll(evt => evt.MajorId == majorId && evt.MinorId == minorId);
        Event newEvent = new Event();
        newEvent.MajorId = majorId;
        newEvent.MinorId = minorId;
        this.UserData.Projects[ind].History.Events.Insert(0, newEvent);
        this.SaveUserCache();
    }

    // ensure / break if ind is out of bounds / there are no projects...?
    public void LoadProject(int ind)
    {
        if (ind == 0)
            return;

        Project project = this.UserData.Projects[ind];
        this.UserData.Projects.RemoveAt(ind);
        this.UserData.Projects.Insert(0, project);
        this.SaveUserCache();
    }

    public void SetProjectName(int ind, string name)
    {
        this.UserData.Projects[ind].Mutable.Name = name;
        this.SaveUserCache();
    }

    public void SetProjectFramework(int ind, string key, bool val)
    {
        this.UserData.Projects[ind].Mutable.Frameworks[key] = val;
        this.SaveUserCache();
    }

    public void SetProjectLoadOrder(int ind, List<string> loadOrder)
    {
        this.UserData.Projects[ind].Mutable.LoadOrder = loadOrder;
        this.SaveUserCache();
    }

    public void UpdateReadOnlyCPKs(string path)
    {
        (this.UserData.ReadOnly.History.CPKs).RemoveAll(cpk => cpk == path);
        this.UserData.ReadOnly.History.CPKs.Insert(0, path);
        this.SaveUserCache();
    }

    public void UpdateReadOnlyEvents(int majorId, int minorId)
    {
        (this.UserData.ReadOnly.History.Events).RemoveAll(evt => evt.MajorId == majorId && evt.MinorId == minorId);
        Event newEvent = new Event();
        newEvent.MajorId = majorId;
        newEvent.MinorId = minorId;
        this.UserData.ReadOnly.History.Events.Insert(0, newEvent);
        this.SaveUserCache();
    }
}
