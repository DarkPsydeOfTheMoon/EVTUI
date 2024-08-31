using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using LibVLCSharp.Shared;

namespace EVTUI;

public class AudioManager
{

    /////////////////////////////
    // *** PRIVATE MEMBERS *** //
    /////////////////////////////
    private LibVLC                       libVLC;
    private MediaPlayer                  mediaPlayer;
    private MemoryStream                 Stream;
    private (uint CueId, int TrackIndex) CurrentTrack;
    private static ulong                 KeyCode = 9923540143823782;

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public string?                    ActiveACB = null;
    public Dictionary<string, ACB>    AudioCueFiles { get; }
    public List<string>               AcbList       { get; }
    public Dictionary<string, List<string>> AcbByType     { get; }

    public List<uint> CueIds
    {
        get
        {
           if (this.ActiveACB is null)
               return new List<uint>{0};
           else
               return this.AudioCueFiles[this.ActiveACB].CueIds; 
        }
    }

    public void SetActiveACBType(string acbType)
    {
        if (this.AcbByType.ContainsKey(acbType) && this.AcbByType[acbType].Count > 0)
            this.ActiveACB = this.AcbByType[acbType][0];
        else
            this.ActiveACB = null;
    }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public AudioManager()
    {
        AudioCueFiles = new Dictionary<string, ACB>();
        AcbList       = new List<string>();

        AcbByType = new Dictionary<string, List<string>>()
        {
           ["BGM"]    = new List<string>(),
           ["System"] = new List<string>(),
           ["SFX"]    = new List<string>(),
           ["Field"]  = new List<string>(),
           ["Common"] = new List<string>(),
           ["Voice"]  = new List<string>(),
        };

        libVLC        = new LibVLC();
        mediaPlayer   = new MediaPlayer(libVLC);
    }

    public void UpdateAudioCueFiles(List<(string ACB, string? AWB)> acwbPaths, string modPath, AudioCues eventCues)
    {
        this.AudioCueFiles.Clear();
        this.AcbList.Clear();
        foreach (string key in this.AcbByType.Keys)
            this.AcbByType[key].Clear();
        if (!(acwbPaths is null))
        {
            object _lock = new();
            Parallel.ForEach(acwbPaths, acwbPath =>
            {
                ACB soundFile = new ACB(acwbPath.ACB, eventCues, acwbPath.AWB);
                if (!(soundFile.Cues is null))
                {
                    Console.WriteLine(acwbPath.ACB);
                    string key = acwbPath.ACB.Substring((modPath.Length+1), acwbPath.ACB.Length-(modPath.Length+1));
                    lock (_lock)
                    {
                        if (key.EndsWith("BGM.ACB"))
                            this.AcbByType["BGM"].Add(key);
                        else if (key.EndsWith("SYSTEM.ACB"))
                            this.AcbByType["System"].Add(key);
                        else if (key.EndsWith("VOICE_SINGLEWORD.ACB"))
                            this.AcbByType["Common"].Add(key);
                        else if (Regex.IsMatch(key, "E[0-9][0-9][0-9]_[0-9][0-9][0-9]\\.ACB$"))
                            this.AcbByType["Voice"].Add(key);
                        else if (Regex.IsMatch(key, "E[0-9][0-9][0-9]_[0-9][0-9][0-9]_SE\\.ACB$"))
                            this.AcbByType["SFX"].Add(key);
                        else if (Regex.IsMatch(key, "F[0-9][0-9][0-9]_[0-9][0-9][0-9]\\.ACB$"))
                            this.AcbByType["Field"].Add(key);
                        this.AudioCueFiles[key] = soundFile;
                        this.AcbList.Add(key);
                    }
                }
            });
        }
        this.AcbList.Sort();
        if (this.AcbList.Count > 0)
            this.ActiveACB = this.AcbList[0];
        else
            this.ActiveACB = null;
        foreach (string key in this.AcbByType.Keys)
            this.AcbByType[key].Sort((x, y) => x.CompareTo(y));
        Console.WriteLine(this.AcbByType["Voice"].Count);
    }

    public void PlayCueTrack(uint cueId, int trackIndex)
    {
        if (this.CurrentTrack != (cueId, trackIndex) || this.mediaPlayer.State.ToString() == "Ended")
        {
            if (!(this.Stream is null))
                this.Stream.Dispose();
            byte[] trackBytes = this.AudioCueFiles[this.ActiveACB].GetTrackBytes(cueId, trackIndex, AudioManager.KeyCode);
            if (!(trackBytes is null))
            {
                this.Stream = new MemoryStream(this.AudioCueFiles[this.ActiveACB].GetTrackBytes(cueId, trackIndex, AudioManager.KeyCode));
                this.mediaPlayer.Media = new Media(libVLC, new StreamMediaInput(this.Stream));
            }
            this.CurrentTrack = (cueId, trackIndex);
        }
        if (this.mediaPlayer.IsPlaying)
            this.mediaPlayer.Pause();
        else
            this.mediaPlayer.Play();
    }

}
