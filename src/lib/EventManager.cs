using System;
using System.Collections;
using System.Collections.Generic;

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
    private EVT?   SerialEvent               = null;
    private ECS?   SerialEventSounds         = null;
    private string CpkDecryptionFunctionName = null;

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ///////////////.////////////
    public List<(string ACB, string? AWB)> AcwbPaths;
    public List<string> BfPaths;
    public List<string> BmdPaths;

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public bool Load(List<string> cpkList, string evtid, string targetdir, string cpkDecryptionFunctionName)
    {
        this.Clear();
        this.CpkDecryptionFunctionName = cpkDecryptionFunctionName;

        CpkEVTContents? cpkEVTContents = CPKExtract.ExtractEVTFiles(cpkList, evtid, targetdir, this.CpkDecryptionFunctionName);
        if (cpkEVTContents is null)
            return false;

        this.SerialEvent = new EVT();
        this.SerialEvent.Read(cpkEVTContents.Value.evtPath);

        this.SerialEventSounds = new ECS();
        this.SerialEventSounds.Read(cpkEVTContents.Value.ecsPath);

        // the DataManager will pass these to the AudioManager
        this.AcwbPaths = new List<(string ACB, string? AWB)>();
        foreach (string acbPath in cpkEVTContents.Value.acbPaths)
        {
            // TODO: smarter way to make this case-insensitive...
            string basePath = acbPath.Substring(0, acbPath.Length-4);
            if (cpkEVTContents.Value.awbPaths.Contains(basePath+".AWB"))
                this.AcwbPaths.Add((acbPath, basePath+".AWB"));
            else if (cpkEVTContents.Value.awbPaths.Contains(basePath+".awb"))
                this.AcwbPaths.Add((acbPath, basePath+".awb"));
            else
                this.AcwbPaths.Add((acbPath, null));
        }
        this.BfPaths  = cpkEVTContents.Value.bfPaths;
        this.BmdPaths = cpkEVTContents.Value.bmdPaths;

        return true;
    }

    public void Clear()
    {
        this.SerialEvent               = null;
        this.SerialEventSounds         = null;
        this.CpkDecryptionFunctionName = null;
    }

    public int EventDuration { get { return this.SerialEvent.TotalFrame; } }
    public SerialCommand[] EventCommands { get { return this.SerialEvent.Commands; } }
    public ArrayList EventCommandData { get { return this.SerialEvent.CommandData; } }
    public SerialCommand[] EventSoundCommands { get { return this.SerialEventSounds.Commands; } }
    public ArrayList EventSoundCommandData { get { return this.SerialEventSounds.CommandData; } }

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

    public Dictionary<int, SerialObject> ObjectsById
    {
        get
        {
            Dictionary<int, SerialObject> ret = new Dictionary<int, SerialObject>();
            foreach (SerialObject obj in this.SerialEvent.Objects)
                ret[obj.Id] = obj;
            return ret;
        }
    }

    public List<string> GetAssetPaths(int assetId, List<string> cpkList, string targetdir)
    {
        SerialObject obj = this.ObjectsById[assetId];
        string pattern = "";
        switch ((ObjectTypes)obj.Type)
        {
            case ObjectTypes.Character:
                // cheat to just get all of the models if it's a 0
                // i.e. it's detected with a datetime checker
                if (obj.ResourceMinorId == 0)
                   pattern = $"MODEL[\\\\/]CHARACTER[\\\\/]{obj.ResourceMajorId:0000}[\\\\/]C{obj.ResourceMajorId:0000}_00._{obj.ResourceSubId:00}\\.GMD";
                else
                {
                    if (obj.ResourceSubId == 0)
                        pattern = $"MODEL[\\\\/]CHARACTER[\\\\/]{obj.ResourceMajorId:0000}[\\\\/]C{obj.ResourceMajorId:0000}_{obj.ResourceMinorId:000}_..\\.GMD";
                    else
                        pattern = $"MODEL[\\\\/]CHARACTER[\\\\/]{obj.ResourceMajorId:0000}[\\\\/]C{obj.ResourceMajorId:0000}_{obj.ResourceMinorId:000}_{obj.ResourceSubId:00}\\.GMD";
                }
                break;
            case ObjectTypes.Item:
                pattern = $"MODEL[\\\\/]ITEM[\\\\/]IT{obj.ResourceMajorId:0000}_{obj.ResourceMinorId:000}\\.GMD";
                break;
            case ObjectTypes.FieldObject:
                pattern = $"MODEL[\\\\/]FIELD_TEX[\\\\/]OBJECT[\\\\/]M{obj.ResourceMajorId:000}_{obj.ResourceMinorId:000}\\.GMD";
                break;
            case ObjectTypes.Field:
                pattern = $"MODEL[\\\\/]FIELD_TEX[\\\\/]F{obj.ResourceMajorId:000}_{obj.ResourceMinorId:000}_{obj.ResourceSubId}\\.GFS";
                break;
            default:
                Console.WriteLine(obj.Type);
                break;
        }
        if (pattern == "")
            return new List<string>();
        else
            return CPKExtract.ExtractMatchingFiles(cpkList, pattern, targetdir, this.CpkDecryptionFunctionName);
    }

    public List<string> GetAnimPaths(int assetId, bool fromBaseAnims, bool blendAnims, List<string> cpkList, string targetdir)
    {
        SerialObject obj = this.ObjectsById[assetId];
        string animType = (fromBaseAnims) ? "B" : "A";
        int animId = (fromBaseAnims) ? obj.BaseMotionNo : obj.ExtBaseMotionNo;
        string pattern = "";
        switch ((ObjectTypes)obj.Type)
        {
            case ObjectTypes.Character:
                if (blendAnims)
                    pattern = $"MODEL[\\\\/]CHARACTER[\\\\/]{obj.ResourceMajorId:0000}[\\\\/]EMT{obj.ResourceMajorId:0000}\\.GAP";
                else
                    pattern = $"MODEL[\\\\/]CHARACTER[\\\\/]{obj.ResourceMajorId:0000}[\\\\/]EVENT[\\\\/]{animType}E{obj.ResourceMajorId:0000}_{animId:000}\\.GAP";
                break;
            case ObjectTypes.Enemy:
                pattern = $"MODEL[\\\\/]CHARACTER[\\\\/]ENEMY[\\\\/]{obj.ResourceMajorId:0000}[\\\\/]{animType}EM{obj.ResourceMajorId:0000}_{animId:000}\\.GAP";
                break;
            case ObjectTypes.Persona:
                pattern = $"MODEL[\\\\/]CHARACTER[\\\\/]PERSONA[\\\\/]{obj.ResourceMajorId:0000}[\\\\/]{animType}PS{obj.ResourceMajorId:0000}_{animId:000}\\.GAP";
                break;
            case ObjectTypes.Item:
                // yes, it's the GMD itself. for items, that's where animations are also stored
                pattern = $"MODEL[\\\\/]ITEM[\\\\/]IT{obj.ResourceMajorId:0000}_{obj.ResourceMinorId:000}\\.GMD";
                break;
            default:
                break;
        }
        if (pattern == "")
            return new List<string>();
        else
        {
            List<string> candidates = CPKExtract.ExtractMatchingFiles(cpkList, pattern, targetdir, this.CpkDecryptionFunctionName);
            if (candidates.Count == 0 && (ObjectTypes)obj.Type == ObjectTypes.Character)
            {
                pattern = $"MODEL[\\\\/]CHARACTER[\\\\/]COMMON_ANIM[\\\\/]{animType}CMN{animId:0000}\\.GAP";
                candidates = CPKExtract.ExtractMatchingFiles(cpkList, pattern, targetdir, this.CpkDecryptionFunctionName);
            }
            return candidates;
        }
    }

    public enum ObjectTypes : int
    {
        Field            = 0x00000003,
        Env              = 0x00000004,
        Image            = 0x00000005,
        ParticlePak      = 0x00000007,
        Movie            = 0x00000008,
        Camera           = 0x00000009,
        Enemy            = 0x00000301,
        SymShadow        = 0x00000401,
        Item             = 0x00000601,
        ResourceTableNPC = 0x00020101,
        Particle         = 0x01000002,
        Character        = 0x01000101,
        FieldCharacter   = 0x02000101,
        FieldObject      = 0x02000701,
        Persona          = 0x04000201,
    }

}
