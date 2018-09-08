using Android.Support.V7.Widget;
using Android.Views;
using Idunas.DanceMusicPlayer.Models;
using Idunas.DanceMusicPlayer.Services;
using System;

namespace Idunas.DanceMusicPlayer.Fragments.PlaylistDetails
{
    public class PlaylistDetailsRvAdapter : RecyclerView.Adapter
    {
        private Playlist _playlist;

        public event EventHandler<Song> SongClick;

        public Playlist Playlist
        {
            get { return _playlist; }
            set
            {
                _playlist = value;
                NotifyDataSetChanged();
            }
        }

        public PlaylistDetailsRvAdapter(Playlist playlist)
        {
            Playlist = playlist;
        }

        public override int ItemCount => Playlist.Songs.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ((PlaylistDetailItemViewHolder)holder).BindData(GetItem(position));
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.PlaylistDetailItem, parent, false);
            var viewHolder = new PlaylistDetailItemViewHolder(view)
            {
                MenuItemClick = HandleMenuItemClick,
                SongClick = HandleSongClick
            };

            return viewHolder;
        }

        private void HandleMenuItemClick(Song song, IMenuItem menuItem)
        {
            switch (menuItem.ItemId)
            {
                case Resource.Id.action_delete:
                {
                    Playlist.Songs.Remove(song);
                    PlaylistsService.Instance.Save();
                    NotifyDataSetChanged();
                    break;
                }
            }
        }

        private void HandleSongClick(Song song)
        {
            SongClick?.Invoke(this, song);
        }

        private Song GetItem(int position)
        {
            if (Playlist.Songs.Count > position)
            {
                return Playlist.Songs[position];
            }

            return null;
        }
    }
}