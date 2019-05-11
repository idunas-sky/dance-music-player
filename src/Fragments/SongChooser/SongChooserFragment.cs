using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Idunas.DanceMusicPlayer.Activities;
using Idunas.DanceMusicPlayer.Fragments.PlaylistDetails;
using Idunas.DanceMusicPlayer.Models;
using Idunas.DanceMusicPlayer.Services;
using Idunas.DanceMusicPlayer.Util;
using System;
using System.Linq;

namespace Idunas.DanceMusicPlayer.Fragments.SongChooser
{
    public class SongChooserFragment : NavFragment
    {
        private SongChooserRvAdapter _rvAdapter;
        private RecyclerView _rvItems;

        public Playlist Playlist { get; set; }

        public override string Title => Context.GetString(Resource.String.add_songs);

        public override bool ShowBackNavigation => true;

        public override void OnBackNavigationPressed()
        {
            NavManager.Instance.NavigateTo<PlaylistDetailsFragment>(NavDirection.Backward, f => f.Playlist = Playlist);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.SongChooser, container, false);

            _rvItems = view.FindViewById<RecyclerView>(Resource.Id.rvItems);
            _rvItems.HasFixedSize = true;
            _rvItems.SetLayoutManager(new LinearLayoutManager(Context));

            if (PermissionRequest.Storage.Request(Activity, Resource.String.rationale_add_songs, Constants.PermissionRequests.AddSongs))
            {
                ActivateListAdapter();
            }

            return view;
        }


        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.song_chooser, menu);
            base.OnCreateOptionsMenu(menu, inflater);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_save)
            {
                SaveSelectedSongs();
            }

            return base.OnOptionsItemSelected(item);
        }

        private void SaveSelectedSongs()
        {
            // TODO: Persist selection when changing folders
            if (_rvAdapter == null || _rvAdapter.Items == null)
            {
                MessageService.ShowLongMessage("Error: Adapter does not have items.");
                return;
            }

            var itemsToAdd = _rvAdapter.Items
                .Where(i => i.IsSelected && !Playlist.Songs.Any(song => song.FilePath == i.Path));

            foreach (var item in itemsToAdd)
            {
                Playlist.Songs.Add(new Song
                {
                    AddedOn = DateTime.Now,
                    Name = item.Name,
                    FilePath = item.Path
                });
            }

            PlaylistsService.Instance.Save();
            NavManager.Instance.NavigateTo<PlaylistDetailsFragment>(NavDirection.Backward, f => f.Playlist = Playlist);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (PermissionRequest.WasGranted(grantResults))
            {
                ActivateListAdapter();
                return;
            }

            // Permission has not been granted, go back to details
            NavManager.Instance.NavigateTo<PlaylistDetailsFragment>(NavDirection.Backward, f => f.Playlist = Playlist);
            return;
        }

        private void ActivateListAdapter()
        {
            _rvAdapter = new SongChooserRvAdapter();
            _rvItems.SetAdapter(_rvAdapter);
        }
    }
}