using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EVTUI;

public class EventManager
{

    /////////////////////////////
    // *** PRIVATE MEMBERS *** //
    ///////////////./////////////
    private DataManager config;
    //private EVT? SerialEvent = null;
    private ECS? SerialEventSounds = null;

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ///////////////.////////////
    public string       EvtPath;
    public string       EcsPath;
    public List<(string ACB, string? AWB)> AcwbPaths;
    public List<string> BfPaths;
    public List<string> BmdPaths;

    // TODO: re-privatize this...? the Basics and Assets tabs use it...
    // is there a nicer way than having it just be public?
    public EVT? SerialEvent = null;

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public EventManager(DataManager config)
    {
        this.config = config;
    }

    public void Dispose()
    {
        this.SerialEvent = null;
        this.SerialEventSounds = null;
        this.AcwbPaths.Clear();
        this.BfPaths.Clear();
        this.BmdPaths.Clear();
        this.config = null;
    }

    private async Task AddEcs(int majorHundred, int majorTen, int majorId, int minorId)
    {
        string[] ecsPrefix = new string[] { "event", $"e{majorHundred:000}", $"e{majorTen:000}", $"e{majorId:000}_{minorId:000}.ecs" };
        List<string> ecsPaths = this.config.ExtractExactFiles(ecsPrefix);
        this.SerialEventSounds = new ECS();
        if (ecsPaths.Count == 0)
            this.EcsPath = this.EvtPath.Substring(0, this.EvtPath.Length-4) + ".ECS";
        else
        {
            this.EcsPath = ecsPaths[0];
            this.SerialEventSounds.Read(this.EcsPath);
        }
        ecsPaths.Clear();
        await Task.Yield();
    }

    private async Task AddCommonAcwb(string name)
    {
        List<string> acbPaths = this.config.ExtractExactFiles(new string[] { "sound", $"{name}.acb" });
        if (acbPaths.Count > 0)
        {
            List<string> awbPaths = this.config.ExtractExactFiles(new string[] { "sound", $"{name}.awb" });
            acbPaths.Sort();
            awbPaths.Sort();
            lock (this.AcwbPaths)
            {
                for (int i=0; i<acbPaths.Count; i++)
                    this.AcwbPaths.Add((acbPaths[i], (awbPaths.Count > i) ? awbPaths[i] : (awbPaths.Count > 0) ? awbPaths[0] : null));
            }
            awbPaths.Clear();
        }
        acbPaths.Clear();
        await Task.Yield();
    }

    private async Task AddVoiceAcwb(int majorId, int minorId)
    {
        List<string> voiceAcbPaths = this.config.ExtractExactFiles(new string[] { "sound", "event", $"e{majorId:000}_{minorId:000}.acb" });
        if (voiceAcbPaths.Count > 0)
        {
            List<string> voiceAwbPaths = this.config.ExtractExactFiles(new string[] { "sound", "event", $"e{majorId:000}_{minorId:000}.awb" });
            voiceAcbPaths.Sort();
            voiceAwbPaths.Sort();
            for (int i=0; i<voiceAcbPaths.Count; i++)
                this.AcwbPaths.Add((voiceAcbPaths[i], (voiceAwbPaths.Count > i) ? voiceAwbPaths[i] : (voiceAwbPaths.Count > 0) ? voiceAwbPaths[0] : null));
            voiceAwbPaths.Clear();
        }
        voiceAcbPaths.Clear();
        await Task.Yield();
    }

    private async Task AddSfxAcwb(int majorId, int minorId)
    {
        List<string> sfxAcbPaths = this.config.ExtractExactFiles(new string[] { "sound", "event", $"e{majorId:000}_{minorId:000}_se.acb" });
        if (sfxAcbPaths.Count > 0)
        {
            List<string> sfxAwbPaths = this.config.ExtractExactFiles(new string[] { "sound", "event", $"e{majorId:000}_{minorId:000}_se.awb" });
            sfxAcbPaths.Sort();
            sfxAwbPaths.Sort();
            for (int i=0; i<sfxAcbPaths.Count; i++)
                this.AcwbPaths.Add((sfxAcbPaths[i], (sfxAwbPaths.Count > i) ? sfxAwbPaths[i] : (sfxAwbPaths.Count > 0) ? sfxAwbPaths[0] : null));
            sfxAwbPaths.Clear();
        }
        sfxAcbPaths.Clear();
        await Task.Yield();
    }

    public async Task AddBmd(int majorHundred, int majorId, int minorId)
    {
        if (this.SerialEvent.Flags[12])
            this.BmdPaths = this.config.ExtractExactFiles(this.SerialEvent.EventBmdPath.Replace("\0", ""));
        else
            this.BmdPaths = this.config.ExtractExactFiles(new string[] { "event_data", "message", $"e{majorHundred:000}", $"e{majorId:000}_{minorId:000}.bmd" });
        this.BmdPaths.Sort();
        this.BmdPaths.Reverse();
        await Task.Yield();
    }

