using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using Idunas.DanceMusicPlayer.Activities;
using Idunas.DanceMusicPlayer.Models;
using Idunas.DanceMusicPlayer.Services;
using Idunas.DanceMusicPlayer.Services.Player;
using Idunas.DanceMusicPlayer.Services.Settings;
using Idunas.DanceMusicPlayer.Util;
using System;

namespace Idunas.DanceMusicPlayer.Fragments.Player
{
    public class PlayerFragment : NavFragment
    {
        private SettingsService _settingsService;
        private PlayerServiceController _controller;
        private Playlist _playlist;
        private Song _song;

        private int _speedMin = 0;
        private int _speedMax = 0;

        private TextView _lblSpeed;
        private SeekBar _seekBarSpeed;
        private TextView _lblCurrentPosition;
        private TextView _lblDuration;
        private SeekBar _seekBarPosition;
        private LoopPositionIndicatorView _loopPositionIndicator;

        private ImageButton _btnToggleLooping;
        private ImageButton _btnAddBookmark;
        private ImageButton _btnSetLoopStartMarker;
        private ImageButton _btnSetLoopEndMarker;
        private RecyclerView _rvBookmarks;
        private BookmarksRvAdapter _rvAdapter;
        private ImageButton _btnPlayPause;
        private ImageButton _btnPrevious;
        private ImageButton _btnNext;

        private Color _accentColor;

        public override string Title => _song?.Name ?? Context.GetString(Resource.String.no_song_selected);

        protected int PlaybackSpeed
        {
            get { return _seekBarSpeed.Progress + _speedMin; }
            set { _seekBarSpeed.SetProgress(value - _speedMin, true); }
        }

        #region --- Constructor / Initializing

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            _settingsService = new SettingsService(Context);
            _settingsService.SettingsChanged += HandleSettingsChanged;

            var view = inflater.Inflate(Resource.Layout.Player, container, false);

            using (var styleAttributes = Activity.Theme.ObtainStyledAttributes(new[] { Android.Resource.Attribute.ColorAccent }))
            {
                _accentColor = styleAttributes.GetColor(0, 0);
            }

            // Speed
            _seekBarSpeed = view.FindViewById<SeekBar>(Resource.Id.seekBar_speed);
            _seekBarSpeed.ProgressChanged += HandleSpeedChanged;
            _lblSpeed = view.FindViewById<TextView>(Resource.Id.lbl_speed);
            _lblSpeed.Click += HandleLabelSpeedClick;
            _lblSpeed.LongClick += HandleLabelSpeedLongClick;

            // Position
            _lblCurrentPosition = view.FindViewById<TextView>(Resource.Id.lbl_current_position);
            _lblDuration = view.FindViewById<TextView>(Resource.Id.lbl_duration);
            _seekBarPosition = view.FindViewById<SeekBar>(Resource.Id.seekBar_position);
            _seekBarPosition.ProgressChanged += HandlePositionChanged;

            // Markers
            _btnToggleLooping = view.FindViewById<ImageButton>(Resource.Id.btn_loop);
            _btnToggleLooping.Click += HandleToggleLoopingClick;
            _btnAddBookmark = view.FindViewById<ImageButton>(Resource.Id.btn_add_bookmark);
            _btnAddBookmark.Click += HandleAddBookmarkClick;
            _btnSetLoopStartMarker = view.FindViewById<ImageButton>(Resource.Id.btn_set_loop_start);
            _btnSetLoopStartMarker.Click += HandleSetStartLoopMarkerClick;
            _btnSetLoopStartMarker.LongClick += HandleSetStartLoopMarkerLongClick;
            _btnSetLoopEndMarker = view.FindViewById<ImageButton>(Resource.Id.btn_set_loop_end);
            _btnSetLoopEndMarker.Click += HandleSetEndLoopMarkerClick;
            _btnSetLoopEndMarker.LongClick += HandleSetEndLoopMarkerLongClick;
            _rvBookmarks = view.FindViewById<RecyclerView>(Resource.Id.rvBookmarks);
            _rvBookmarks.HasFixedSize = true;
            _rvBookmarks.SetLayoutManager(new LinearLayoutManager(Context));

