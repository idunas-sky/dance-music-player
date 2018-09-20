using Android.Support.V4.App;
using System;

namespace Idunas.DanceMusicPlayer.Fragments
{
    public sealed class NavManager
    {
        #region --- Singleton

        private static Lazy<NavManager> _instance = new Lazy<NavManager>(() => new NavManager());

        public static NavManager Instance
        {
            get { return _instance.Value; }
        }

        private NavManager()
        {
        }

        #endregion

        public event EventHandler<NavigationEventArgs> NavigationRequested;

        public void NavigateTo<T>(NavDirection direction) where T : Fragment, INavFragment
        {
            NavigateTo<T>(direction, null);
        }

        public void NavigateTo<T>(Action<T> initalizer) where T : Fragment, INavFragment
        {
            NavigateTo(NavDirection.Forward, initalizer);
        }

        public void NavigateTo<T>(
            NavDirection direction,
            Action<T> initalizer)
                where T : Fragment, INavFragment
        {
            NavigationRequested?.Invoke(
                this,
                new NavigationEventArgs(typeof(T), direction, ConvertInitializer(initalizer)));
        }

        private Action<object> ConvertInitializer<T>(Action<T> initalizer) where T : Fragment, INavFragment
        {
            if (initalizer == null)
            {
                return null;
            }

            return obj => initalizer((T)obj);
        }
    }
}