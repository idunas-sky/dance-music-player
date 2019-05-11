using Android.Support.V7.Widget;
using Android.Views;
using Idunas.DanceMusicPlayer.Framework.ListView;
using Idunas.DanceMusicPlayer.Models;
using Idunas.DanceMusicPlayer.Services;
using System;
using System.Collections.Generic;

namespace Idunas.DanceMusicPlayer.Fragments.PlaylistDetails
{
    public class PlaylistDetailsRvAdapter : RecyclerViewAdapterBase<Song>, ITouchableListViewAdapter
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

        public Song SelectedSong { get; private set; }

        protected override IList<Song> Items
        {
            get
            {
                return Playlist.Songs;
            }
        }

        public PlaylistDetailsRvAdapter(Playlist playlist)
        {
            Playlist = playlist;
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
            SelectedSong = song;
            SongClick?.Invoke(this, song);
        }

        public void ItemMoved(int fromPosition, int toPosition)
        {
            var movedSong = GetItem(fromPosition);
            if (movedSong != null)
            {
                Items.Remove(movedSong);
                Items.Insert(toPosition, movedSong);

                PlaylistsService.Instance.Save();
                NotifyItemMoved(fromPosition, toPosition);
            }
        }
    }
}