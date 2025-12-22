using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace EVTUI;

public struct CpkEVTContents
{
    public string? evtPath;
    public string? ecsPath;
    public List<string> acbPaths = new List<string>();
    public List<string> awbPaths = new List<string>();
    public List<string> bfPaths  = new List<string>();
    public List<string> bmdPaths = new List<string>();

    public CpkEVTContents() {}
}

public class EventManager
{

    /////////////////////////////
    // *** PRIVATE MEMBERS *** //
    ///////////////./////////////
    private DataManager config;
    //private EVT?   SerialEvent               = null;
    private ECS?   SerialEventSounds         = null;

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
    public EVT?   SerialEvent               = null;

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public EventManager(DataManager config)
    {
        this.config = config;
    }

    public bool Load(CpkEVTContents? cpkEVTContents)
    {
        this.Clear();
        if (cpkEVTContents is null)
            return false;

        this.EvtPath = cpkEVTContents.Value.evtPath;
        this.SerialEvent = new EVT();
        this.SerialEvent.Read(this.EvtPath);

        this.EcsPath = cpkEVTContents.Value.ecsPath;
        this.SerialEventSounds = new ECS();
        if (this.EcsPath is null)
            this.EcsPath = this.EvtPath.Substring(0, this.EvtPath.Length-4) + ".ECS";
        else
            this.SerialEventSounds.Read(this.EcsPath);

        // cheap way of putting en.cpk before base.cpk and modded files before vanilla files, lolllll
        // i'll do it properly eventually, maybe
        cpkEVTContents.Value.acbPaths.Reverse();
        cpkEVTContents.Value.bfPaths.Reverse();
        cpkEVTContents.Value.bmdPaths.Reverse();

        // the DataManager will pass these to the AudioManager
        this.AcwbPaths = new List<(string ACB, string? AWB)>();
        foreach (string acbPath in cpkEVTContents.Value.acbPaths)
        {
            string basePath = acbPath.Substring(0, acbPath.Length-4);
            Regex awbPattern = new Regex(Regex.Escape(basePath)+"\\.AWB$", RegexOptions.IgnoreCase);
            string awbPath = null;
            foreach (string candidateAwbPath in cpkEVTContents.Value.awbPaths)
                if (awbPattern.IsMatch(candidateAwbPath))
                {
                    awbPath = candidateAwbPath;
                    break;
                }
            this.AcwbPaths.Add((acbPath, awbPath));
        }
        if (this.SerialEvent.Flags[12])
            this.BmdPaths = config.ExtractMatchingFiles(this.SerialEvent.EventBmdPath.Replace("\0", "").Replace("/", "[\\\\/]"));
        else
            // derive this from IDs within EVT instead...?
            this.BmdPaths = cpkEVTContents.Value.bmdPaths;

        if (this.SerialEvent.Flags[14])
            this.BfPaths = config.ExtractMatchingFiles(this.SerialEvent.EventBfPath.Replace("\0", "").Replace("/", "[\\\\/]"));
        else
            // derive this from IDs within EVT instead...?
            this.BfPaths = cpkEVTContents.Value.bfPaths;

        return true;
    }

    public void Clear()
    {
        this.SerialEvent               = null;
        this.SerialEventSounds         = null;
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
