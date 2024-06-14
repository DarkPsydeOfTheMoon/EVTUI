using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;

using Xv2CoreLib.ACB;
using Xv2CoreLib.AFS2;
using VGAudio.Cli;
using Convert = VGAudio.Cli.Convert;

namespace EVTUI;

public class CueData
{
    public CueData(uint id, string name)
    {
        Id     = id;
        Name   = name;
        Tracks = new List<TrackData>();
    }

    public uint            Id;
    public string          Name;
    public List<TrackData> Tracks;

    public void AddTrack(int index, bool isStreaming, int awbId, string adxPath)
    {
        Tracks.Add(new TrackData(index, isStreaming, awbId, adxPath));
    }
}

public class TrackData
{
    public TrackData(int index, bool isStreaming, int awbId, string adxPath)
    {
        Index = index;
        if (isStreaming)
            Mode = "Stream";
        else
            Mode = "Memory";
        AwbId = awbId;
        AdxPath = adxPath;
    }

    public int    Index;
    public string Mode;
    public int    AwbId;
    public string AdxPath;
}

public class TrackEntry
{
    public TrackEntry(uint cueId, string cueName, int trackIndex, bool isStreaming, int awbId)
    {
        CueId = cueId;
        CueName = cueName;
        TrackIndex = trackIndex;
        if (isStreaming)
            AwbMode = "Stream";
        else
            AwbMode = "Memory";
        AwbId = awbId;
    }

    public string  CueRange   { get; set; }
    public uint    CueId      { get; set; }
    public string  CueName    { get; set; }
    public int     TrackIndex { get; set; }
    public string  AwbMode    { get; set; }
    public int     AwbId      { get; set; }
    public string? Usage      { get; set; }
}

