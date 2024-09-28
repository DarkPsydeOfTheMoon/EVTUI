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

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public string?                          ActiveACB = null;
    public Dictionary<string, ACB>          AudioCueFiles { get; }
    public List<string>                     AcbList       { get; }
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

    private static Dictionary<string, Regex> Patterns = new Dictionary<string, Regex>()
    {    
        ["BGM"]    = new Regex("BGM\\.ACB$",                                 RegexOptions.IgnoreCase),
        ["System"] = new Regex("SYSTEM\\.ACB$",                              RegexOptions.IgnoreCase),
        ["Common"] = new Regex("VOICE_SINGLEWORD\\.ACB$",                    RegexOptions.IgnoreCase),
        ["Voice"]  = new Regex("E[0-9][0-9][0-9]_[0-9][0-9][0-9]\\.ACB$",    RegexOptions.IgnoreCase),
        ["SFX"]    = new Regex("E[0-9][0-9][0-9]_[0-9][0-9][0-9]_SE\\.ACB$", RegexOptions.IgnoreCase),
        ["Field"]  = new Regex("F[0-9][0-9][0-9]_[0-9][0-9][0-9]\\.ACB$",    RegexOptions.IgnoreCase),
    };

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
                // stuff to be passed to the ACB object
                string extractionMode = "default";
                Dictionary<uint, MessageCue>? messageCues = null;
                LocaleCues locale = (acwbPath.ACB.Contains("_J")) ? eventCues.JpCues : eventCues.EnCues;
                if (AudioManager.Patterns["Common"].IsMatch(acwbPath.ACB))
                {
                    extractionMode = "grouped";
                    messageCues    = locale.Common;
                }
                else if (AudioManager.Patterns["Field"].IsMatch(acwbPath.ACB))
                    messageCues    = locale.Field;
                else if (AudioManager.Patterns["Voice"].IsMatch(acwbPath.ACB))
                    messageCues    = locale.EventVoice;
                else if (AudioManager.Patterns["SFX"].IsMatch(acwbPath.ACB))
                    messageCues    = locale.EventSFX;
                else if (AudioManager.Patterns["System"].IsMatch(acwbPath.ACB))
                    extractionMode = "used";

                ACB soundFile = new ACB(acwbPath.ACB, messageCues, extractionMode, acwbPath.AWB);
                if (!(soundFile.Cues is null))
                {
                    string key = acwbPath.ACB.Substring((modPath.Length+1), acwbPath.ACB.Length-(modPath.Length+1));
                    lock (_lock)
                    {
                        foreach (string typeKey in AudioManager.Patterns.Keys)
                        {
                            if (AudioManager.Patterns[typeKey].IsMatch(key))
                            {
                                this.AcbByType[typeKey].Add(key);
                                break;
                            }
                        }
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

    public void PlayCueTrack(uint cueId, int trackIndex, ulong keyCode)
    {
        if (this.CurrentTrack != (cueId, trackIndex) || this.mediaPlayer.State.ToString() == "Ended")
        {
            if (!(this.Stream is null))
                this.Stream.Dispose();
            byte[] trackBytes = this.AudioCueFiles[this.ActiveACB].GetTrackBytes(cueId, trackIndex, keyCode);
            if (!(trackBytes is null))
            {
                this.Stream = new MemoryStream(this.AudioCueFiles[this.ActiveACB].GetTrackBytes(cueId, trackIndex, keyCode));
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
