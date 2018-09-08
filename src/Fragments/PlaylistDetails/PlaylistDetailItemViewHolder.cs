using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Idunas.DanceMusicPlayer.Models;
using Idunas.DanceMusicPlayer.Services;

namespace Idunas.DanceMusicPlayer.Fragments.PlaylistDetails
{
    public class PlaylistDetailItemViewHolder : RecyclerView.ViewHolder
    {
        public Action<Song, IMenuItem> MenuItemClick
        {
            get;
            set;
        }

        public Action<Song> SongClick
        {
            get;
            set;
        }

        private Song _song;

        private ImageView _icon;
        private TextView _lblTitle;
        private TextView _lblSubTitle;
        private ImageButton _btnMenu;

        public PlaylistDetailItemViewHolder(View itemView) : base(itemView)
        {
            _icon = itemView.FindViewById<ImageView>(Resource.Id.img_icon);
            _lblTitle = itemView.FindViewById<TextView>(Resource.Id.lbl_title);
            _lblSubTitle = itemView.FindViewById<TextView>(Resource.Id.lbl_sub_title);
            _btnMenu = itemView.FindViewById<ImageButton>(Resource.Id.action_menu);
            _btnMenu.Click += (_, __) =>
            {
                var menu = new Android.Support.V7.Widget.PopupMenu(itemView.Context, _btnMenu);
                menu.Inflate(Resource.Menu.playlist_detail_item);
                menu.MenuItemClick += HandleMenuItemClick;
                menu.Show();
            };

            itemView.Click += (sender, e) => SongClick?.Invoke(_song);
        }

        public void BindData(Song song)
        {
            _song = song;

            if (song == null)
            {
                return;
            }

            // Set values
            _lblTitle.Text = song.Name;
            _lblSubTitle.Text = ""; // TODO: MP3 Info?

            // Configure layout
            if (string.IsNullOrEmpty(_lblSubTitle.Text))
            {
                // Move our title to the center if there is no subtitle
                ((RelativeLayout.LayoutParams)_lblTitle.LayoutParameters).AddRule(LayoutRules.CenterInParent);
            }
            else
            {
                ((RelativeLayout.LayoutParams)_lblTitle.LayoutParameters).RemoveRule(LayoutRules.CenterInParent);
            }

        }

        private void HandleMenuItemClick(object sender, Android.Support.V7.Widget.PopupMenu.MenuItemClickEventArgs e)
        {
            if (_song == null)
            {
                return;
            }

            MenuItemClick?.Invoke(_song, e.Item);
        }
    }
}