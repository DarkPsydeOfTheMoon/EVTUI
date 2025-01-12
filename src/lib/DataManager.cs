using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace EVTUI;

public class DataManager
{

    /////////////////////////////
    // *** PRIVATE MEMBERS *** //
    /////////////////////////////
    private static Regex CpkPattern = new Regex("\\.CPK$", RegexOptions.IgnoreCase);

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public ProjectManager ProjectManager;
    public EventManager   EventManager;
    public ScriptManager  ScriptManager;
    public AudioManager   AudioManager;

    public bool ReadOnly { get; set; }
    public bool ProjectLoaded;
    public bool EventLoaded;

    public List<string> CpkList { get; set; }

    public string? CpkPath;
    public string? ModPath;
    public string VanillaExtractionPath = Path.Combine(UserCache.LocalDir, "Extracted");
    public string WorkingPath = Path.Combine(UserCache.LocalDir, "Working");

    public string? ActiveEventId { get => (this.ProjectManager.ActiveEvent is null) ? null : $"E{this.ProjectManager.ActiveEvent.MajorId:000}_{this.ProjectManager.ActiveEvent.MinorId:000}"; }

    public List<Project>      AllProjects   { get => this.ProjectManager.UserData.Projects; }
    public List<GameSettings> AllGames      { get => this.ProjectManager.UserData.Games;    }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public DataManager()
    {
        if (!Directory.Exists(this.VanillaExtractionPath))
            Directory.CreateDirectory(this.VanillaExtractionPath);
        if (!Directory.Exists(this.WorkingPath))
            Directory.CreateDirectory(this.WorkingPath);

        this.ProjectManager = new ProjectManager();
        this.EventManager   = new EventManager();
        this.ScriptManager  = new ScriptManager();
        this.AudioManager   = new AudioManager();
        this.CpkList        = new List<string>();
        this.Reset();

    }

    public void Reset()
    {
        this.ModPath = null;
        this.CpkPath = null;
        this.ReadOnly      = false;
        this.ProjectLoaded = false;
        this.EventLoaded   = false;
        this.EventManager.Clear();
        this.CpkList.Clear();
        this.ClearCache();
    }

    public void LoadProject(int ind)
    {
        // ActiveProject and ActiveGame should both be set
        this.ProjectManager.LoadProject(ind);
        this.ModPath = this.ProjectManager.ActiveProject.Mod.Path;
        this.CpkPath = this.ProjectManager.ActiveGame.Path;
        this.ReadOnly = false;
        this.ProjectLoaded = true;
    }

    public void LoadGameReadOnly(int ind)
    {
        // ActiveGame should be set, ActiveProject should be null
        this.ProjectManager.LoadGameReadOnly(ind);
        this.ModPath = null;
        this.CpkPath = this.ProjectManager.ActiveGame.Path;
        this.ReadOnly = true;
        this.ProjectLoaded = false;
    }

    public bool LoadEvent(int majorId, int minorId)
    {
        // this shouldn't happen
        if (!this.ReadOnly && !this.ProjectLoaded)
            return false;

        bool success = this.EventManager.Load(this.CpkList, $"E{majorId:000}_{minorId:000}", this.VanillaExtractionPath, this.ProjectManager.ModdedFileDir, this.ProjectManager.CpkDecryptionFunctionName);
        if (success)
            this.ProjectManager.LoadEvent(majorId, minorId);
        this.EventLoaded = success;

        this.ScriptManager.PopulateWorkingDir(this.WorkingPath, this.VanillaExtractionPath, this.ProjectManager.ModdedFileDir, this.ProjectManager.EmulatedFileDir, this.EventManager.BmdPaths, this.EventManager.BfPaths, this.ProjectManager.ActiveGame.Type);

        // TODO: load common files! system sounds, common voice lines, models, bustups, cutins
        // so far, VOICE_SINGLEWORD gets loaded, but the rest will have to wait for full EVT/ECS parsing
        this.AudioManager.UpdateAudioCueFiles(this.EventManager.AcwbPaths, this.VanillaExtractionPath, this.ScriptManager.EventCues);

        return true;
    }

    public string CompileMessage(string fileBase)
    {
        return this.ScriptManager.CompileMessage(this.WorkingPath, fileBase);
    }

    public string CompileScript(string fileBase)
    {
        return this.ScriptManager.CompileScript(this.WorkingPath, fileBase);
    }

    public List<string> GetCPKsFromPath(string? directoryPath)
    {
        var cpks = new List<string>();
        if (!(directoryPath is null))
            foreach (var file in Directory.GetFiles(directoryPath))
                if (DataManager.CpkPattern.IsMatch(file))
                    cpks.Add(file);
        return cpks;
    }

    public List<SimpleEvent> ListAllEvents()
    {
        List<SimpleEvent> ret = new List<SimpleEvent>();
        foreach (var tuple in CPKExtract.ListAllEvents(this.CpkList, this.ProjectManager.CpkDecryptionFunctionName))
        {
            SimpleEvent evt = new SimpleEvent();
            evt.MajorId = tuple.MajorId;
            evt.MinorId = tuple.MinorId;
            ret.Add(evt);
        }
        return ret;
    }

    public void ClearCache()
    {
        CPKExtract.ClearDirectory(this.VanillaExtractionPath);
        CPKExtract.ClearDirectory(this.WorkingPath);
    }

    public void SaveBF()
    {
        this.ScriptManager.SaveScript("BF", this.WorkingPath, this.ProjectManager.ModdedFileDir, this.ProjectManager.HasFramework("BFEmulator") ? this.ProjectManager.EmulatedFileDir : null);
    }

    public void SaveBMD()
    {
        this.ScriptManager.SaveScript("BMD", this.WorkingPath, this.ProjectManager.ModdedFileDir, this.ProjectManager.HasFramework("BMDEmulator") ? this.ProjectManager.EmulatedFileDir : null);
    }

    public void SaveModdedFiles(bool evt, bool ecs, bool bmd, bool bf)
    {
        if (this.ReadOnly)
            return;

        if (evt)
        {
            if (!this.EventManager.EvtPath.StartsWith(this.ProjectManager.ModdedFileDir))
            {
                string baseEvtPath = this.EventManager.EvtPath.Substring((this.VanillaExtractionPath.Length+1), this.EventManager.EvtPath.Length-(this.VanillaExtractionPath.Length+1));
                this.EventManager.EvtPath = Path.Combine(this.ProjectManager.ModdedFileDir, baseEvtPath);
            }
            (new FileInfo(this.EventManager.EvtPath)).Directory.Create();
            this.EventManager.SaveEVT();
        }

        if (ecs)
        {
            if (!this.EventManager.EcsPath.StartsWith(this.ProjectManager.ModdedFileDir))
            {
                string baseEcsPath = this.EventManager.EcsPath.Substring((this.VanillaExtractionPath.Length+1), this.EventManager.EcsPath.Length-(this.VanillaExtractionPath.Length+1));
                this.EventManager.EcsPath = Path.Combine(this.ProjectManager.ModdedFileDir, baseEcsPath);
            }
            (new FileInfo(this.EventManager.EcsPath)).Directory.Create();
            this.EventManager.SaveECS();
        }

        if (bmd)
            this.SaveBMD();

        if (bf)
            this.SaveBF();
    }

}
