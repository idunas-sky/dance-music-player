using Android.Support.V4.App;
using Idunas.DanceMusicPlayer.Activities;
using System;

namespace Idunas.DanceMusicPlayer.Fragments
{
    public abstract class NavFragment : Fragment, IAppBarFragment
    {
        protected MainActivity MainActivity => (MainActivity)Activity;

        #region --- Actionbar

        public virtual string Title
        {
            get
            {
                return Context.GetString(Resource.String.app_name);
            }
        }

        #endregion

        #region --- Navigation

        public event EventHandler<NavigationEventArgs> NavigationRequested;

        public virtual bool ShowBackNavigation
        {
            get { return false; }
        }

        public virtual int BackNavigationIcon
        {
            get { return Resource.Drawable.ic_arrow_left; }
        }

        protected void NavigateTo<T>(NavDirection direction) where T : NavFragment
        {
            NavigateTo<T>(direction, null);
        }

        protected void NavigateTo<T>(Action<T> initalizer) where T : NavFragment
        {
            NavigateTo(NavDirection.Forward, initalizer);
        }

        protected void NavigateTo<T>(
            NavDirection direction,
            Action<T> initalizer)
                where T : NavFragment
        {
            NavigationRequested?.Invoke(
                this,
                new NavigationEventArgs(typeof(T), direction, ConvertInitializer(initalizer)));
        }

        private Action<object> ConvertInitializer<T>(Action<T> initalizer) where T : NavFragment
        {
            if (initalizer == null)
            {
                return null;
            }

            return obj => initalizer((T)obj);
        }

        public virtual void OnBackNavigationPressed()
        {
        }

        #endregion
    }
}