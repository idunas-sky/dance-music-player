using Android.Support.V7.Widget;
using Android.Views;
using Idunas.DanceMusicPlayer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Idunas.DanceMusicPlayer.Fragments.SongChooser
{
    public class SongChooserRvAdapter : RecyclerView.Adapter
    {
        public IList<FileSystemItem> Items
        {
            get;
            private set;
        }

        public SongChooserRvAdapter()
        {
            SetVisibleDirectory(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath);
        }

        public override int ItemCount => Items.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ((SongItemViewHolder)holder).BindData(GetItem(position));
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.TwoLineMultipleChoiceItem, parent, false);
            var viewHolder = new SongItemViewHolder(view);
            viewHolder.OnItemClick = pos => OnItemClick(viewHolder, pos);

            return viewHolder;
        }

        private void OnItemClick(SongItemViewHolder viewHolder, int position)
        {
            var item = GetItem(position);
            if (item.IsDirectory)
            {
                SetVisibleDirectory(item.Path);
                return;
            }

            item.IsSelected = !item.IsSelected;
            viewHolder.BindData(item);
        }

        private IList<FileSystemItem> GetFileSystemItems(string path)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(path);
                if (!directoryInfo.Exists)
                {
                    // Oh, now have a problem because the user alredy clicked an item
                    // that does no longer exist. Show the parent directory instead.
                    return GetFileSystemItems(directoryInfo.Parent.FullName);
                }

                var directories = directoryInfo.GetDirectories().Select(x => new FileSystemItem
                {
                    IsDirectory = true,
                    IsSelectable = false,
                    Name = x.Name,
                    Path = x.FullName,
                    Icon = Resource.Drawable.ic_folder
                });

                var files = directoryInfo.GetFiles().Select(x => new FileSystemItem
                {
                    IsDirectory = false,
                    IsSelectable = true,
                    Name = x.Name,
                    Path = x.FullName,
                    Icon = Resource.Drawable.ic_file
                });

                var result = directories.Concat(files).OrderBy(x => x.IsDirectory).ThenBy(x => x.Name).ToList();

                // Insert a directory the user can tap to go back to the parent directory
                if (directoryInfo.FullName != Android.OS.Environment.ExternalStorageDirectory.AbsolutePath)
                {
                    result.Insert(0, new FileSystemItem
                    {
                        IsDirectory = true,
                        Name = "..",
                        IsSelectable = false,
                        IsSelected = false,
                        Path = path.Substring(0, path.LastIndexOf('/')),
                        Icon = Resource.Drawable.ic_folder
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                ErrorService.Instance.ShowError(ex, "Failed to retrieve file system items");
                return GetFileSystemItems(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath);
            }
        }

        private void SetVisibleDirectory(string path)
        {
            Items = GetFileSystemItems(path);
            NotifyDataSetChanged();
        }

        private FileSystemItem GetItem(int position)
        {
            if (Items.Count > position)
            {
                return Items[position];
            }

            return null;
        }
    }
}