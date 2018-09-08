using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using Idunas.DanceMusicPlayer.Fragments.PlaylistDetails;
using Idunas.DanceMusicPlayer.Fragments.Playlists;
using Idunas.DanceMusicPlayer.Models;
using Idunas.DanceMusicPlayer.Services;

namespace Idunas.DanceMusicPlayer.Fragments.PlaylistEditor
{
    public class PlaylistEditorFragment : NavFragment
    {
        private TextInputEditText _inputName;

        public bool IsNew { get; set; }
        public Playlist Playlist { get; set; }

        public override string Title => Playlist.Name;

        public override bool ShowBackNavigation => true;

        public override void OnBackNavigationPressed()
        {
            if (IsNew)
            {
                NavigateTo<PlaylistsFragment>(NavDirection.Backward);
            }
            else
            {
                NavigateTo<PlaylistDetailsFragment>(
                    NavDirection.Backward,
                    fragment => fragment.Playlist = Playlist);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.PlaylistEditor, container, false);
            _inputName = view.FindViewById<TextInputEditText>(Resource.Id.input_name);
            _inputName.Text = Playlist.Name;

            return view;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.playlist_editor, menu);
            base.OnCreateOptionsMenu(menu, inflater);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_save)
            {
                Playlist.Name = _inputName.Text;

                if (IsNew)
                {
                    PlaylistsService.Instance.Playlists.Add(Playlist);
                }

                PlaylistsService.Instance.Save();

                if (IsNew)
                {
                    NavigateTo<PlaylistsFragment>(NavDirection.Backward);
                }
                else
                {
                    NavigateTo<PlaylistDetailsFragment>(NavDirection.Backward, f => f.Playlist = Playlist);
                }
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}