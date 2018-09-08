using Android.Support.V7.Widget;
using Android.Views;
using Idunas.DanceMusicPlayer.Models;
using Idunas.DanceMusicPlayer.Services;
using System;

namespace Idunas.DanceMusicPlayer.Fragments.Player
{
    public class BookmarksRvAdapter : RecyclerView.Adapter
    {
        public event EventHandler<Bookmark> BookmarkClick;

        public Song Song
        {
            get;
            set;
        }

        public BookmarksRvAdapter(Song song)
        {
            Song = song;
        }

        public override int ItemCount => Song?.Bookmarks.Count ?? 0;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ((BookmarkItemViewHolder)holder).BindData(GetItem(position));
        }

        private Bookmark GetItem(int position)
        {
            if (Song.Bookmarks.Count > position)
            {
                return Song.Bookmarks[position];
            }

            return null;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.BookmarkDetailItem, parent, false);
            var viewHolder = new BookmarkItemViewHolder(view)
            {
                MenuItemClick = HandleMenuItemClick,
                BookmarkClick = HandleBookmarkClick
            };

            return viewHolder;
        }

        private void HandleMenuItemClick(Bookmark bookmark, IMenuItem menuItem)
        {
            switch (menuItem.ItemId)
            {
                case Resource.Id.action_delete:
                {
                    Song.Bookmarks.Remove(bookmark);
                    PlaylistsService.Instance.Save();
                    NotifyDataSetChanged();
                    break;
                }
            }
        }

        private void HandleBookmarkClick(Bookmark bookmark)
        {
            BookmarkClick?.Invoke(this, bookmark);
        }
    }
}