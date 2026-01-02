using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CriFsV2Lib.Definitions.Structs;

using static EVTUI.Utils;

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

    private List<string> CpkList { get; set; }
    private Dictionary<string, Trie> CpkTries { get; set; }

    public string? CpkPath;
    public string? ModPath;
    public string VanillaExtractionPath = Path.Combine(UserCache.LocalDir, "Extracted");
    private string WorkingPathBase = Path.Combine(UserCache.LocalDir, "Working");
    public string WorkingPath;

    public string? ActiveEventId { get => (this.ProjectManager.ActiveEvent is null) ? null : $"E{this.ProjectManager.ActiveEvent.MajorId:000}_{this.ProjectManager.ActiveEvent.MinorId:000}"; }

    public List<Project>      AllProjects   { get => this.ProjectManager.UserData.Projects; }
    public List<GameSettings> AllGames      { get => this.ProjectManager.UserData.Games;    }

    private HashSet<(string GamePath, string? ModPath, int MajorId, int MinorId)> OpenStuff;

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public DataManager(User userData, HashSet<(string GamePath, string? ModPath, int MajorId, int MinorId)> openStuff)
    {
        if (!Directory.Exists(this.VanillaExtractionPath))
            Directory.CreateDirectory(this.VanillaExtractionPath);

        this.OpenStuff = openStuff;

        this.ProjectManager = new ProjectManager(userData);
        this.EventManager   = new EventManager(this);
        this.ScriptManager  = new ScriptManager(this);
        this.AudioManager   = new AudioManager();
        this.CpkList        = new List<string>();
    }

    public void InitCPKs(List<string> cpkPaths)
    {
        this.CpkList = cpkPaths;
        this.CpkTries = new Dictionary<string, Trie>();
        foreach (string cpkPath in cpkPaths)
            this.CpkTries[cpkPath] = new Trie(cpkPath, this.ProjectManager.CpkDecryptionFunctionName);
    }

    public bool CheckIfProjOpen(string modPath)
    {
        foreach (var openThing in this.OpenStuff)
            if (openThing.ModPath == modPath)
                return true;
        return false;
    }

    public void Reset()
    {
        this.ModPath = null;
        this.CpkPath = null;
        this.ReadOnly      = false;
        this.ProjectLoaded = false;
        this.EventLoaded   = false;
        this.AudioManager.Dispose();
        this.ScriptManager.Dispose();
        this.EventManager.Dispose();
        this.AudioManager = null;
        this.ScriptManager = null;
        this.EventManager = null;
        this.ScriptManager = null;
        this.CpkList.Clear();
        if (!(this.CpkTries is null))
        {
            foreach (string key in this.CpkTries.Keys)
                this.CpkTries[key].Dispose();
            this.CpkTries.Clear();
        }
        //this.ClearCache();
    }

    public async Task LoadProject(int ind)
    {
        // ActiveProject and ActiveGame should both be set
        await this.ProjectManager.LoadProject(ind);
        this.ModPath = this.ProjectManager.ActiveProject.Mod.Path;
        this.CpkPath = this.ProjectManager.ActiveGame.Path;
        this.ReadOnly = false;
        this.ProjectLoaded = true;

        this.WorkingPath = Path.Combine(this.WorkingPathBase, Hashify(this.ModPath));
        if (!Directory.Exists(this.WorkingPath))
            Directory.CreateDirectory(this.WorkingPath);
    }

    public async Task LoadGameReadOnly(int ind)
    {
        // ActiveGame should be set, ActiveProject should be null
        await this.ProjectManager.LoadGameReadOnly(ind);
        this.ModPath = null;
        this.CpkPath = this.ProjectManager.ActiveGame.Path;
        this.ReadOnly = true;
        this.ProjectLoaded = false;

        this.WorkingPath = Path.Combine(this.WorkingPathBase, Hashify(this.CpkPath));
        if (!Directory.Exists(this.WorkingPath))
            Directory.CreateDirectory(this.WorkingPath);
    }

    public async Task<bool> LoadEvent(int majorId, int minorId)
    {
        // this shouldn't happen
        if (!this.ReadOnly && !this.ProjectLoaded)
            return false;

        bool success = await this.EventManager.Load(majorId, minorId);
        if (success)
            await this.ProjectManager.LoadEvent(majorId, minorId);
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
        CPKExtract.ClearDirectory(this.WorkingPathBase);
    }

    private async Task<List<string>> GetModFiles(string[] prefix, string suffix = null)
    {
        return await CPKExtract.FindModFiles(prefix, suffix, this.ProjectManager.ModdedFileDir).ToListAsync();
    }

    private async Task<List<string>> GetGameFiles(string CpkPath, string[] prefix, string suffix = null)
    {
        List<CpkFile> matchingFiles = this.CpkTries[CpkPath].TryGetFile(prefix, suffix: suffix);
        if (matchingFiles.Count > 0)
            return await CPKExtract.ExtractFiles(matchingFiles, CpkPath, this.VanillaExtractionPath, this.ProjectManager.CpkDecryptionFunctionName).ToListAsync();
        else
            return new List<string>();
    }

    public List<string> ExtractExactFiles(string[] prefix, string suffix = null)
    {
        List<Task<List<string>>> tasks = new List<Task<List<string>>>();
        tasks.Add(this.GetModFiles(prefix, suffix));
        foreach (string CpkPath in this.CpkTries.Keys)
            tasks.Add(this.GetGameFiles(CpkPath, prefix, suffix));
        Task.WhenAll(tasks).Wait();
        List<string> ret = new List<string>();
        foreach (var task in tasks)
        {
            ret.AddRange(task.Result);
            task.Dispose();
        }
        tasks.Clear();
        return ret;
    }

    public List<string> ExtractExactFiles(string path)
    {
        string[] prefix = path.ToLower().Split(Trie.separators, StringSplitOptions.RemoveEmptyEntries);
        return this.ExtractExactFiles(prefix);
    }

    public async Task SaveBF()
    {
        this.ScriptManager.SaveScript("BF", this.WorkingPath, this.ProjectManager.ModdedFileDir, (await this.ProjectManager.HasFramework("BFEmulator")) ? this.ProjectManager.EmulatedFileDir : null);
    }

    public async Task SaveBMD()
    {
        this.ScriptManager.SaveScript("BMD", this.WorkingPath, this.ProjectManager.ModdedFileDir, (await this.ProjectManager.HasFramework("BMDEmulator")) ? this.ProjectManager.EmulatedFileDir : null);
    }

    public async Task SaveModdedFiles(bool evt, bool ecs, bool bmd, bool bf)
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
            await this.SaveBMD();

        if (bf)
            await this.SaveBF();
    }

}

