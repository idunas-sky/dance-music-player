using Android.Support.V7.Widget;
using Android.Views;
using Idunas.DanceMusicPlayer.Framework.ListView;
using Idunas.DanceMusicPlayer.Models;
using Idunas.DanceMusicPlayer.Services;
using System;
using System.Collections.Generic;

namespace Idunas.DanceMusicPlayer.Fragments.Playlists
{
    public class PlaylistsRvAdapter : RecyclerViewAdapterBase<Playlist>, ITouchableListViewAdapter
    {
        public event EventHandler<Playlist> ItemClick;

        protected override IList<Playlist> Items
        {
            get
            {
                return PlaylistsService.Instance.Playlists;
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.PlaylistItem, parent, false);
            return new PlaylistItemViewHolder(view, HandleItemClick);
        }

        private void HandleItemClick(int position)
        {
            ItemClick?.Invoke(this, PlaylistsService.Instance.Playlists[position]);
        }

        public void ItemMoved(int fromPosition, int toPosition)
        {
            var movedItem = GetItem(fromPosition);
            if (movedItem != null)
            {
                Items.Remove(movedItem);
                Items.Insert(toPosition, movedItem);

                PlaylistsService.Instance.Save();
                NotifyItemMoved(fromPosition, toPosition);
            }
        }
    }
}