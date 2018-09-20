using Android.Support.V4.App;
using Idunas.DanceMusicPlayer.Activities;
using System;

namespace Idunas.DanceMusicPlayer.Fragments
{
    public abstract class NavFragment : Fragment, INavFragment
    {
        protected MainActivity MainActivity => (MainActivity)Activity;

        public virtual string Title
        {
            get
            {
                return Context.GetString(Resource.String.app_name);
            }
        }

        public virtual bool ShowBackNavigation
        {
            get { return false; }
        }

        public virtual int BackNavigationIcon
        {
            get { return Resource.Drawable.ic_arrow_left; }
        }

        public virtual void OnBackNavigationPressed()
        {
        }
    }
}