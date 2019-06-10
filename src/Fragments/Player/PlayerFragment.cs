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
using Idunas.DanceMusicPlayer.Services.AudioService;
using Idunas.DanceMusicPlayer.Services.Player;
using Idunas.DanceMusicPlayer.Services.Settings;
using Idunas.DanceMusicPlayer.Util;
using System;

namespace Idunas.DanceMusicPlayer.Fragments.Player
{
    public class PlayerFragment : NavFragment
    {
        private SettingsService _settingsService;

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

        public override string Title => MusicPlayer?.CurrentSong?.Name ?? Context.GetString(Resource.String.no_song_selected);

        protected int PlaybackSpeed
        {
            get { return _seekBarSpeed.Progress + _speedMin; }
            set { _seekBarSpeed.SetProgress(value - _speedMin, true); }
        }

        protected IMusicPlayer MusicPlayer
        {
            get { return MainActivity.MusicPlayer; }
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

            _rvAdapter = new BookmarksRvAdapter(MusicPlayer.CurrentSong);
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

            // Get the current state of the player once
            HandlePlayerServiceStateChanged(null, MusicPlayer.State);
            HandlePlayerServiceSongChanged(null, MusicPlayer.CurrentSong);
            HandlePlayerServiceDurationChanged(null, MusicPlayer.Duration);
            HandlePlayerServicePositionChanged(null, MusicPlayer.Position);

            // Subscribe for future updates
            MusicPlayer.PositionChanged += HandlePlayerServicePositionChanged;
            MusicPlayer.DurationChanged += HandlePlayerServiceDurationChanged;
            MusicPlayer.StateChanged += HandlePlayerServiceStateChanged;
            MusicPlayer.SongChanged += HandlePlayerServiceSongChanged;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_settingsService != null)
            {
                _settingsService.SettingsChanged -= HandleSettingsChanged;
                _settingsService.Dispose();
            }

            MusicPlayer.PositionChanged -= HandlePlayerServicePositionChanged;
            MusicPlayer.DurationChanged -= HandlePlayerServiceDurationChanged;
            MusicPlayer.StateChanged -= HandlePlayerServiceStateChanged;
            MusicPlayer.SongChanged -= HandlePlayerServiceSongChanged;
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

        private void HandlePlayerServicePositionChanged(object sender, long position)
        {
            Activity.RunOnUiThread(() => SetPosition(position));
        }

        private void HandlePlayerServiceDurationChanged(object sender, long duration)
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
            if (_rvAdapter != null)
            {
                _rvAdapter.Song = song;
                Activity.RunOnUiThread(() => _rvAdapter.NotifyDataSetChanged());
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
                MusicPlayer.CurrentPlaylist.Speed = PlaybackSpeed;
                PlaylistsService.Instance.Save();
            }
            else if (MusicPlayer.CurrentPlaylist != null)
            {
                PlaybackSpeed = MusicPlayer.CurrentPlaylist.Speed;
            }

            _lblSpeed.Text = $"{PlaybackSpeed}%";
            MusicPlayer?.ChangeSpeed(PlaybackSpeed / 100f);
        }

        private void HandleLabelSpeedLongClick(object sender, View.LongClickEventArgs e)
        {
            if (MusicPlayer.CurrentSong == null)
            {
                return;
            }

            SetSpeed(100);
        }

        private void HandleLabelSpeedClick(object sender, EventArgs e)
        {
            if (MusicPlayer.CurrentSong == null)
            {
                return;
            }

            // Prepare editor
            var layout = new LinearLayout(Context);
            var txtInput = new EditText(Context);
            txtInput.FocusChange += (_, __) => KeyboardUtils.ShowKeyboard(Context, txtInput);
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
                    KeyboardUtils.HideKeyboard(Context, txtInput);
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
            if (MusicPlayer.CurrentSong == null)
            {
                return;
            }

            MusicPlayer.CurrentSong.IsLooping = !MusicPlayer.CurrentSong.IsLooping;
            PlaylistsService.Instance.Save();
            EnsureState();
        }

        private void HandleSetStartLoopMarkerClick(object sender, EventArgs e)
        {
            if (MusicPlayer.CurrentSong == null)
            {
                return;
            }

            MusicPlayer.CurrentSong.LoopMarkerStart = _seekBarPosition.Progress;
            MusicPlayer.CurrentSong.IsLooping = true;

            if (MusicPlayer.CurrentSong.LoopMarkerEnd <= MusicPlayer.CurrentSong.LoopMarkerStart)
            {
                MusicPlayer.CurrentSong.LoopMarkerEnd = null;
            }

            MessageService.ShowHelpText(Resource.String.help_loop_marker);

            PlaylistsService.Instance.Save();
            EnsureState();
        }

