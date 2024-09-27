using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace EVTUI;

public class ACB
{
    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public string   AcbPath;
    public string   AwbPath;

    public UtfTable SerialAcb;
    public Afs2     SerialStreamAwbHeader;
    public Afs2     SerialMemoryAwb;
    public bool     SimpleAwbId;

    public Dictionary<string, UtfTable> Tables;
    public Dictionary<uint,   Cue>      Cues;

    public Stack<CueRange> CueRanges;
    public ObservableCollection<TrackEntry> TrackList;

    public string                           ExtractionMode { get; set; } = "default";
    public Dictionary<uint, MessageCue>?    MessageCues    { get; set; }

    public List<uint> CueIds { get { return new List<uint>(this.Cues.Keys); } }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public ACB(string acbPath, AudioCues eventCues, string awbPath = null)
    {
        this.AcbPath = acbPath;

        LocaleCues locale;
        if (this.AcbPath.Contains("_J"))
            locale = eventCues.JpCues;
        else
            locale = eventCues.EnCues;

        // TODO: this logic should really be in the audiomanager... passed as extractionMode arg, maybe
        if (AudioManager.Patterns["Common"].IsMatch(this.AcbPath))
        {
            this.ExtractionMode = "grouped";
            this.MessageCues    = locale.Common;
        }
        else if (AudioManager.Patterns["Field"].IsMatch(this.AcbPath))
            this.MessageCues    = locale.Field;
        else if (AudioManager.Patterns["Voice"].IsMatch(this.AcbPath))
            this.MessageCues    = locale.EventVoice;
        else if (AudioManager.Patterns["SFX"].IsMatch(this.AcbPath))
            this.MessageCues    = locale.EventSFX;
        else if (AudioManager.Patterns["System"].IsMatch(this.AcbPath))
            this.ExtractionMode = "used";

        // UpdateAudioCueFiles in the AudioManager should then just skip this file
        // (i.e., it won't be displayed)
        if (this.ExtractionMode != "default" && (this.MessageCues is null || this.MessageCues.Count == 0))
            return;

        this.SerialAcb = new UtfTable();
        this.SerialAcb.Read(this.AcbPath);

        if (!(this.SerialAcb.GetRowField(0, "StreamAwbAfs2Header").GetValue().GetValue() is null))
        {
            if (this.SerialAcb.GetRowField(0, "StreamAwbAfs2Header").GetValue().Magic == "@UTF")
                this.SerialStreamAwbHeader = this.SerialAcb.GetRowField(0, "StreamAwbAfs2Header").GetValue().GetValue().GetRowField(0, "Header").GetValue().GetValue();
            else
                this.SerialStreamAwbHeader = this.SerialAcb.GetRowField(0, "StreamAwbAfs2Header").GetValue().GetValue();
        }
        this.AwbPath = awbPath;

        this.Tables = new Dictionary<string, UtfTable>();
        foreach (string shortName in new string[]{ "TrackEvent", "TrackCommand", "SynthCommand", "SeqCommand" })
        {
            string longName = $"{shortName}Table";
            if (this.SerialAcb.Fields.ContainsKey(longName))
                this.Tables[shortName] = this.SerialAcb.GetRowField(0, longName).GetValue().GetValue();
            else if (this.SerialAcb.Fields.ContainsKey("CommandTable"))
                this.Tables[shortName] = this.SerialAcb.GetRowField(0, "CommandTable").GetValue().GetValue();
            else
                throw new Exception("Unknown ACB version.");
        }
        
        foreach (string name in new string[]
        {
            "Cue", "CueName", "Waveform", "Synth",
            "Track", "Sequence", "OutsideLink", "StringValue"
        })
            this.Tables[name] = this.SerialAcb.GetRowField(0, $"{name}Table").GetValue().GetValue();

        if (this.Tables["Waveform"].Fields.ContainsKey("Id"))
            this.SimpleAwbId = true;
        else if (this.Tables["Waveform"].Fields.ContainsKey("MemoryAwbId") && this.Tables["Waveform"].Fields.ContainsKey("StreamAwbId"))
            this.SimpleAwbId = false;
        else
            throw new Exception("Unknown ACB version.");

        this.SerialMemoryAwb = this.SerialAcb.GetRowField(0, "AwbFile").GetValue().GetValue();

        this.Cues = new Dictionary<uint, Cue>();
        this.CueRanges = new Stack<CueRange>();

        Dictionary<uint, CueRange> cueRanges = new Dictionary<uint, CueRange>();
        HashSet<CueRange> baseRanges = new HashSet<CueRange>();
        for (int i=0; i<this.Tables["Cue"].RowCount; i++)
        {
            uint cueId = this.Tables["Cue"].GetRowField(i, "CueId").GetValue();
            if (this.CueRanges.Count > 0 && this.CueRanges.Peek().IsContinuous(cueId))
                this.CueRanges.Peek().Update(cueId);
            else
                this.CueRanges.Push(new CueRange(cueId));
            cueRanges[cueId] = this.CueRanges.Peek();
            if (this.ExtractionMode == "grouped" && this.MessageCues.ContainsKey(cueId))
                baseRanges.Add(this.CueRanges.Peek());
        }

        Dictionary<uint, string> cueNames = new Dictionary<uint, string>();
        for (int i=0; i<this.Tables["CueName"].RowCount; i++)
        {
            int cueIndex = this.Tables["CueName"].GetRowField(i, "CueIndex").GetValue();
            string cueName = this.Tables["CueName"].GetRowField(i, "CueName").GetValue().Value;
            uint cueId = this.Tables["Cue"].GetRowField(cueIndex, "CueId").GetValue();
            cueNames[cueId] = cueName;
        }

        List<TrackEntry> trackList = new List<TrackEntry>();
        for (int i=0; i<this.Tables["Cue"].RowCount; i++)
        {
            uint cueId = this.Tables["Cue"].GetRowField(i, "CueId").GetValue();
            int refType = this.Tables["Cue"].GetRowField(i, "ReferenceType").GetValue();
            int refIndex = this.Tables["Cue"].GetRowField(i, "ReferenceIndex").GetValue();

            // TODO: how do we pick up when multiple messages refer to the same cue...?
            string? usage = (!(this.MessageCues is null) && this.MessageCues.ContainsKey((ushort)cueId)) ? this.MessageCues[(ushort)cueId].Stringification : null;

            if (
                this.ExtractionMode == "default" ||
                (this.ExtractionMode == "grouped" && baseRanges.Contains(cueRanges[cueId])) || 
                (this.ExtractionMode == "used" && this.MessageCues.ContainsKey(cueId))
            ) {
                List<Track> tracks = new List<Track>();
                List<int> waveInds = this.GetReference(refType, refIndex);
                for (int j=0; j<waveInds.Count; j++)
                {
                    byte encodeType = this.Tables["Waveform"].GetRowField(waveInds[j], "EncodeType").GetValue();
                    bool streaming = (this.Tables["Waveform"].GetRowField(waveInds[j], "Streaming").GetValue() != 0);
                    int awbId = 0;
                    if (this.SimpleAwbId)
                        awbId = this.Tables["Waveform"].GetRowField(waveInds[j], "Id").GetValue();
                    else
                    {
                        int memoryAwbId = this.Tables["Waveform"].GetRowField(waveInds[j], "MemoryAwbId").GetValue();
                        int streamAwbId = this.Tables["Waveform"].GetRowField(waveInds[j], "StreamAwbId").GetValue();
                        awbId = (streaming ? streamAwbId : memoryAwbId);
                    }
                    tracks.Add(new Track(awbId, encodeType, streaming));
                    trackList.Add(new TrackEntry(cueRanges[cueId].Name, cueId, cueNames[cueId], j+1, streaming, awbId, usage));
                }

                this.Cues[cueId] = new Cue(cueNames[cueId], cueId, cueRanges[cueId], tracks);
            } else
                this.Cues.Remove(cueId);
        }

        this.TrackList = new ObservableCollection<TrackEntry>(trackList);
    }

