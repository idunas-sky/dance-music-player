using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Idunas.DanceMusicPlayer.Models;
using System;

namespace Idunas.DanceMusicPlayer.Fragments.Playlists
{
    public class PlaylistItemViewHolder : RecyclerView.ViewHolder
    {
        private TextView _lblName;
        private TextView _lblSongCount;

        public PlaylistItemViewHolder(View view, Action<int> clickListener) : base(view)
        {
            _lblName = view.FindViewById<TextView>(Resource.Id.lbl_name);
            _lblSongCount = view.FindViewById<TextView>(Resource.Id.lbl_song_count);

            view.Click += (sender, e) => clickListener(LayoutPosition);
        }

        public void BindData(Playlist playlist)
        {
            _lblName.Text = playlist.Name;
            _lblSongCount.Text = $"{playlist.Songs.Count} Lieder";
        }
    }
}