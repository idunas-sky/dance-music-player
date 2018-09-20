using Android.Support.V4.App;

namespace Idunas.DanceMusicPlayer.Fragments
{
    public interface INavFragment
    {
        string Title { get; }

        bool ShowBackNavigation { get; }

        int BackNavigationIcon { get; }

        void OnBackNavigationPressed();
    }
}