    public byte[] GetTrackBytes(uint cueId, int trackIndex, ulong? keyCode = null)
    {
        Track track = this.Cues[cueId].Tracks[trackIndex-1];
        if ((AudioEncodingType)track.EncodeType != AudioEncodingType.ADX)
            return null;

        byte[]? trackBytes = null;
        if (track.Streaming)
        {
            if (this.AwbPath is null)
                return null;
            trackBytes = this.SerialStreamAwbHeader.GetStreamEntry(this.AwbPath, track.AwbId);
        }
        else
            trackBytes = this.SerialMemoryAwb.GetMemoryEntry(track.AwbId);

        if (trackBytes is null || keyCode is null)
            return trackBytes;

        Adx adx = new Adx();
        adx.FromBytes(trackBytes);
        adx.Decrypt((ulong)keyCode);
        return adx.ToBytes();
    }

    /////////////////////////////
    // *** PRIVATE METHODS *** //
    /////////////////////////////
    private List<int> GetReference(int refType, int refIndex)
    {
        List<int> rowInds = new List<int>();
        switch ((CueReferenceType)refType)
        {
            case CueReferenceType.Waveform:
                rowInds.Add(refIndex);
                break;
            case CueReferenceType.Synth:
                byte[] refItems = this.Tables["Synth"].GetRowField(refIndex, "ReferenceItems").GetValue().GetValue();
                int refType2 = ((int)refItems[0] << 8) + (int)refItems[1];
                int refIndex2 = ((int)refItems[2] << 8) + (int)refItems[3];
                rowInds.AddRange(this.GetReference(refType2, refIndex2));
                break;
            case CueReferenceType.Sequence:
                int numTracks = this.Tables["Sequence"].GetRowField(refIndex, "NumTracks").GetValue();
                byte[] trackIndex = this.Tables["Sequence"].GetRowField(refIndex, "TrackIndex").GetValue().GetValue();
                for (int i=0; i<numTracks; i++)
                {
                    int trackId = ((int)trackIndex[2*i] << 8) + (int)trackIndex[(2*i)+1];
                    int eventIndex = this.Tables["Track"].GetRowField(trackId, "EventIndex").GetValue();
                    Queue<byte> cmdBytes = new Queue<byte>(this.Tables["TrackEvent"].GetRowField(eventIndex, "Command").GetValue().GetValue());
                    while (cmdBytes.Count > 0)
                    {
                        int cmdType = ((int)cmdBytes.Dequeue() << 8) + (int)cmdBytes.Dequeue();
                        byte paramCount = cmdBytes.Dequeue();
                        int[] param = new int[paramCount];
                        for (int j=0; j<paramCount; j++)
                            param[j] = cmdBytes.Dequeue();
                        if ((CommandType)cmdType == CommandType.ReferenceItem1 || (CommandType)cmdType == CommandType.ReferenceItem2)
                        {
                            int refType3 = (param[0] << 8) + param[1];
                            int refIndex3 = (param[2] << 8) + param[3];
                            rowInds.AddRange(this.GetReference(refType3, refIndex3));
                        }
                    }
                }
                break;
            case CueReferenceType.Link:
                if (!(this.Tables["OutsideLink"] is null))
                {
                    UInt16 acbIndex = 0xFFFF;
                    if (this.Tables["OutsideLink"].Fields.ContainsKey("AcbNameStringIndex"))
                        acbIndex = this.Tables["OutsideLink"].GetRowField(refIndex, "AcbNameStringIndex").GetValue();
                    if (acbIndex == 0xFFFF)
                    {
                        uint linkId = this.Tables["OutsideLink"].GetRowField(refIndex, "Id").GetValue();
                        for (int i=0; i<this.Tables["Cue"].Rows.Length; i++)
                            if (this.Tables["Cue"].GetRowField(i, "CueId").GetValue() == linkId)
                            {
                                int refType4 = this.Tables["Cue"].GetRowField(i, "ReferenceType").GetValue();
                                int refIndex4 = this.Tables["Cue"].GetRowField(i, "ReferenceIndex").GetValue();
                                rowInds.AddRange(this.GetReference(refType4, refIndex4));
                                break;
                            }
                    }
                }
                break;
        }
        return rowInds;
    }
}