public class ACWBData
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public string                           AcbPath        { get; }
    public Dictionary<uint, CueData?>       Cues           { get; }
    public ObservableCollection<TrackEntry> TrackList      { get; }

    public string                           ExtractionMode { get; set; } = "default";
    public Dictionary<uint, MessageCue>?    MessageCues    { get; set; }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    //public ACWBData(string filename) : this(filename, null) {}
    public ACWBData(string filename, AudioCues eventCues)
    {
        Console.WriteLine(filename);

        AcbPath = filename;
        ACB_File acb = ACB_File.Load(AcbPath);

        LocaleCues locale;
        if (this.AcbPath.Contains("_J"))
            locale = eventCues.JpCues;
        else
            locale = eventCues.EnCues;

        if (Regex.IsMatch(this.AcbPath, "VOICE_SINGLEWORD\\.ACB$"))
        {
            this.ExtractionMode = "grouped";
            this.MessageCues    = locale.Common;
        }
        else if (Regex.IsMatch(this.AcbPath, "F[0-9]{3}_[0-9]{3}\\.ACB$"))
            this.MessageCues    = locale.Field;
        else if (Regex.IsMatch(this.AcbPath, "E[0-9]{3}_[0-9]{3}\\.ACB$"))
            this.MessageCues    = locale.EventVoice;
        else if (Regex.IsMatch(this.AcbPath, "E[0-9]{3}_[0-9]{3}_SE\\.ACB$"))
            this.MessageCues    = locale.EventSFX;
        else if (Regex.IsMatch(this.AcbPath, "SYSTEM\\.ACB$"))
            this.ExtractionMode = "used";

        // UpdateAudioCueFiles in the AudioManager should then just skip this file
        // (i.e., it won't be displayed)
        if (this.ExtractionMode != "default" && (this.MessageCues is null || this.MessageCues.Count == 0))
            return;

        string fullAcbPath = Path.GetFullPath(AcbPath);
        string baseOutputDir = Path.Combine(Path.GetDirectoryName(fullAcbPath), Path.GetFileNameWithoutExtension(fullAcbPath));
        if (!Directory.Exists(baseOutputDir))
            Directory.CreateDirectory(baseOutputDir);

        var memoryIdToBytes = new Dictionary<ushort, byte[]>();
        AFS2_File memory_awb = acb.GenerateAwbFile(false);
        string memoryOutputDir = Path.Combine(baseOutputDir, "MEMORY");
        if (memory_awb != null)
            foreach (var entry in memory_awb.Entries)
                memoryIdToBytes[entry.ID] = entry.bytes;

        var streamIdToBytes = new Dictionary<ushort, byte[]>();
        AFS2_File stream_awb = acb.GenerateAwbFile(true);
        string streamOutputDir = Path.Combine(baseOutputDir, "STREAM");
        if (stream_awb != null)
            foreach (var entry in stream_awb.Entries)
                streamIdToBytes[entry.ID] = entry.bytes;

        Cues = new Dictionary<uint, CueData>();
        List<TrackEntry> trackList = new List<TrackEntry>();
        foreach (var cue in acb.Cues)
            Cues.Add(cue.ID, null);

        HashSet<uint> inRange = new HashSet<uint>();
        if (this.ExtractionMode != "default" && !(this.MessageCues is null))
            foreach (uint cueId in this.MessageCues.Keys)
                if (this.ExtractionMode == "grouped")
                {
                    this.MessageCues[cueId].CueRange = this.GetCueRange(cueId);
                    for (uint i=this.MessageCues[cueId].CueRange.Lower; i<=this.MessageCues[cueId].CueRange.Upper; i++)
                        inRange.Add(i);
                }
                else if (this.ExtractionMode == "used")
                    inRange.Add(cueId);

        foreach (var cue in acb.Cues) {

            // TODO: will need to update this when ECS is integrated
            if (this.ExtractionMode != "default" && !inRange.Contains(cue.ID))
            {
                Cues.Remove(cue.ID);
                continue;
            }
            /*else if (this.ExtractionMode == "grouped" && inRange.Contains(cue.ID) && (cue.ID < this.MessageCues[cue.ID].CueRange.Lower || cue.ID > this.MessageCues[cue.ID].CueRange.Upper))
            {
                Cues.Remove(cue.ID);
                continue;
            }*/

            var newCue = new CueData(cue.ID, cue.Name);
            List<ACB_Waveform> waveforms = acb.GetWaveformsFromCue(cue);
            for (int i = 0; i < waveforms.Count; i++) {
                string outputPath = Path.Combine(baseOutputDir, String.Format("{0}-{1}.{2}.{3}", cue.ID, i, cue.Name, waveforms[i].EncodeType));
                if (waveforms[i].IsStreaming)
                    File.WriteAllBytes(outputPath, streamIdToBytes[waveforms[i].AwbId]);
                else
                    File.WriteAllBytes(outputPath, memoryIdToBytes[waveforms[i].AwbId]);
                try
                {
                    string[] vgcli_args = new[] {"-i", outputPath, "-o", outputPath, "--keycode", "9923540143823782"};
                    Convert.ConvertFile(CliArguments.Parse(vgcli_args));
                    byte[] decryptedBytes = File.ReadAllBytes(outputPath);
                    decryptedBytes[19] = 0x00;
                    File.WriteAllBytes(outputPath, decryptedBytes);
                    newCue.AddTrack(i, waveforms[i].IsStreaming, waveforms[i].AwbId, outputPath);
                    trackList.Add(new TrackEntry(cue.ID, cue.Name, i+1, waveforms[i].IsStreaming, waveforms[i].AwbId));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            //Cues.Add(cue.ID, newCue);
            Cues[cue.ID] = newCue;
        }

        foreach (var track in trackList)
        {
            (uint Lower, uint Upper) cueRange = this.GetCueRange(track.CueId);
            track.CueRange                    = $"{cueRange.Lower}-{cueRange.Upper}";
        }

        TrackList = new ObservableCollection<TrackEntry>(trackList);

        // SYSTEM and _SE should be skipped
        if (!(this.MessageCues is null))
            foreach (TrackEntry track in this.TrackList)
                if (this.MessageCues.ContainsKey((ushort)track.CueId))
                {
                    if (track.Usage is null)
                        track.Usage = this.MessageCues[(ushort)track.CueId].Stringification;
                    else
                        track.Usage += ", " + this.MessageCues[(ushort)track.CueId].Stringification;
                }

    }

    public (uint Lower, uint Upper) GetCueRange(uint cueId)
    {
        (uint Lower, uint Upper) cueRange = (cueId, cueId);
        while (true)
        {
            if (!this.Cues.ContainsKey(cueRange.Lower))
            {
                cueRange.Lower += 1;
                break;
            } else
                cueRange.Lower -= 1;
        }
        while (true)
        {
            if (!this.Cues.ContainsKey(cueRange.Upper))
            {
                cueRange.Upper -= 1;
                break;
            } else
                cueRange.Upper += 1;
        }
        return cueRange;
    }

}
