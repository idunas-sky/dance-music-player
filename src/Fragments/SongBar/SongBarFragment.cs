using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Idunas.DanceMusicPlayer.Activities;
using System;

namespace Idunas.DanceMusicPlayer.Fragments.SongBar
{
    public class SongBarFragment : Fragment
    {
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

        private void HandleSongLabelClick(object sender, EventArgs e)
        {
            ((MainActivity)Activity).ShowPlayer();
        }

        private void HandlePlayPauseClick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void HandleNextClick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}