public class Cue
{
    public Cue(string name, uint cueId, CueRange cueGroup, List<Track> tracks)
    {
        this.Name     = name;
        this.CueId    = cueId;
        this.CueGroup = cueGroup;
        this.Tracks   = tracks;
    }

    public string      Name;
    public uint        CueId;
    public CueRange    CueGroup;
    public List<Track> Tracks;
}

public class Track
{
    public Track(int awbId, byte encodeType, bool streaming)
    {
        this.AwbId       = awbId;
        this.EncodeType  = encodeType;
        this.Streaming   = streaming;
    }

    public int    AwbId;
    public byte   EncodeType;
    public bool   Streaming;
}

public class CueRange
{
    public CueRange(uint start)
    {
        this.Lower = start;
        this.Upper = start;
    }

    public bool IsContinuous(uint candidate)
    {
        return (candidate+1 >= this.Lower && candidate <= this.Upper+1);
    }

    public void Update(uint newMember)
    {
        if (newMember < this.Lower)
            this.Lower = newMember;
        if (newMember > this.Upper)
            this.Upper = newMember;
    }

    public uint Lower;
    public uint Upper;
    public string Name { get { return $"{this.Lower}-{this.Upper}"; } }
}

public class TrackEntry
{
    public TrackEntry(string cueGroup, uint cueId, string cueName, int trackIndex, bool isStreaming, int awbId, string? usage)
    {
        CueGroup = cueGroup;
        CueId = cueId;
        CueName = cueName;
        TrackIndex = trackIndex;
        if (isStreaming)
            AwbMode = "Stream";
        else
            AwbMode = "Memory";
        AwbId = awbId;
        Usage = usage;
    }

    public string  CueGroup   { get; set; }
    public uint    CueId      { get; set; }
    public string  CueName    { get; set; }
    public int     TrackIndex { get; set; }
    public string  AwbMode    { get; set; }
    public int     AwbId      { get; set; }
    public string? Usage      { get; set; }
}

public enum CueReferenceType : byte
{
    Waveform      = 1,
    Synth         = 2,
    Sequence      = 3,
    Link          = 5,
    BlockSequence = 8,
    Nothing       = 255
}

public enum CommandType : int
{
    Null      = 0,
    /***** Synth Commands *****/
    Unk34     = 34,
    /***** Sequence Commands *****/
    Unk65     = 65,
    Unk68     = 68,
    Unk69     = 69, //nice
    CueLimit  = 79,
    VolumeBus = 111,
    /***** Track Commands *****/
    ReferenceItem1 = 2000,
    Wait           = 2001,
    ReferenceItem2 = 20003
}

public enum AudioEncodingType : byte
{
    ADX     = 0,
    HCA     = 2,
    HCA_ALT = 6,
    VAG     = 7,
    ATRAC3  = 8,
    BCWAV   = 9,
    ATRAC9  = 11,
    DSP     = 13,
    Nothing = 255
}
