using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Idunas.DanceMusicPlayer.Activities;
using Idunas.DanceMusicPlayer.Models;
using Idunas.DanceMusicPlayer.Services.Player;
using System;

namespace Idunas.DanceMusicPlayer.Fragments.SongBar
{
    public class SongBarFragment : Fragment
    {
        private PlayerServiceController _controller;
        private ProgressBar _progressBar;
        private TextView _lblSong;
        private ImageButton _btnPlayPause;
        private ImageButton _btnNext;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            var view = inflater.Inflate(Resource.Layout.SongBar, container, false);
            _progressBar = view.FindViewById<ProgressBar>(Resource.Id.progress_song);
            _lblSong = view.FindViewById<TextView>(Resource.Id.lbl_song);
            _lblSong.Click += HandleSongLabelClick;
            _btnPlayPause = view.FindViewById<ImageButton>(Resource.Id.btn_play_pause);
            _btnPlayPause.Click += HandlePlayPauseClick;
            _btnNext = view.FindViewById<ImageButton>(Resource.Id.btn_next);
            _btnNext.Click += HandleNextClick;

            return view;
        }

        public override void OnStart()
        {
            base.OnStart();

            if (_controller == null)
            {
                // Player-Service is not connected yet, so we will connect now
                // and handle everything else in the Connected-event.
                _controller = new PlayerServiceController(Context);
                _controller.Connected += HandleControllerConnected;
                _controller.Start();
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

        #region --- Event handlers

        private void HandlePlayerServicePositionChanged(object sender, int position)
        {
            Activity.RunOnUiThread(() => _progressBar.Progress = position);
        }

        private void HandlePlayerServiceDurationChanged(object sender, int duration)
        {
            Activity.RunOnUiThread(() =>
            {
                _progressBar.Max = duration;
                _progressBar.Progress = 0;
            });
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
            Activity.RunOnUiThread(() =>
            {
                _lblSong.Text = song?.Name ?? Context.GetString(Resource.String.no_song_selected);

                EnsureState();
            });
        }

        private void HandleSongLabelClick(object sender, EventArgs e)
        {
            ((MainActivity)Activity).ShowPlayer();
        }

        private async void HandlePlayPauseClick(object sender, EventArgs e)
        {
            if (_controller.Service.State == PlayerState.Playing)
            {
                _controller.Service.Pause();
            }
            else
            {
                await _controller.Service.Play();
            }
        }

        private async void HandleNextClick(object sender, EventArgs e)
        {
            await _controller.Service.PlayNextSong();
        }

        #endregion

        private void EnsureState()
        {
            var isConnected = _controller?.Service != null;

            // Disable / enable all buttons depending on connection
            _btnPlayPause.Enabled = isConnected;
            _btnNext.Enabled = isConnected;

            if (!isConnected)
            {
                _btnPlayPause.SetImageResource(Resource.Drawable.ic_play);
                return;
            }

            _btnPlayPause.SetImageResource(
                _controller.Service.State == PlayerState.Playing
                    ? Resource.Drawable.ic_pause
                    : Resource.Drawable.ic_play);
            _btnNext.Enabled = _controller.Service.HasNextSong;
        }
    }
}