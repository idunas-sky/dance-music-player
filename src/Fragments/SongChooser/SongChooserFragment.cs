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

            if (ContextCompat.CheckSelfPermission(Context, Android.Manifest.Permission.ReadExternalStorage) != Permission.Granted)
            {
                RequestPermissions();
            }
            else
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

        private void RequestPermissions()
        {
            // Request file system permissions
            if (ActivityCompat.ShouldShowRequestPermissionRationale(Activity, Android.Manifest.Permission.ReadExternalStorage))
            {
                // Show rationale
                Snackbar
                    .Make(MainActivity.MainLayout, Resource.String.rationale_read_external_storage, Snackbar.LengthIndefinite)
                    .SetAction(Resource.String.ok, new Action<View>(view =>
                    {
                        // Request permissions
                        ActivityCompat.RequestPermissions(
                           Activity,
                           new[] { Android.Manifest.Permission.ReadExternalStorage },
                           Constants.PermissionRequests.ReadExternalStorage);
                    }))
                    .Show();
            }
            else
            {
                // Request permissions
                ActivityCompat.RequestPermissions(
                    Activity,
                    new[] { Android.Manifest.Permission.ReadExternalStorage },
                    Constants.PermissionRequests.ReadExternalStorage);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == Constants.PermissionRequests.ReadExternalStorage)
            {
                if (grantResults.Length == 1 && grantResults[0] == Permission.Granted)
                {
                    ActivateListAdapter();
                    return;
                }

                // Permission has not been granted, go back to details
                NavManager.Instance.NavigateTo<PlaylistDetailsFragment>(NavDirection.Backward, f => f.Playlist = Playlist);
                return;
            }

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void ActivateListAdapter()
        {
            _rvAdapter = new SongChooserRvAdapter();
            _rvItems.SetAdapter(_rvAdapter);
        }
    }
}