using System;
using System.Collections.Generic;
using System.IO;
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
    public string?                 ActiveACB = null;
    public Dictionary<string, ACB> AudioCueFiles { get; }
    public List<string>            AcbList       { get; }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public AudioManager()
    {
        AudioCueFiles = new Dictionary<string, ACB>();
        AcbList       = new List<string>();
        libVLC        = new LibVLC();
        mediaPlayer   = new MediaPlayer(libVLC);
    }

    public void UpdateAudioCueFiles(List<(string ACB, string? AWB)> acwbPaths, string modPath, AudioCues eventCues)
    {
        this.AudioCueFiles.Clear();
        this.AcbList.Clear();
        foreach (var acwbPath in acwbPaths)
        {
            ACB soundFile = new ACB(acwbPath.ACB, eventCues, acwbPath.AWB);
            if (soundFile.Cues is null)
                continue;
            Console.WriteLine(acwbPath.ACB);
            string key = acwbPath.ACB.Substring((modPath.Length+1), acwbPath.ACB.Length-(modPath.Length+1));
            this.AudioCueFiles[key] = soundFile;
            this.AcbList.Add(key);
        }
        if (acwbPaths.Count > 0)
            this.ActiveACB = this.AcbList[0];
        else
            this.ActiveACB = null;
    }

    public void PlayCueTrack(uint cueId, int trackIndex)
    {
        if (this.CurrentTrack != (cueId, trackIndex) || this.mediaPlayer.State.ToString() == "Ended")
        {
            if (!(this.Stream is null))
                this.Stream.Dispose();
            this.Stream = new MemoryStream(this.AudioCueFiles[this.ActiveACB].Cues[cueId].Tracks[trackIndex-1].GetBytes(AudioManager.KeyCode));
            this.mediaPlayer.Media = new Media(libVLC, new StreamMediaInput(this.Stream));
            this.CurrentTrack = (cueId, trackIndex);
        }
        if (this.mediaPlayer.IsPlaying)
            this.mediaPlayer.Pause();
        else
            this.mediaPlayer.Play();
    }

}