public class Trie
{
    public Dictionary<string, Trie> Children;
    public CpkFile? Value;

    public static char[] separators = new char[] { '/', '\\' };

    public Trie(string cpkPath, string decryptionFunctionName)
    {
        this.Children = new Dictionary<string, Trie>();
        foreach (CpkFile file in CPKExtract.ListAllFiles(cpkPath, decryptionFunctionName))
            this.AddFile(file);
    }

    public Trie(CpkFile file, Queue<string> filePath)
    {
        this.Children = new Dictionary<string, Trie>();
        this.AddFile(file, filePath);
    }

    public void Dispose()
    {
        foreach (string key in this.Children.Keys)
            this.Children[key].Dispose();
        this.Children.Clear();
        this.Value = null;
    }

    public void AddFile(CpkFile file)
    {
        string normedDir = file.Directory.ToLower();
        string normedFile = file.FileName.ToLower();

        List<string> fullPath = new List<string>();
        fullPath.AddRange(normedDir.Split(separators, StringSplitOptions.RemoveEmptyEntries));
        fullPath.AddRange(normedFile.Split(separators, StringSplitOptions.RemoveEmptyEntries));

        this.AddFile(file, new Queue<string>(fullPath));
    }

    public void AddFile(CpkFile file, Queue<string> filePath)
    {
        if (filePath.Count == 0)
            this.Value = file;
        else
        {
            string prefix = filePath.Dequeue();
            if (this.Children.ContainsKey(prefix))
                this.Children[prefix].AddFile(file, filePath);
            else
                this.Children[prefix] = new Trie(file, filePath);
        }
    }

    public List<CpkFile> TryGetFile(string prefix, string suffix = null)
    {
        return this.TryGetFile(prefix.Split(separators, StringSplitOptions.RemoveEmptyEntries), suffix: suffix);
    }

    public List<CpkFile> TryGetFile(string[] prefix, string suffix = null)
    {
        Queue<string> fullPath = new Queue<string>(prefix);
        return this.TryGetFile(fullPath, suffix: suffix);
    }

    public List<CpkFile> TryGetFile(Queue<string> filePath, string suffix = null)
    {
        if (filePath.Count == 0)
        {
            if (suffix is null)
            {
                if (this.Value is null)
                    return new List<CpkFile>();
                else
                    return new List<CpkFile> { (CpkFile)this.Value };
            }
            else
            {
                List<CpkFile> ret = new List<CpkFile>();
                Regex suffixPattern = new Regex(suffix.ToLower());
                foreach (string key in this.Children.Keys)
                    if (suffixPattern.IsMatch(key))
                        ret.AddRange(this.Children[key].TryGetFile(filePath));
                return ret;
            }
        }
        else
        {
            string prefix = filePath.Dequeue().ToLower();
            if (this.Children.ContainsKey(prefix))
                return this.Children[prefix].TryGetFile(filePath, suffix: suffix);
            else
                return new List<CpkFile>();
        }
    }
}