            _rvAdapter = new BookmarksRvAdapter(_song);
            _rvAdapter.BookmarkClick += (sender, e) => SeekTo(e.Position);
            _rvBookmarks.SetAdapter(_rvAdapter);

            // Indicators
            _loopPositionIndicator = view.FindViewById<LoopPositionIndicatorView>(Resource.Id.loop_position_indicator);
            _loopPositionIndicator.SeekBarPosition = _seekBarPosition;
            _loopPositionIndicator.ButtonStartLoopMarker = _btnSetLoopStartMarker;
            _loopPositionIndicator.ButtonEndLoopMarker = _btnSetLoopEndMarker;
            _loopPositionIndicator.Color = _accentColor;

            // Player controls
            _btnPlayPause = view.FindViewById<ImageButton>(Resource.Id.btn_play_pause);
            _btnPlayPause.Click += HandlePlayPauseClick;
            _btnPrevious = view.FindViewById<ImageButton>(Resource.Id.btn_previous);
            _btnPrevious.Click += HandlePreviousClick;
            _btnNext = view.FindViewById<ImageButton>(Resource.Id.btn_next);
            _btnNext.Click += HandleNextClick;

            EnsureState();
            return view;
        }

        public override void OnStart()
        {
            base.OnStart();

            EnsureMinMaxSpeed(_settingsService.Settings);

            if (_controller == null)
            {
                // Player-Service is not connected yet, so we will connect now
                // and handle everything else in the Connected-event.
                _controller = new PlayerServiceController(Context);
                _controller.Connected += HandleControllerConnected;
                _controller.Start();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_settingsService != null)
            {
                _settingsService.SettingsChanged -= HandleSettingsChanged;
                _settingsService.Dispose();
            }
        }

        private void HandleControllerConnected(object sender, EventArgs e)
        {
            // Get the current state of the player once
            HandlePlayerServiceStateChanged(null, _controller.Service.State);
            HandlePlayerServiceSongChanged(null, _controller.Service.CurrentSong);
            HandlePlayerServiceDurationChanged(null, _controller.Service.Duration);
            HandlePlayerServicePositionChanged(null, _controller.Service.Position);

            // Subscribe for future updates
            _controller.Service.PositionChanged += HandlePlayerServicePositionChanged;
            _controller.Service.DurationChanged += HandlePlayerServiceDurationChanged;
            _controller.Service.StateChanged += HandlePlayerServiceStateChanged;
            _controller.Service.SongChanged += HandlePlayerServiceSongChanged;
        }

        private void HandleSettingsChanged(object sender, AppSettings e)
        {
            EnsureMinMaxSpeed(e);
        }

        private void EnsureMinMaxSpeed(AppSettings settings)
        {
            if (settings.SpeedMin != _speedMin || settings.SpeedMax != _speedMax)
            {
                _speedMin = settings.SpeedMin;
                _speedMax = settings.SpeedMax;
                _seekBarSpeed.Max = _speedMax - _speedMin;
            }
        }

        #endregion

        #region --- Back-navigation

        public override int BackNavigationIcon => Resource.Drawable.ic_chevron_down;

        public override bool ShowBackNavigation => true;

        public override void OnBackNavigationPressed()
        {
            MainActivity.HidePlayer();
        }

        #endregion

        #region --- Event handlers

        private void HandlePlayerServicePositionChanged(object sender, int position)
        {
            Activity.RunOnUiThread(() => SetPosition(position));
        }

        private void HandlePlayerServiceDurationChanged(object sender, int duration)
        {
            Activity.RunOnUiThread(() => SetDuration(duration));
        }

        private void HandlePlayerServiceStateChanged(object sender, PlayerState state)
        {
            if (!IsVisible)
            {
                return;
            }

            Activity.RunOnUiThread(EnsureState);
        }

        private void HandlePlayerServiceSongChanged(object sender, Song song)
        {
            _song = song;

            if (_rvAdapter != null)
            {
                _rvAdapter.Song = song;
                _rvAdapter.NotifyDataSetChanged();
            }

            if (MainActivity != null)
            {
                MainActivity.InvalidateActionBar();
            }
        }

