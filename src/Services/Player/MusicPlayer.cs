using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;
using Android.Util;
using Idunas.DanceMusicPlayer.Models;
using Idunas.DanceMusicPlayer.Util;
using Java.Lang;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Idunas.DanceMusicPlayer.Services.Player
{
    public class MusicPlayer : IMusicPlayer
    {
        private MediaSessionCompat _mediaSession;
        private readonly AudioManager.IOnAudioFocusChangeListener _audioFocusChangeListener;
        private Context _context;

        private SemaphoreSlim _loadingSemaphore = new SemaphoreSlim(1);
        private AudioFocusRequestClass _audioFocusRequest;
        private PlaybackParams _playbackParams;
        private MediaPlayer _mediaPlayer;
        private TaskCompletionSource<bool> _preparationTcs;
        private System.Timers.Timer _positionReportTimer;
        private Playlist _playlist = null;
        private int _currentSongIndex = -1;
        private Bitmap _defaultArtwork = null;
        private Bitmap _currentAlbumArtwork = null;


        public MusicPlayer(
            Context context,
            MediaSessionCompat mediaSession,
            AudioManager.IOnAudioFocusChangeListener audioFocusChangeListener)
        {
            _context = context;
            _mediaSession = mediaSession;
            _audioFocusChangeListener = audioFocusChangeListener;
            _defaultArtwork = _currentAlbumArtwork = BitmapFactory.DecodeResource(_context.Resources, Resource.Mipmap.ic_launcher);

            _positionReportTimer = new System.Timers.Timer(1000);
            _positionReportTimer.Elapsed += HandlePositionReportTimerElapsed;

            UpdateMediaSession();
            UpdateNotifiation();
        }

        #region --- IMusicPlayer implementation

        public event EventHandler<long> DurationChanged;
        public event EventHandler<long> PositionChanged;
        public event EventHandler<PlayerState> StateChanged;
        public event EventHandler<Song> SongChanged;

        public PlayerState State
        {
            get;
            private set;
        }

        public int Duration
        {
            get
            {
                switch (State)
                {
                    case PlayerState.Unknown:
                    case PlayerState.Loading:
                    default:
                    {
                        return 0;
                    }
                    case PlayerState.Ready:
                    case PlayerState.Playing:
                    case PlayerState.Paused:
                    case PlayerState.Stopped:
                    {
                        return _mediaPlayer?.Duration ?? 0;
                    }
                }
            }
        }


        public int Position => _mediaPlayer?.CurrentPosition ?? 0;

        public bool IsLooping { get; set; }

        public bool HasNextSong => _currentSongIndex < _playlist?.Songs.Count - 1;

        public bool HasPreviousSong => _currentSongIndex > 0;

        public Song CurrentSong
        {
            get
            {
                if (_playlist == null || _currentSongIndex >= _playlist.Songs.Count)
                {
                    return null;
                }

                return _playlist.Songs[_currentSongIndex];
            }
        }

        public async Task Load(Song song, Playlist playlist)
        {
            try
            {
                await _loadingSemaphore.WaitAsync();

                // Reset the currently playing song
                SetPlayerState(PlayerState.Loading);

                if (_mediaPlayer == null)
                {
                    CreatePlayer();
                }
                else if (_mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.Stop();
                }

                _mediaPlayer.Reset();

                // Swap out song / playlist
                _playlist = playlist;
                _currentSongIndex = _playlist.Songs.IndexOf(song);

                // Extract artwork from mp3
                _currentAlbumArtwork = GetAlbumArtwork();

                SongChanged?.Invoke(this, song);

                // Prepare and play the next song
                await _mediaPlayer.SetDataSourceAsync(song.FilePath);
                await PreparePlayer();

                _loadingSemaphore.Release();

                await Play(true);
            }
            catch (System.Exception ex)
            {
                Log.Error(nameof(PreparePlayer), Throwable.FromException(ex), "Exception loading song");
                ReleasePlayer();
            }
        }

        private Bitmap GetAlbumArtwork()
        {
            if (CurrentSong == null)
            {
                return _defaultArtwork;
            }

            try
            {
                var mmReceiver = new MediaMetadataRetriever();
                mmReceiver.SetDataSource(CurrentSong.FilePath);
                var bitmapData = mmReceiver.GetEmbeddedPicture();
                if (bitmapData == null)
                {
                    return _defaultArtwork;
                }

                return BitmapFactory.DecodeByteArray(bitmapData, 0, bitmapData.Length);
            }
            catch
            {
                return _defaultArtwork;
            }
        }

        public async Task Play(bool requestAudioFocus)
        {
            if (requestAudioFocus)
            {
                if (!RequestAudioFocus())
                {
                    MessageService.ShowMessage(_context.GetString(Resource.String.error_could_not_get_audio_focus));
                    return;
                }
            }

            await PlayInternal();
        }

        public void Pause()
        {
            if (State == PlayerState.Playing)
            {
                _positionReportTimer.Stop();
                _mediaPlayer.Pause();
                SetPlayerState(PlayerState.Paused);
            }
        }

        public void Stop()
        {

        }

        public async Task PlayNextSong()
        {
            if (!HasNextSong)
            {
                return;
            }

            await Load(_playlist.Songs[_currentSongIndex + 1], _playlist);
        }

        public async Task PlayPreviousSong()
        {
            if (!HasPreviousSong)
            {
                return;
            }

            await Load(_playlist.Songs[_currentSongIndex - 1], _playlist);
        }

        public void ChangeSpeed(float speed)
        {
            // Generate the new playback-parameters with the new speed
            _playbackParams = new PlaybackParams().SetSpeed(speed);

            // We can only set them instantly if the player is currently playing
            // as changing the playback-params will cause the player to start
            // playing. Otherwise the params will be applied the next time the player
            // is started
            if (State == PlayerState.Playing)
            {
                _mediaPlayer.PlaybackParams = _playbackParams;
            }
        }

        public void SeekTo(long position)
        {
            if (State == PlayerState.Stopped)
            {
                State = PlayerState.Paused;
            }

            if (State != PlayerState.Playing &&
                State != PlayerState.Paused)
            {
                return;
            }

            _mediaPlayer.SeekTo(position, MediaPlayerSeekMode.PreviousSync);
        }

        public void LowerVolume()
        {
            _mediaPlayer?.SetVolume(0.3f, 0.3f);
        }

        public void Dispose()
        {
            if (_audioFocusRequest != null)
            {
                var audioManager = (AudioManager)_context.GetSystemService(Context.AudioService);
                audioManager.AbandonAudioFocusRequest(_audioFocusRequest);
                _audioFocusRequest = null;
            }

            ReleasePlayer();
        }

        #endregion

        #region --- Event handlers & private functionality

        private bool RequestAudioFocus()
        {
            var audioManager = (AudioManager)_context.GetSystemService(Context.AudioService);
            _audioFocusRequest = new AudioFocusRequestClass.Builder(AudioFocus.Gain)
                .SetAudioAttributes(BackgroundAudioService.PlaybackAttributes)
                .SetAcceptsDelayedFocusGain(true)
                .SetOnAudioFocusChangeListener(_audioFocusChangeListener)
                .Build();

            return audioManager.RequestAudioFocus(_audioFocusRequest) == AudioFocusRequest.Granted;
        }

        private async Task PlayInternal()
        {
            if (Duration <= 0)
            {
                return;
            }

            switch (State)
            {
                case PlayerState.Unknown:
                case PlayerState.Loading:
                case PlayerState.Playing:
                {
                    _mediaPlayer.SetVolume(1.0f, 1.0f);
                    return;
                }
                case PlayerState.Paused:
                case PlayerState.Ready:
                {
                    _positionReportTimer.Start();

                    // Apply playback-paramters if they did change in the meantime
                    if (_playbackParams != null && _mediaPlayer.PlaybackParams != _playbackParams)
                    {
                        _mediaPlayer.PlaybackParams = _playbackParams;
                    }

                    _mediaPlayer.SetVolume(1.0f, 1.0f);
                    _mediaPlayer.Start();
                    SetPlayerState(PlayerState.Playing);
                    return;
                }
                case PlayerState.Stopped:
                {
                    // Play from the beginning of the playlist
                    await Load(_playlist.Songs[0], _playlist);
                    return;
                }
            }
        }

        private void HandlePositionReportTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (State != PlayerState.Playing)
            {
                return;
            }

            FirePositionChanged(_mediaPlayer.CurrentPosition);

            // Check if we need to seek to a previous position
            // due to looping
            var currentSong = CurrentSong;
            if (currentSong.IsLooping &&
                currentSong.LoopMarkerEnd != null &&
                _mediaPlayer.CurrentPosition >= currentSong.LoopMarkerEnd)
            {
                SeekTo(currentSong.LoopMarkerStart ?? 0);
                FirePositionChanged(_mediaPlayer.CurrentPosition);
            }
        }

        private async void HandlePlayerCompletion(object sender, EventArgs e)
        {
            var currentSong = CurrentSong;
            if (currentSong.IsLooping)
            {
                SeekTo(currentSong.LoopMarkerStart ?? 0);
                _mediaPlayer.Start();
                return;
            }

            if (HasNextSong)
            {
                await PlayNextSong();
                return;
            }

            _positionReportTimer.Stop();
            FirePositionChanged(Duration);
            SetPlayerState(PlayerState.Stopped);
        }

        private void HandlePlayerPrepared(object sender, EventArgs e)
        {
            _preparationTcs?.TrySetResult(true);
        }

        private void SetPlayerState(PlayerState state)
        {
            State = state;
            StateChanged?.Invoke(this, State);

            UpdateMediaSession();
            UpdateNotifiation();
        }

        private void FirePositionChanged(long position)
        {
            UpdateMediaSession();
            UpdateNotifiation();

            PositionChanged?.Invoke(this, position);
        }

        private async Task PreparePlayer()
        {
            // Prepare the player
            _preparationTcs = new TaskCompletionSource<bool>();
            _mediaPlayer.PrepareAsync();

            await _preparationTcs.Task;
            _preparationTcs = null;
            SetPlayerState(PlayerState.Ready);

            DurationChanged?.Invoke(this, Duration);
        }

        private void CreatePlayer()
        {
            ReleasePlayer();

            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.SetWakeMode(_context, WakeLockFlags.Partial);
            _mediaPlayer.SetAudioAttributes(BackgroundAudioService.PlaybackAttributes);
            _mediaPlayer.Completion += HandlePlayerCompletion;
            _mediaPlayer.Prepared += HandlePlayerPrepared;
        }

        private void ReleasePlayer()
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Completion -= HandlePlayerCompletion;
                _mediaPlayer.Prepared -= HandlePlayerPrepared;
                _mediaPlayer.Release();
                _mediaPlayer = null;
            }
        }

        private void UpdateMediaSession()
        {
            // Update media state
            var stateBuilder = new PlaybackStateCompat.Builder();

            if (State == PlayerState.Playing)
            {
                stateBuilder
                    .SetActions(
                        PlaybackStateCompat.ActionPlayPause |
                        PlaybackStateCompat.ActionPause |
                        PlaybackStateCompat.ActionSkipToPrevious |
                        PlaybackStateCompat.ActionSkipToNext)
                    .SetState(PlaybackStateCompat.StatePlaying, Position, _mediaPlayer.PlaybackParams.Speed);
            }
            else
            {
                stateBuilder
                    .SetActions(
                        PlaybackStateCompat.ActionPlayPause |
                        PlaybackStateCompat.ActionPlay |
                        PlaybackStateCompat.ActionSkipToPrevious |
                        PlaybackStateCompat.ActionSkipToNext)
                    .SetState(PlaybackStateCompat.StatePaused, Position, _mediaPlayer?.PlaybackParams?.Speed ?? 0);
            }

            _mediaSession.SetPlaybackState(stateBuilder.Build());
            _mediaSession.SetMetadata(new MediaMetadataCompat.Builder()
                .PutBitmap(MediaMetadataCompat.MetadataKeyArt, _currentAlbumArtwork)
                .PutString(MediaMetadataCompat.MetadataKeyTitle, CurrentSong?.Name ?? _context.GetString(Resource.String.no_song_selected))
                .PutLong(MediaMetadataCompat.MetadataKeyDuration, Duration)
                .Build());
        }

        private void UpdateNotifiation()
        {
            var notification = MediaSessionHelper.GetNotification(_context, _mediaSession, this);
            NotificationManagerCompat.From(_context).Notify(BackgroundAudioService.SERVICE_RUNNING_NOTIFICATION_ID, notification);
        }

        #endregion
    }
}