        private void HandleSetStartLoopMarkerLongClick(object sender, View.LongClickEventArgs e)
        {
            if (MusicPlayer.CurrentSong == null)
            {
                return;
            }

            MusicPlayer.CurrentSong.LoopMarkerStart = null;
            PlaylistsService.Instance.Save();
            EnsureState();
        }

        private void HandleSetEndLoopMarkerClick(object sender, EventArgs e)
        {
            if (MusicPlayer.CurrentSong == null)
            {
                return;
            }

            MusicPlayer.CurrentSong.LoopMarkerEnd = _seekBarPosition.Progress;
            MusicPlayer.CurrentSong.IsLooping = true;

            if (MusicPlayer.CurrentSong.LoopMarkerStart >= MusicPlayer.CurrentSong.LoopMarkerEnd)
            {
                MusicPlayer.CurrentSong.LoopMarkerStart = null;
            }

            MessageService.ShowHelpText(Resource.String.help_loop_marker);

            PlaylistsService.Instance.Save();
            EnsureState();
        }

        private void HandleSetEndLoopMarkerLongClick(object sender, View.LongClickEventArgs e)
        {
            if (MusicPlayer.CurrentSong == null)
            {
                return;
            }

            MusicPlayer.CurrentSong.LoopMarkerEnd = null;
            PlaylistsService.Instance.Save();
            EnsureState();
        }

        private async void HandleAddBookmarkClick(object sender, EventArgs e)
        {
            if (MusicPlayer.CurrentSong == null)
            {
                return;
            }

            var position = _seekBarPosition.Progress;
            var dialogResult = await MessageBox
                .Build(Activity)
                .SetTitle(Resource.String.add_bookmark)
                .ShowWithEditText(
                    Resource.String.name,
                    Resource.String.ok,
                    Resource.String.cancel);

            if (dialogResult.DialogResult == MessageBox.MessageBoxResult.Positive)
            {
                MusicPlayer.CurrentSong.Bookmarks.Add(new Bookmark
                {
                    Name = dialogResult.Text,
                    Position = position
                });

                PlaylistsService.Instance.Save();
                _rvAdapter.NotifyDataSetChanged();
            }
        }

        private async void HandlePlayPauseClick(object sender, EventArgs e)
        {
            if (MusicPlayer.State == PlayerState.Playing)
            {
                MusicPlayer.Pause();
            }
            else
            {
                await MusicPlayer.Play(true);
            }
        }

        private async void HandlePreviousClick(object sender, EventArgs e)
        {
            await MusicPlayer.PlayPreviousSong();
        }

        private async void HandleNextClick(object sender, EventArgs e)
        {
            await MusicPlayer.PlayNextSong();
        }

        #endregion

        public void PlaySong(Song song, Playlist playlist)
        {
            PlaybackSpeed = playlist.Speed;
            EnsureState();

            MusicPlayer.Load(song, playlist);
        }

        private void SeekTo(long position)
        {
            _seekBarPosition.Progress = (int)position;
            _lblCurrentPosition.Text = TimeSpan.FromMilliseconds(position).ToString(@"mm\:ss");
            MusicPlayer?.SeekTo(position);
        }

        private void SetDuration(long duration)
        {
            _seekBarPosition.Max = (int)duration;
            _lblDuration.Text = TimeSpan.FromMilliseconds(duration).ToString(@"mm\:ss");
        }

        private void SetPosition(long position)
        {
            _seekBarPosition.SetProgress((int)position, true);
            _lblCurrentPosition.Text = TimeSpan.FromMilliseconds(position).ToString(@"mm\:ss");
        }

        private void SetSpeed(int speed)
        {
            PlaybackSpeed = speed;
            MusicPlayer.CurrentPlaylist.Speed = speed;
            PlaylistsService.Instance.Save();
        }

        private void EnsureState()
        {
            var isPlayerReady = MusicPlayer?.CurrentPlaylist != null;

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

            _btnToggleLooping.SetColorFilter(MusicPlayer.CurrentSong != null && MusicPlayer.CurrentSong.IsLooping ? _accentColor : Color.LightGray);
            _btnSetLoopStartMarker.SetColorFilter(MusicPlayer.CurrentSong?.LoopMarkerStart != null ? _accentColor : Color.LightGray);
            _btnSetLoopEndMarker.SetColorFilter(MusicPlayer.CurrentSong?.LoopMarkerEnd != null ? _accentColor : Color.LightGray);

            _loopPositionIndicator.LoopMarkerStart = MusicPlayer.CurrentSong?.LoopMarkerStart;
            _loopPositionIndicator.LoopMarkerEnd = MusicPlayer.CurrentSong?.LoopMarkerEnd;

            _btnPlayPause.SetImageResource(
                MusicPlayer.State == PlayerState.Playing
                    ? Resource.Drawable.ic_pause
                    : Resource.Drawable.ic_play);
            _btnPrevious.Enabled = MusicPlayer.HasPreviousSong;
            _btnNext.Enabled = MusicPlayer.HasNextSong;
        }
    }
}