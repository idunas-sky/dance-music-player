using Android.Support.V7.Widget;
using Android.Views;
using Idunas.DanceMusicPlayer.Models;
using Idunas.DanceMusicPlayer.Services;
using System;

namespace Idunas.DanceMusicPlayer.Fragments.Playlists
{
    public class PlaylistsRvAdapter : RecyclerView.Adapter
    {
        public event EventHandler<Playlist> ItemClick;

        public override int ItemCount => PlaylistsService.Instance.Playlists.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var playlist = PlaylistsService.Instance.Playlists[position];
            ((PlaylistItemViewHolder)holder).BindData(playlist);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.PlaylistItem, parent, false);
            return new PlaylistItemViewHolder(view, OnItemClick);
        }

        private void OnItemClick(int position)
        {
            ItemClick?.Invoke(this, PlaylistsService.Instance.Playlists[position]);
        }
    }
}