        private void HandleSpeedChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (e.FromUser)
            {
                _playlist.Speed = PlaybackSpeed;
                PlaylistsService.Instance.Save();
            }

            _lblSpeed.Text = $"{PlaybackSpeed}%";
            _controller?.Service?.ChangeSpeed(PlaybackSpeed / 100f);
        }

        private void HandleLabelSpeedLongClick(object sender, View.LongClickEventArgs e)
        {
            if (_song == null)
            {
                return;
            }

            SetSpeed(100);
        }

        private void HandleLabelSpeedClick(object sender, EventArgs e)
        {
            if (_song == null)
            {
                return;
            }

            // Prepare editor
            var layout = new LinearLayout(Context);
            var txtInput = new EditText(Context);
            txtInput.FocusChange += (_, __) => AlertDialogUtils.ShowKeyboard(Context, txtInput);
            txtInput.Text = PlaybackSpeed.ToString();
            txtInput.InputType = InputTypes.ClassNumber | InputTypes.NumberFlagSigned;
            txtInput.SetSelectAllOnFocus(true);

            var layoutParams = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent);
            layoutParams.SetMargins(
                (int)Context.Resources.GetDimension(Resource.Dimension.spacing_large),
                0,
                (int)Context.Resources.GetDimension(Resource.Dimension.spacing_large),
                0);
            txtInput.LayoutParameters = layoutParams;
            layout.AddView(txtInput);

            var dialogBuilder = new AlertDialog.Builder(Context)
                .SetTitle(Resource.String.change_speed)
                .SetView(layout)
                .SetPositiveButton(Resource.String.ok, (_, __) =>
                {
                    AlertDialogUtils.HideKeyboard(Context, txtInput);
                    if (int.TryParse(txtInput.Text, out var result))
                    {
                        SetSpeed(result);
                    }
                });

            dialogBuilder.Show();
        }

        private void HandlePositionChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (!e.FromUser)
            {
                return;
            }

            SeekTo(e.Progress);
        }

        private void HandleToggleLoopingClick(object sender, EventArgs e)
        {
            if (_song == null)
            {
                return;
            }

            _song.IsLooping = !_song.IsLooping;
            PlaylistsService.Instance.Save();
            EnsureState();
        }

        private void HandleSetStartLoopMarkerClick(object sender, EventArgs e)
        {
            if (_song == null)
            {
                return;
            }

            _song.LoopMarkerStart = _seekBarPosition.Progress;
            _song.IsLooping = true;

            if (_song.LoopMarkerEnd <= _song.LoopMarkerStart)
            {
                _song.LoopMarkerEnd = null;
            }

            MessageService.ShowHelpText(Resource.String.help_loop_marker);

            PlaylistsService.Instance.Save();
            EnsureState();
        }

        private void HandleSetStartLoopMarkerLongClick(object sender, View.LongClickEventArgs e)
        {
            if (_song == null)
            {
                return;
            }

            _song.LoopMarkerStart = null;
            PlaylistsService.Instance.Save();
            EnsureState();
        }

        private void HandleSetEndLoopMarkerClick(object sender, EventArgs e)
        {
            if (_song == null)
            {
                return;
            }

            _song.LoopMarkerEnd = _seekBarPosition.Progress;
            _song.IsLooping = true;

            if (_song.LoopMarkerStart >= _song.LoopMarkerEnd)
            {
                _song.LoopMarkerStart = null;
            }

            MessageService.ShowHelpText(Resource.String.help_loop_marker);

            PlaylistsService.Instance.Save();
            EnsureState();
        }

        private void HandleSetEndLoopMarkerLongClick(object sender, View.LongClickEventArgs e)
        {
            if (_song == null)
            {
                return;
            }

            _song.LoopMarkerEnd = null;
            PlaylistsService.Instance.Save();
            EnsureState();
        }

        private async void HandleAddBookmarkClick(object sender, EventArgs e)
        {
            if (_song == null)
            {
                return;
            }

            var position = _seekBarPosition.Progress;
            var dialogResult = await AlertDialogUtils.ShowEditTextDialog(
                Context,
                Resource.String.add_bookmark,
                Resource.String.name,
                Resource.String.ok,
                Resource.String.cancel);

            if (dialogResult.DialogResult == AlertDialogUtils.AlertDialogResult.Positive)
            {
                _song.Bookmarks.Add(new Bookmark
                {
                    Name = dialogResult.Text,
                    Position = position
                });

                PlaylistsService.Instance.Save();
                _rvAdapter.NotifyDataSetChanged();
            }
        }

        private void HandlePlayPauseClick(object sender, EventArgs e)
        {
            if (_controller.Service.State == PlayerState.Playing)
            {
                _controller.Service.Pause();
            }
            else
            {
                _controller.Service.Play();
            }
        }

        private async void HandlePreviousClick(object sender, EventArgs e)
        {
            await _controller.Service.PlayPreviousSong();
        }

        private async void HandleNextClick(object sender, EventArgs e)
        {
            await _controller.Service.PlayNextSong();
        }

        #endregion

        public void PlaySong(Song song, Playlist playlist)
        {
            _playlist = playlist;
            _song = song;

            PlaybackSpeed = _playlist.Speed;
            EnsureState();

            _controller.Service.Load(_song, _playlist);
        }

        private void SeekTo(int position)
        {
            _seekBarPosition.Progress = position;
            _lblCurrentPosition.Text = TimeSpan.FromMilliseconds(position).ToString(@"mm\:ss");
            _controller?.Service?.SeekTo(position);
        }

        private void SetDuration(int duration)
        {
            _seekBarPosition.Max = duration;
            _lblDuration.Text = TimeSpan.FromMilliseconds(duration).ToString(@"mm\:ss");
        }

        private void SetPosition(int position)
        {
            _seekBarPosition.SetProgress(position, true);
            _lblCurrentPosition.Text = TimeSpan.FromMilliseconds(position).ToString(@"mm\:ss");
        }

        private void SetSpeed(int speed)
        {
            PlaybackSpeed = speed;
            _playlist.Speed = speed;
            PlaylistsService.Instance.Save();
        }

        private void EnsureState()
        {
            var isPlayerReady = _controller?.Service != null && _playlist != null;

            // Disable / enable all buttons depending on connection
            _seekBarSpeed.Enabled = isPlayerReady;
            _seekBarPosition.Enabled = isPlayerReady;

            _btnToggleLooping.Enabled = isPlayerReady;
            _btnSetLoopStartMarker.Enabled = isPlayerReady;
            _btnSetLoopEndMarker.Enabled = isPlayerReady;
            _btnAddBookmark.Enabled = isPlayerReady;

            _btnPlayPause.Enabled = isPlayerReady;
            _btnPrevious.Enabled = isPlayerReady;
            _btnNext.Enabled = isPlayerReady;

            if (!isPlayerReady)
            {
                _btnPlayPause.SetImageResource(Resource.Drawable.ic_play);
                return;
            }

            _btnToggleLooping.SetColorFilter(_song != null && _song.IsLooping ? _accentColor : Color.LightGray);
            _btnSetLoopStartMarker.SetColorFilter(_song?.LoopMarkerStart != null ? _accentColor : Color.LightGray);
            _btnSetLoopEndMarker.SetColorFilter(_song?.LoopMarkerEnd != null ? _accentColor : Color.LightGray);

            _loopPositionIndicator.LoopMarkerStart = _song?.LoopMarkerStart;
            _loopPositionIndicator.LoopMarkerEnd = _song?.LoopMarkerEnd;

            _btnPlayPause.SetImageResource(
                _controller.Service.State == PlayerState.Playing
                    ? Resource.Drawable.ic_pause
                    : Resource.Drawable.ic_play);
            _btnPrevious.Enabled = _controller.Service.HasPreviousSong;
            _btnNext.Enabled = _controller.Service.HasNextSong;
        }
    }
}