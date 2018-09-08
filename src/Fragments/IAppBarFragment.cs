namespace Idunas.DanceMusicPlayer.Fragments
{
    public interface IAppBarFragment
    {
        string Title { get; }

        bool ShowBackNavigation { get; }

        int BackNavigationIcon { get; }

        void OnBackNavigationPressed();
    }
}