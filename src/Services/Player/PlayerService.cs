using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Widget;
using Idunas.DanceMusicPlayer.Activities;
using Idunas.DanceMusicPlayer.Models;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace Idunas.DanceMusicPlayer.Services.Player
{
    [Service(Exported = false)]
    public class PlayerService : Service, IPlayerService
    {
        public const string MAIN_ACTION = "de.idunas.dancemusicplayer.action.main";
        public const string PLAY_PAUSE_ACTION = "de.idunas.dancemusicplayer.action.play";
        public const string NEXT_ACTION = "de.idunas.dancemusicplayer.action.next";
        public const string START_FOREGROUND_ACTION = "de.idunas.dancemusicplayer.action.startforeground";
        private const int SERVICE_RUNNING_NOTIFICATION_ID = 1;

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
            _positionReportTimer.Elapsed += HandlePositionReportTimerElapsed;
        }

        public override IBinder OnBind(Intent intent)
        {
            Binder = new PlayerServiceBinder(this);
            return Binder;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            switch (intent.Action)
            {
                case START_FOREGROUND_ACTION:
                {
                    CreateAndShowServiceNotification();
                    break;
                }
                case PLAY_PAUSE_ACTION:
                {
                    if (State == PlayerState.Playing)
                    {
                        Pause();
                        CreateAndShowServiceNotification();
                        break;
                    }
                    else
                    {
                        Play().ContinueWith(result => CreateAndShowServiceNotification());
                        break;
                    }
                }
                case NEXT_ACTION:
                {
                    PlayNextSong().ContinueWith(result => CreateAndShowServiceNotification());
                    break;
                }
            }

            return StartCommandResult.Sticky;
        }

        private void CreateAndShowServiceNotification()
        {
            // Build the intent that will show the app if the user taps our notification
            var showAppIntent = PendingIntent.GetActivity(
                this, 0, new Intent(this, typeof(MainActivity)).SetAction(MAIN_ACTION), 0);

            // Play / Pause event handler
            var playPauseIntent = PendingIntent.GetService(
                this, 0, new Intent(this, typeof(PlayerService)).SetAction(PLAY_PAUSE_ACTION), 0);

            // Next event handler
            var nextIntent = PendingIntent.GetService(
                this, 0, new Intent(this, typeof(PlayerService)).SetAction(NEXT_ACTION), 0);


            // Build the custom view
            var contentView = new RemoteViews(PackageName, Resource.Layout.Notification);
            contentView.SetOnClickPendingIntent(Resource.Id.btn_play_pause, playPauseIntent);
            contentView.SetOnClickPendingIntent(Resource.Id.btn_next, nextIntent);
            contentView.SetImageViewResource(
                Resource.Id.btn_play_pause,
                State == PlayerState.Playing ? Resource.Drawable.ic_pause : Resource.Drawable.ic_play);
            contentView.SetTextViewText(
                Resource.Id.lbl_song,
                CurrentSong?.Name ?? GetString(Resource.String.no_song_selected));

            var notification = new NotificationCompat.Builder(this, MainActivity.NOTIFICATION_CHANNEL_ID)
                .SetSmallIcon(Resource.Mipmap.ic_launcher)
                .SetCustomContentView(contentView)
                .SetStyle(new NotificationCompat.DecoratedCustomViewStyle())
                .SetContentIntent(showAppIntent)
                .SetOngoing(true)
                .Build();

            StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, notification);
        }

        public override void OnDestroy()
        {
            Binder = null;
            ReleasePlayer();

            base.OnDestroy();
        }

        #endregion

        #region --- Event handlers & private functionality

        private void HandlePositionReportTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (State != PlayerState.Playing)
            {
                return;
            }

            FirePositionChanged(_player.CurrentPosition);

            // Check if we need to seek to a previous position
            // due to looping
            var currentSong = CurrentSong;
            if (currentSong.IsLooping &&
                currentSong.LoopMarkerEnd != null &&
                _player.CurrentPosition >= currentSong.LoopMarkerEnd)
            {
                SeekTo(currentSong.LoopMarkerStart ?? 0);
                FirePositionChanged(_player.CurrentPosition);
            }
        }

        private async void HandlePlayerCompletion(object sender, EventArgs e)
        {
            var currentSong = CurrentSong;
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
            FirePositionChanged(_player.Duration);
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
            CreateAndShowServiceNotification();
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

        public void Pause()
        {
            if (State == PlayerState.Playing)
            {
                _positionReportTimer.Stop();
                _player.Pause();
                SetPlayerState(PlayerState.Paused);
            }
        }

        public async Task Play()
        {
            if (_player.Duration <= 0)
            {
                return;
            }

            switch (State)
            {
                case PlayerState.Unknown:
                case PlayerState.Loading:
                case PlayerState.Playing:
                {
                    return;
                }
                case PlayerState.Paused:
                case PlayerState.Ready:
                {
                    _positionReportTimer.Start();

                    // Apply playback-paramters if they did change in the meantime
                    if (_playbackParams != null && _player.PlaybackParams != _playbackParams)
                    {
                        _player.PlaybackParams = _playbackParams;
                    }
                    _player.Start();
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
            await Play();

            CreateAndShowServiceNotification();
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
            if (State == PlayerState.Stopped)
            {
                State = PlayerState.Paused;
            }

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