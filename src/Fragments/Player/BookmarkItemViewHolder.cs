using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Idunas.DanceMusicPlayer.Models;
using System;

namespace Idunas.DanceMusicPlayer.Fragments.Player
{
    public class BookmarkItemViewHolder : RecyclerView.ViewHolder
    {
        public Action<Bookmark, IMenuItem> MenuItemClick
        {
            get;
            set;
        }

        public Action<Bookmark> BookmarkClick
        {
            get;
            set;
        }

        private Bookmark _bookmark;

        private TextView _lblTitle;
        private ImageButton _btnMenu;

        public BookmarkItemViewHolder(View itemView) : base(itemView)
        {
            _lblTitle = itemView.FindViewById<TextView>(Resource.Id.lbl_title);
            _btnMenu = itemView.FindViewById<ImageButton>(Resource.Id.action_menu);
            _btnMenu.Click += (_, __) =>
            {
                var menu = new Android.Support.V7.Widget.PopupMenu(itemView.Context, _btnMenu);
                menu.Inflate(Resource.Menu.bookmark_detail_item);
                menu.MenuItemClick += HandleMenuItemClick;
                menu.Show();
            };

            itemView.Click += (sender, e) => BookmarkClick?.Invoke(_bookmark);
        }

        public void BindData(Bookmark bookmark)
        {
            _bookmark = bookmark;
            if (bookmark == null)
            {
                return;
            }

            _lblTitle.Text = string.Format(
                @"{0:mm\:ss} {1}",
                TimeSpan.FromMilliseconds(_bookmark.Position),
                _bookmark.Name);
        }

        private void HandleMenuItemClick(object sender, Android.Support.V7.Widget.PopupMenu.MenuItemClickEventArgs e)
        {
            if (_bookmark == null)
            {
                return;
            }

            MenuItemClick?.Invoke(_bookmark, e.Item);
        }
    }
}