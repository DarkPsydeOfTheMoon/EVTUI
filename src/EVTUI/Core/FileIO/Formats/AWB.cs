using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

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

    public string CueRange { get; set; }
    public uint CueId      { get; set; }
    public string CueName  { get; set; }
    public int TrackIndex  { get; set; }
    public string AwbMode  { get; set; }
    public int AwbId       { get; set; }
}

public class ACWBData
{

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public string AcbPath { get; }
    public Dictionary<uint, CueData> Cues { get; }
    public ObservableCollection<TrackEntry> TrackList { get; }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public ACWBData(string filename)
    {
        AcbPath = filename;
        ACB_File acb = ACB_File.Load(AcbPath);

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
        foreach (var cue in acb.Cues) {
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
                    trackList.Add(new TrackEntry(cue.ID, cue.Name, i, waveforms[i].IsStreaming, waveforms[i].AwbId));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            Cues.Add(cue.ID, newCue);
        }

        foreach (var track in trackList)
        {
            uint[] cueRange = this.GetCueRange(track.CueId);
            track.CueRange = $"{cueRange[0]}-{cueRange[1]}";
        }

        TrackList = new ObservableCollection<TrackEntry>(trackList);

    }

    public uint[] GetCueRange(uint cueId)
    {
        uint[] ret = [cueId, cueId];
        while (true)
        {
            if (!Cues.ContainsKey(ret[0]))
            {
                ret[0] += 1;
                break;
            } else
                ret[0] -= 1;
        }
        while (true)
        {
            if (!Cues.ContainsKey(ret[1]))
            {
                ret[1] -= 1;
                break;
            } else
                ret[1] += 1;
        }
        return ret;
    }

}
