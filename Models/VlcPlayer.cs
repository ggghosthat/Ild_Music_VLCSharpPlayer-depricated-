using ShareInstances;
using ShareInstances.Instances;

using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace IldMusic.VlcPlayer;
public class VlcPlayer : IPlayer
{
    private static readonly VlcPlayerService _playerService = new();
    public Guid PlayerId => Guid.NewGuid();
    public string PlayerName => "Vlc Player";

    public Track? CurrentTrack {get; private set;} = null;
    public Playlist? CurrentPlaylist { get; private set;} = null;

    #region Player Triggers
    public bool IsEmpty => _playerService.IsEmpty;
    public bool ToggleState => _playerService.ToggleState;
    public int PlaylistPoint {get; private set;} = 0;


    public bool IsSwipe {get; private set;} = false;
    public bool IsPlaylistLoop {get; set;} = false;
    private bool IsPlaylist = false;
    #endregion

    #region Time Presenters
    public TimeSpan TotalTime => _playerService.TotalTime;
    public TimeSpan CurrentTime 
    {
        get => _playerService.CurrentTime;
        set => _playerService.CurrentTime = value;
    }
    #endregion

    #region Volume Presenters
    public float MaxVolume {get; private set;} = 100;
    public float MinVolume {get; private set;} = 0;
    public float CurrentVolume 
    {
        get => _playerService.CurrentVolume;
        set => _playerService.CurrentVolume = (int)value;
    }
    #endregion

    #region Events
    private event Action ShuffleCollection;
    #endregion

    #region constructor
    public VlcPlayer()
    {}
    #endregion


    #region Player Inits
    public void SetNotifier(Action callback) =>
        _playerService.DefineCallback(callback);

    public async Task DropTrack(Track track)
    {            
        CurrentTrack = track;
        await _playerService.SetTrack(track);
        //mediator tag for a toogle event
    }
       
    public async Task DropPlaylist(Playlist playlist, int index=0)
    { 
        PlaylistPoint = index;
        var startTrack = playlist[index];
        CurrentTrack = startTrack;
        CurrentPlaylist = playlist;
        IsSwipe = true;
        IsPlaylist = true;

        
    }

    public async Task DropNetworkStream(ReadOnlyMemory<char> uri)
    {}

    
    public async Task Toggle()
    {
        _playerService.Toggle().Start();
    }

    public async Task Stop()
    {
        IsSwipe = false;
        IsPlaylist = false;
        await _playerService.Stop();
    }

    public async Task Repeat()
    {}

    public async Task SkipPrev()
    {}

    public async Task SkipNext()
    {}

    public async Task Shuffle()
    {}


    public async void DropPrevious()
    {
        if (IsPlaylist)
            await SetNewMediaInstance(false);
    }

    public async void DropNext()
    {
        if(IsPlaylist)
            await SetNewMediaInstance(true); 
    }

    private async Task SetNewMediaInstance(bool direct)
    {
        if(CurrentPlaylist is not null)
        {
            DragPointer(direct);
   
            if(IsSwipe)
            {
                var newTrack = (Track)CurrentPlaylist?[PlaylistPoint];
                CurrentTrack = newTrack;
                await _playerService.SetTrack(newTrack);

                //mediator for toogle event
            }
        }
    }

    private void DragPointer(bool direction)
    {
        if (direction)
        {
            if (PlaylistPoint == CurrentPlaylist?.Tracky.Count - 1)
            {
                if (!IsPlaylistLoop)
                {
                    IsSwipe = false;
                }
            }
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
