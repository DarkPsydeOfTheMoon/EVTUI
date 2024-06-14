using System;
using System.Collections.Generic;
using LibVLCSharp.Shared;

namespace EVTUI;

public class AudioManager
{

    /////////////////////////////
    // *** PRIVATE MEMBERS *** //
    /////////////////////////////
    private LibVLC      libVLC;
    private MediaPlayer mediaPlayer;
    private string      CurrentTrack;

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public string?                      ActiveACB = null; //{ get; set; }
    public Dictionary<string, ACWBData> AudioCueFiles { get; }
    public List<string>                 AcbList       { get; }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public AudioManager()
    {
        AudioCueFiles = new Dictionary<string, ACWBData>();
        AcbList       = new List<string>();
        libVLC        = new LibVLC();
        mediaPlayer   = new MediaPlayer(libVLC);
    }

    //public void UpdateAudioCueFiles(List<string> acbPaths, string modPath, Dictionary<(ushort, bool), (string, string)> eventCues)
    public void UpdateAudioCueFiles(List<string> acbPaths, string modPath, AudioCues eventCues)
    {
        this.AudioCueFiles.Clear();
        this.AcbList.Clear();
        foreach (var acbPath in acbPaths)
        {
            var soundFile = new ACWBData(acbPath, eventCues);
            if (soundFile.Cues is null)
                continue;
            //if (!acbPath.Contains("_SE"))
            //soundFile.UpdateUsage(eventCues);
            string key = acbPath.Substring((modPath.Length+1), acbPath.Length-(modPath.Length+1));
            this.AudioCueFiles[key] = soundFile;
            this.AcbList.Add(key);
        }
        if (acbPaths.Count > 0)
            this.ActiveACB = this.AcbList[0];
        else
            this.ActiveACB = null;
    }

    public void PlayCueTrack(uint cueId, int trackIndex)
    {
        string adxPath = this.AudioCueFiles[this.ActiveACB].Cues[cueId].Tracks[trackIndex-1].AdxPath;
        if (this.CurrentTrack != adxPath || this.mediaPlayer.State.ToString() == "Ended")
        {
            this.mediaPlayer.Media = new Media(libVLC, new Uri(adxPath));
            this.CurrentTrack = adxPath;
        }
        if (this.mediaPlayer.IsPlaying)
            this.mediaPlayer.Pause();
        else
            this.mediaPlayer.Play();
    }

}
