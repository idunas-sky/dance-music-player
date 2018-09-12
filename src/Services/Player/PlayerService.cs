using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Idunas.DanceMusicPlayer.Models;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace Idunas.DanceMusicPlayer.Services.Player
{
    [Service]
    public class PlayerService : Service, IPlayerService
    {
        private const int SERVICE_RUNNING_NOTIFICATION_ID = 999;

        private PlaybackParams _playbackParams;
        private MediaPlayer _player;
        private TaskCompletionSource<bool> _preparationTcs;
        private Timer _positionReportTimer;
        private Playlist _playlist = null;
        private int _currentSongIndex = -1;

        public event EventHandler<int> PositionChanged;
        public event EventHandler<int> DurationChanged;
        public event EventHandler<PlayerState> StateChanged;
        public event EventHandler<Song> SongChanged;

        public IBinder Binder { get; private set; }

        #region --- Service startup / shutdown

        public PlayerService()
        {
            _positionReportTimer = new Timer(1000);
            _positionReportTimer.Elapsed += (sender, e) =>
            {
                if (State != PlayerState.Playing)
                {
                    return;
                }

                FirePositionChanged(_player.CurrentPosition);

                // Check if we need to seek to a previous position
                // due to looping
                var currentSong = _playlist.Songs[_currentSongIndex];
                if (currentSong.IsLooping &&
                    currentSong.LoopMarkerEnd != null && 
                    _player.CurrentPosition >= currentSong.LoopMarkerEnd)
                {
                    SeekTo(currentSong.LoopMarkerStart ?? 0);
                    FirePositionChanged(_player.CurrentPosition);
                }
            };
        }

        public override IBinder OnBind(Intent intent)
        {
            Binder = new PlayerServiceBinder(this);
            return Binder;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            var notification = new Notification.Builder(this)
                //var notification = new Notification.Builder(this, GetString(Resource.String.default_notification_channel_id))
                //.SetContentTitle(GetString(Resource.String.app_name))
                //.SetContentText("TODO: Currently playing ...")
                //.SetSmallIcon(Resource.Drawable.ic_music_note)
                //.SetOngoing(true)
                .Build();

            StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, notification);

            return base.OnStartCommand(intent, flags, startId);
        }

        public override void OnDestroy()
        {
            Binder = null;
            ReleasePlayer();

            base.OnDestroy();
        }

        #endregion

        #region --- Event handlers & private functionality

        private async void HandlePlayerCompletion(object sender, EventArgs e)
        {
            var currentSong = _playlist.Songs[_currentSongIndex];
            if (currentSong.IsLooping)
            {
                SeekTo(currentSong.LoopMarkerStart ?? 0);
                _player.Start();
                return;
            }

            if (HasNextSong)
            {
                await PlayNextSong();
                return;
            }

            _positionReportTimer.Stop();
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
        }

        private void FirePositionChanged(int position)
        {
            PositionChanged?.Invoke(this, position);
        }

        private async Task PreparePlayer()
        {
            _preparationTcs = new TaskCompletionSource<bool>();
            _player.PrepareAsync();

            await _preparationTcs.Task;
            _preparationTcs = null;
            SetPlayerState(PlayerState.Ready);

            DurationChanged?.Invoke(this, _player.Duration);
        }

        private void CreatePlayer()
        {
            ReleasePlayer();

            _player = new MediaPlayer();
            _player.Completion += HandlePlayerCompletion;
            _player.Prepared += HandlePlayerPrepared;
        }

        private void ReleasePlayer()
        {
            if (_player != null)
            {
                _player.Completion -= HandlePlayerCompletion;
                _player.Prepared -= HandlePlayerPrepared;
                _player.Release();
                _player = null;
            }
        }

        #endregion


        #region --- IPlayerService implementation

        public PlayerState State
        {
            get;
            private set;
        }

        public int Duration => _player?.Duration ?? 0;

        public int Position => _player?.CurrentPosition ?? 0;

        public bool IsLooping { get; set; }

        public bool HasNextSong => _currentSongIndex < _playlist?.Songs.Count - 1;

        public bool HasPreviousSong => _currentSongIndex > 0;

        public void Pause()
        {
            if (State == PlayerState.Playing)
            {
                _positionReportTimer.Stop();
                _player.Pause();
                SetPlayerState(PlayerState.Paused);
            }
        }

        public void Play()
        {
            if (State != PlayerState.Paused &&
                State != PlayerState.Ready)
            {
                return;
            }

            if (_player.Duration <= 0)
            {
                return;
            }

            _positionReportTimer.Start();

            // Apply playback-paramters if they did change in the meantime
            if (_playbackParams != null && _player.PlaybackParams != _playbackParams)
            {
                _player.PlaybackParams = _playbackParams;
            }
            _player.Start();
            SetPlayerState(PlayerState.Playing);
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
                _player.PlaybackParams = _playbackParams;
            }
        }

        public async Task Load(Song song, Playlist playlist)
        {
            // Reset the currently playing song
            SetPlayerState(PlayerState.Loading);

            if (_player == null)
            {
                CreatePlayer();
            }
            else if (_player.IsPlaying)
            {
                _player.Stop();
            }

            _player.Reset();

            // Swap out song / playlist
            _playlist = playlist;
            _currentSongIndex = _playlist.Songs.IndexOf(song);

            SongChanged?.Invoke(this, song);

            // Prepare and play the next song
            await _player.SetDataSourceAsync(song.FilePath);
            await PreparePlayer();
            Play();
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

        public void SeekTo(int position)
        {
            if (State != PlayerState.Playing &&
                State != PlayerState.Paused)
            {
                return;
            }

            _player.SeekTo(position);
        }

        #endregion


    }
}