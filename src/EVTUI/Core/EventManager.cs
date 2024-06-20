using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

// these are used in the commented-out block
//using System.IO;
//using System.Linq;

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
    private EVT? SerialEvent       = null;
    private ECS? SerialEventSounds = null;

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ///////////////.////////////
    public List<(string ACB, string? AWB)> AcwbPaths;
    public List<string> BfPaths;
    public List<string> BmdPaths;

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public bool Load(List<string> cpkList, string evtid, string targetdir)
    {
        this.Clear();
        CpkEVTContents? cpkEVTContents = CPKExtract.ExtractEVTFiles(cpkList, evtid, targetdir);
        if (cpkEVTContents is null)
            return false;

        this.SerialEvent = new EVT();
        this.SerialEvent.Read(cpkEVTContents.Value.evtPath);

        this.SerialEventSounds = new ECS();
        this.SerialEventSounds.Read(cpkEVTContents.Value.ecsPath);

        // TODO: put below into separate unit test package!
        /*this.SerialEvent.Write(cpkEVTContents.Value.evtPath + ".COPY");
        var serialEventCopy = new EVT();
        serialEventCopy.Read(cpkEVTContents.Value.evtPath + ".COPY");
        Trace.Assert(File.ReadAllBytes(cpkEVTContents.Value.evtPath).SequenceEqual(File.ReadAllBytes(cpkEVTContents.Value.evtPath + ".COPY")), $"Reflexivity of read/write fails for {cpkEVTContents.Value.evtPath}");*/

        // the DataManager will pass these to the AudioManager
        this.AcwbPaths = new List<(string ACB, string? AWB)>();
        foreach (string acbPath in cpkEVTContents.Value.acbPaths)
        {
            string awbPath = acbPath.Substring(0, acbPath.Length-4) + ".AWB";
            if (cpkEVTContents.Value.awbPaths.Contains(awbPath))
                this.AcwbPaths.Add((acbPath, awbPath));
            else
                this.AcwbPaths.Add((acbPath, null));
        }
        this.BfPaths  = cpkEVTContents.Value.bfPaths;
        this.BmdPaths = cpkEVTContents.Value.bmdPaths;

        // TODO: put below into separate unit test package!
        /*foreach (string bmdPath in this.BmdPaths)
        {
            var dialogue = new BMD();
            dialogue.Read(bmdPath);
            // this won't actually be equal to the original, mostly, because of padding
            // will need to test later whether that actually matters for the game's reader
            dialogue.Write(bmdPath + ".COPY");
            var dialogueCopy = new BMD();
            dialogueCopy.Read(bmdPath + ".COPY");
            dialogueCopy.Write(bmdPath + ".COPY.COPY");
            var dialogueCopyCopy = new BMD();
            dialogueCopyCopy.Read(bmdPath + ".COPY.COPY");
            Trace.Assert(File.ReadAllBytes(bmdPath + ".COPY").SequenceEqual(File.ReadAllBytes(bmdPath + ".COPY.COPY")), $"Reflexivity of read/write fails for {bmdPath}");
            Trace.Assert(dialogue.Speakers.Length == dialogueCopyCopy.Speakers.Length, $"Original speakers ({dialogue.Speakers.Length}) don't match rewritten speakers ({dialogueCopyCopy.Speakers.Length})");
            for (int i=0; i<dialogue.Speakers.Length; i++)
            {
                string speakerIn = System.Text.Encoding.Default.GetString(dialogue.Speakers[i]);
                string speakerOut = System.Text.Encoding.Default.GetString(dialogueCopyCopy.Speakers[i]);
                Trace.Assert(speakerIn == speakerOut, $"Original speaker ({speakerIn}) doesn't match rewritten speaker ({speakerOut})");
            }
        }*/

        return true;
    }

    public void Clear()
    {
        this.SerialEvent       = null;
        this.SerialEventSounds = null;
    }

    public int EventDuration
    {
        get
        {
            return this.SerialEvent.TotalFrame;
        }
    }

    public SerialCommand[] EventCommands
    {
        get
        {
            return this.SerialEvent.Commands;
        }
    }

    public ArrayList EventCommandData
    {
        get
        {
            return this.SerialEvent.CommandData;
        }
    }

    public SerialCommand[] EventSoundCommands
    {
        get
        {
            return this.SerialEventSounds.Commands;
        }
    }

    public ArrayList EventSoundCommandData
    {
        get
        {
            return this.SerialEventSounds.CommandData;
        }
    }

    public List<int> ObjectIndices(string? type)
    {
        List<int> ret = new List<int>();
        foreach (SerialObject obj in this.SerialEvent.Objects)
            if (type is null || (type == "model" && (obj.Type == 0x00000301 || obj.Type == 0x00000401 || obj.Type == 0x00000601 || obj.Type == 0x00020101 || obj.Type == 0x01000101 || obj.Type == 0x02000101 || obj.Type == 0x02000701 || obj.Type == 0x04000201)))
                ret.Add(obj.Id);
        return ret;
    }

}
