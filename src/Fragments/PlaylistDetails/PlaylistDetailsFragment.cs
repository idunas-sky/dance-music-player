
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Idunas.DanceMusicPlayer.Fragments.Player;
using Idunas.DanceMusicPlayer.Fragments.PlaylistEditor;
using Idunas.DanceMusicPlayer.Fragments.Playlists;
using Idunas.DanceMusicPlayer.Fragments.SongChooser;
using Idunas.DanceMusicPlayer.Models;
using Idunas.DanceMusicPlayer.Services;

namespace Idunas.DanceMusicPlayer.Fragments.PlaylistDetails
{
    public class PlaylistDetailsFragment : NavFragment
    {
        private RecyclerView _rvItems;
        private PlaylistDetailsRvAdapter _rvAdapter;

        public override string Title => Playlist.Name;

        public override bool ShowBackNavigation => true;

        public override void OnBackNavigationPressed()
        {
            NavigateTo<PlaylistsFragment>(NavDirection.Backward);
        }

        public Playlist Playlist { get; set; }

        public PlaylistDetailsFragment()
        {
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.PlaylistDetails, container, false);

            _rvItems = view.FindViewById<RecyclerView>(Resource.Id.rvItems);
            _rvItems.HasFixedSize = true;
            _rvItems.SetLayoutManager(new LinearLayoutManager(Context));

            _rvAdapter = new PlaylistDetailsRvAdapter(Playlist);
            _rvAdapter.SongClick += (sender, e) =>
            {
                MainActivity.ShowPlayer(e, Playlist);
            };
            _rvItems.SetAdapter(_rvAdapter);

            return view;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.playlist_details, menu);
            base.OnCreateOptionsMenu(menu, inflater);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_add_songs:
                {
                    NavigateTo<SongChooserFragment>(f => f.Playlist = Playlist);
                    break;
                }
                case Resource.Id.action_edit:
                {
                    NavigateTo<PlaylistEditorFragment>(initalizer: f =>
                    {
                        f.Playlist = Playlist;
                        f.IsNew = false;
                    });
                    break;
                }
                case Resource.Id.action_delete:
                {
                    DeletePlaylistWithConfirmation();
                    break;
                }
            }

            return base.OnOptionsItemSelected(item);
        }

        private void DeletePlaylistWithConfirmation()
        {
            var message = string.Format(Context.GetString(Resource.String.message_delete_playlist), Playlist.Name);

            new AlertDialog.Builder(Context)
                .SetTitle(Resource.String.title_delete_playlist)
                .SetMessage(message)
                .SetPositiveButton(Resource.String.delete, (sender, e) =>
                {
                    PlaylistsService.Instance.Playlists.Remove(Playlist);
                    PlaylistsService.Instance.Save();
                    NavigateTo<PlaylistsFragment>(NavDirection.Backward);
                })
                .SetNegativeButton(Resource.String.cancel, (sender, e) => { })
                .Create()
                .Show();
        }
    }
}