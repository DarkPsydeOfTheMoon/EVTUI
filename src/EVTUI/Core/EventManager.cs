using System;
using System.Collections.Generic;

// these are used in the commented-out block
//using System.IO;
//using System.Linq;

namespace EVTUI;

public struct CpkEVTContents
{
    public string? evtPath;
    public string? ecsPath;
    public List<string> acbPaths = new List<string>();
    public List<string> bfPaths  = new List<string>();
    public List<string> bmdPaths = new List<string>();

    public CpkEVTContents() {}
}

public class EventManager
{

    /////////////////////////////
    // *** PRIVATE MEMBERS *** //
    ///////////////./////////////
    private EVT? SerialEvent = null;

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ///////////////.////////////
    public List<string> AcbPaths;
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
        // TODO: put below into separate unit test package!
        /*this.SerialEvent.Write(cpkEVTContents.Value.evtPath + ".COPY");
        var serialEventCopy = new EVT();
        serialEventCopy.Read(cpkEVTContents.Value.evtPath + ".COPY");
        Console.WriteLine("##########");
        Console.WriteLine(File.ReadAllBytes(cpkEVTContents.Value.evtPath).SequenceEqual(File.ReadAllBytes(cpkEVTContents.Value.evtPath + ".COPY")));
        Console.WriteLine("##########");*/

        // the DataManager will pass these to the AudioManager
        this.AcbPaths = cpkEVTContents.Value.acbPaths;
        this.BfPaths  = cpkEVTContents.Value.bfPaths;
        this.BmdPaths = cpkEVTContents.Value.bmdPaths;

        return true;
    }

    public void Clear()
    {
        this.SerialEvent = null;
    }

}