    public async Task AddBf(int majorHundred, int majorId, int minorId)
    {
        if (this.SerialEvent.Flags[14])
            this.BfPaths = this.config.ExtractExactFiles(this.SerialEvent.EventBfPath.Replace("\0", ""));
        else
            this.BfPaths = this.config.ExtractExactFiles(new string[] { "event_data", "script", $"e{majorHundred:000}", $"e{majorId:000}_{minorId:000}.bf" });
        this.BfPaths.Sort();
        this.BfPaths.Reverse();
        await Task.Yield();
    }

    public async Task<bool> Load(int majorId, int minorId)
    {
        int majorHundred = (majorId / 100) * 100;
        int majorTen = (majorId / 10) * 10;

        // EVT
        string[] evtPrefix = new string[] { "event", $"e{majorHundred:000}", $"e{majorTen:000}", $"e{majorId:000}_{minorId:000}.evt" };
        List<string> evtPaths = this.config.ExtractExactFiles(evtPrefix);
        if (evtPaths.Count == 0)
            return false;
        this.EvtPath = evtPaths[0];
        this.SerialEvent = new EVT();
        this.SerialEvent.Read(this.EvtPath);
        evtPaths.Clear();

        this.AcwbPaths = new List<(string ACB, string? AWB)>();
        List<Task> tasks = new List<Task>();

        // ECS
        tasks.Add(this.AddEcs(majorHundred, majorTen, majorId, minorId));

        // ACB/AWB
        // SYSTEM, BGM, COMMON
        //foreach (string name in new string[] { "system", "bgm", "voice_singleword" })
        foreach (string name in new string[] { "bgm", "voice_singleword" })
            tasks.Add(this.AddCommonAcwb(name));
        // EVENT (voice)
        tasks.Add(this.AddVoiceAcwb(majorId, minorId));
        // EVENT (sfx)
        tasks.Add(this.AddSfxAcwb(majorId, minorId));

        // BMD
        tasks.Add(this.AddBmd(majorHundred, majorId, minorId));

        // BF
        tasks.Add(this.AddBf(majorHundred, majorId, minorId));

        await Task.WhenAll(tasks);
        foreach (Task task in tasks)
            task.Dispose();
        tasks.Clear();
        return true;
    }

    public void SaveEVT() { this.SerialEvent.Write(this.EvtPath); }
    public void SaveECS() { this.SerialEventSounds.Write(this.EcsPath); }

    public int EventDuration { get { return this.SerialEvent.FrameCount; } }
    public SerialCommand[] EventCommands { get { return this.SerialEvent.Commands; } }
    public ArrayList EventCommandData { get { return this.SerialEvent.CommandData; } }
    public SerialCommand[] EventSoundCommands { get { return this.SerialEventSounds.Commands; } }
    public ArrayList EventSoundCommandData { get { return this.SerialEventSounds.CommandData; } }

    public bool DeleteCommand(int index, bool isAudio)
    {
        if (isAudio)
            return this.SerialEventSounds.DeleteCommand(index);
        else
            return this.SerialEvent.DeleteCommand(index);
    }

    public int CopyCommandToNewFrame(SerialCommand cmd, dynamic cmdData, bool isAudio, int frame)
    {
        if (isAudio)
            return this.SerialEventSounds.CopyCommandToNewFrame(cmd, cmdData, frame);
        else
            return this.SerialEvent.CopyCommandToNewFrame(cmd, cmdData, frame);
    }

    public int NewCommand(string commandCode, int frameStart)
    {
        if (ECS.ValidEcsCommands.Contains(commandCode))
            return this.SerialEventSounds.NewCommand(commandCode, frameStart);
        else
            return this.SerialEvent.NewCommand(commandCode, frameStart);
    }

    public List<string> AddableCodes
    {
        get
        {
            List<string> codes = new List<string>();
            foreach (Type t in typeof(CommandTypes).GetNestedTypes())
                codes.Add(t.Name);
            return codes;
        }
    }

    public List<int> AssetIDs
    {
        get
        {
            List<int> ids = new List<int>();
            foreach (SerialObject obj in this.SerialEvent.Objects)
                ids.Add(obj.Id);
            return ids;
        }
    }

    public List<int> AssetIDsOfType(int type)
    {
        List<int> ids = new List<int>();
        foreach (SerialObject obj in this.SerialEvent.Objects)
            if (obj.Type == type)
                ids.Add(obj.Id);
        return ids;
    }

}
