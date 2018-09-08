using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using System;

namespace Idunas.DanceMusicPlayer.Fragments.SongChooser
{
    public class SongItemViewHolder : RecyclerView.ViewHolder
    {
        private FileSystemItem _item;
        private TextView _lblTitle;
        private TextView _lblSubTitle;
        private CheckBox _chxIsSelected;
        private ImageView _icon;

        public Action<int> OnItemClick
        {
            get;
            set;
        }

        public SongItemViewHolder(View itemView) : base(itemView)
        {
            _icon = itemView.FindViewById<ImageView>(Resource.Id.img_icon);
            _lblTitle = itemView.FindViewById<TextView>(Resource.Id.lbl_title);
            _lblSubTitle = itemView.FindViewById<TextView>(Resource.Id.lbl_sub_title);
            _chxIsSelected = itemView.FindViewById<CheckBox>(Resource.Id.chx_selected);
            _chxIsSelected.CheckedChange += (sender, e) =>
            {
                if (_item == null)
                {
                    return;
                }

                _item.IsSelected = _chxIsSelected.Checked;
            };

            itemView.Click += (sender, e) => OnItemClick?.Invoke(LayoutPosition);
        }

        public void BindData(FileSystemItem item)
        {
            _item = item;

            if (item == null)
            {
                return;
            }

            // Set values
            _lblTitle.Text = item.Name;
            _lblSubTitle.Text = ""; // TODO: MP3 Info?
            _chxIsSelected.Checked = item.IsSelected;
            _icon.SetImageResource(item.Icon);


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

            _chxIsSelected.Visibility = item.IsSelectable ? ViewStates.Visible : ViewStates.Invisible;
        }
    }
}