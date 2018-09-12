using Android.OS;
using Android.Views;
using Idunas.DanceMusicPlayer.Fragments.Playlists;

namespace Idunas.DanceMusicPlayer.Fragments.About
{
    public class AboutFragment : NavFragment
    {
        public override bool ShowBackNavigation => true;

        public override void OnBackNavigationPressed()
        {
            NavigateTo<PlaylistsFragment>(NavDirection.Backward);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.About, container, false);
        }
    }
}