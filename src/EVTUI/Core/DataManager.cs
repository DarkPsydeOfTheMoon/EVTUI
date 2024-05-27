using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace EVTUI;

public class DataManager
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public ProjectManager ProjectManager;
    public EventManager EventManager;

    public bool ReadOnly;
    public bool ProjectLoaded;
    public bool EventLoaded;

    public List<string> CpkList { get; set; }

    public string? CpkPath
    {
        get
        {
            if (!this.ReadOnly && this.ProjectLoaded)
                return this.ProjectManager.LatestProjectGamePath;
            else if (this.ReadOnly)
                return this.ProjectManager.LatestReadOnlyGamePath;
            else
                return null;
        }
    }

    public string? ModPath
    {
        get
        {
            if (!this.ReadOnly && this.ProjectLoaded)
                return this.ProjectManager.LatestProjectModPath;
            else if (this.ReadOnly)
                return Path.Combine(UserCache.LocalDir, "Extracted");
            else
                return null;
        }
    }

    public Project? ActiveProject
    {
        get
        {
            if (!this.ReadOnly && this.ProjectLoaded)
                return this.ProjectManager.LatestProject;
            else
                return null;
        }
    }

    public Event? ActiveEvent
    {
        get
        {
            if (this.EventLoaded)
            {
                if (this.ReadOnly)
                    return this.ProjectManager.LatestReadOnlyEvent;
                else
                    return this.ProjectManager.LatestProjectEvent;
            }
            else
                return null;
        }
    }

    public string? ActiveEventId
    {
        get
        {
            if (this.ActiveEvent is null)
                return null;
            else
                return $"E{this.ActiveEvent.MajorId:000}_{this.ActiveEvent.MinorId:000}";
        }
    }

    public List<Project> AllProjects { get { return this.ProjectManager.UserData.Projects; } }
    public List<Event> AllReadOnlyEvents { get { return this.ProjectManager.UserData.ReadOnly.History.Events; } }
    public List<string> AllReadOnlyCPKs { get { return this.ProjectManager.UserData.ReadOnly.History.CPKs; } }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public DataManager()
    {
        this.ProjectManager = new ProjectManager();
        this.EventManager   = new EventManager();
        this.CpkList        = new List<string>();
        this.Reset();
    }

    public void Reset()
    {
        this.ReadOnly      = false;
        this.ProjectLoaded = false;
        this.EventLoaded   = false;
        this.EventManager.Clear();
        this.CpkList.Clear();
    }

    public void LoadProject(int ind)
    {
        this.ProjectManager.LoadProject(ind);
        this.ReadOnly = false;
        this.ProjectLoaded = true;
    }

    public bool LoadEvent(int majorId, int minorId)
    {
        // this shouldn't happen
        if (!this.ReadOnly && !this.ProjectLoaded)
            return false;

        bool success = this.EventManager.Load(this.CpkList, $"E{majorId:000}_{minorId:000}", this.ModPath);
        if (success)
        {
            if (this.ReadOnly)
                this.ProjectManager.UpdateReadOnlyEvents(majorId, minorId);
            else
                this.ProjectManager.UpdateProjectEvents(0, majorId, minorId);
        }
        this.EventLoaded = success;
		return true;
    }

    public List<string> GetCPKsFromPath(string? directoryPath)
    {
        var cpks = new List<string>();
        if (!(directoryPath is null))
            foreach(var file in Directory.GetFiles(directoryPath))
                if (Regex.IsMatch(file, "\\.[Cc][Pp][Kk]$"))
                    cpks.Add(file);
        return cpks;
    }

}
