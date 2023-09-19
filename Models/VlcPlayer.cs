using ShareInstances;
using ShareInstances.Instances;

using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace IldMusic.VlcPlayer;
public class VlcPlayer : IPlayer
{
    public Guid PlayerId => Guid.NewGuid();
    public string PlayerName => "Vlc Player";

    public Track? CurrentTrack {get; private set;} = null;
    public Playlist? CurrentPlaylist { get; private set;} = null;
    private Media currentMedia = null;

    #region VLCSharp Instances
    private static readonly LibVLC _vlc = new LibVLC(); 
    private static MediaPlayer _mediaPlayer = new(_vlc);
    #endregion

    #region Player Triggers
    public bool IsSwipe {get; private set;} = false;
    public bool IsEmpty {get; private set;} = true;
    public bool ToggleState {get; private set;} = false;
    public int PlaylistPoint {get; private set;} = 0;
    private bool IsPlaylist = false;

    #endregion

    #region Time Presenters
    public TimeSpan TotalTime {get; private set;}
    public TimeSpan CurrentTime 
    {
        get => TimeSpan.FromMilliseconds(_mediaPlayer.Time);
        set => _mediaPlayer.SeekTo(value);
    }
    #endregion

    #region Volume Presenters
    public float MaxVolume {get; private set;} = 100;
    public float MinVolume {get; private set;} = 0;
    public float CurrentVolume 
    {
        get => _mediaPlayer.Volume;
        set => _mediaPlayer.Volume = (int)value;
    }
    #endregion

    #region Actions
    private Action notifyAction;
    #endregion

    #region Events
    private event Action ShuffleCollection;
    public event Action TrackStarted;
    #endregion

    #region constructor
    public VlcPlayer()
    {}
    #endregion


    #region Player Inits
    public void SetNotifier(Action callback)
    {
        notifyAction = callback;
    }
 
    public void DropTrack(Track track)
    {
        CleanCurrentState();
            
        CurrentTrack = track;
        IsEmpty = false;

        TotalTime = track.Duration;
        TrackStarted?.Invoke();
        currentMedia = new Media (_vlc, new Uri(track.Pathway.ToString()));
        _mediaPlayer = new(currentMedia);

    }
       
    public void DropPlaylist(Playlist playlist, int index=0)
    {
        CleanCurrentState();
        
        PlaylistPoint = index;
        var startTrack = playlist[index];
        TotalTime = startTrack.Duration;
        CurrentTrack = startTrack;
        CurrentPlaylist = playlist;

        currentMedia = new Media (_vlc, new Uri(startTrack.Pathway.ToString()));
        _mediaPlayer = new(currentMedia);

        IsEmpty = false;
        IsPlaylist = true;
    }

    public void DropNetworkStream(ReadOnlyMemory<char> uri)
    {}

    private void CleanCurrentState()
    {
        ToggleState = false;
        notifyAction?.Invoke();
        _mediaPlayer.Stop();

        if (currentMedia != null)
        {
            currentMedia.Dispose();
            currentMedia = null;
        }
            
        CurrentTrack = null;
        PlaylistPoint = 0; 
    }

    public async Task Toggle()
    {
        if (!_mediaPlayer.IsPlaying)
        {
            ToggleState = true;
            notifyAction?.Invoke();
            _mediaPlayer.Play();

            while(_mediaPlayer.Position < 0.99f);

            ToggleState = false;
            notifyAction?.Invoke();
            _mediaPlayer.Stop();
        }
        else
        {
            ToggleState = false;
            notifyAction?.Invoke();
            _mediaPlayer.Pause();
        }
    }

    public async Task Stop()
    {
        await Task.Run(() => 
        {
            ToggleState = false;
            notifyAction?.Invoke();
            _mediaPlayer.Stop();
        });
    }

    public async Task Repeat()
    {}

    public async Task SkipPrev()
    {}

    public async Task SkipNext()
    {}

    public async Task Shuffle()
    {}

    public async Task ChangeVolume(float volume)
    {}


    public async void DropPrevious()
    {
        await Task.Run (() => {
            if ((IsSwipe) && (!IsEmpty))
                DropMediaInstance(false);
        });
    }

    public async void DropNext()
    {
        await Task.Run(() => {
            if((IsSwipe) && (!IsEmpty))
                DropMediaInstance(true);
        });
    }

    private void SetNewMediaInstance(bool direct)
    {
        CleanCurrentState(); 
        DragPointer(direct);

        var newTrack = CurrentPlaylist?[PlaylistPoint];
        TotalTime = (TimeSpan)newTrack?.Duration;
        CurrentTrack = newTrack;

        currentMedia = new Media (_vlc, new Uri(CurrentTrack?.Pathway.ToString()));
        _mediaPlayer = new(currentMedia);

        IsEmpty = false;
        IsPlaylist = true;
    }

    private void DragPointer(bool direction)
    {
        if (direction)
        {
            if (PlaylistPoint == CurrentPlaylist?.Tracky.Count - 1)
                PlaylistPoint = 0;
            else
                PlaylistPoint++;
        }
        else 
        {
            if (PlaylistPoint == 0)
                PlaylistPoint = (int)(CurrentPlaylist?.Tracky.Count - 1);
            else
                PlaylistPoint--;
        }
    }
    #endregion
}
