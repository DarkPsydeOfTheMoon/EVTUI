using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Windows.Input;
using ReactiveUI;

namespace EVTUI.ViewModels;

public class AudioPanelViewModel : ViewModelBase
{
    /////////////////////////////
    // *** PRIVATE MEMBERS *** //
    /////////////////////////////
    private DataManager Config;
    private Dictionary<string, ACB> AudioCueFiles { get { return this.Config.AudioManager.AudioCueFiles; } }

    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    // For some reason, if you switch to the tab before loading an event, once you do, you won't be able to change the selection unless the list is copied rather than reference??
	// I tried so many alternatives and nothing worked except for this. I am baffled.
    public List<string> AcbList { get { return new List<String>(this.Config.AudioManager.AcbList); } }
    // ...Like, the below does not work. The dropdown gets populated but the selection is immutable. Why...?
    //public List<string> AcbList { get { return this.Config.AudioManager.AcbList; } }

    public ObservableCollection<TrackEntry> TrackList
    {
        get { return AudioCueFiles[ActiveACB].TrackList; }
    }

    public string ActiveACB
    { 
        get { return this.Config.AudioManager.ActiveACB; }
        set
        {
            this.Config.AudioManager.ActiveACB = value;
            // This is needed for the table contents to actually update!
            OnPropertyChanged(nameof(TrackList));
        }
    }

    public TrackEntry TrackSelection { get; set; }

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public AudioPanelViewModel(DataManager Config)
    {
        this.Config = Config;
    }

    public void PlaySelectedTrack()
    {
        if (this.TrackSelection != null)
            this.Config.AudioManager.PlayCueTrack(this.TrackSelection.CueId, this.TrackSelection.TrackIndex, this.Config.ProjectManager.AdxKey);
    }
}
