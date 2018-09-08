using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Idunas.DanceMusicPlayer.Activities;

namespace Idunas.DanceMusicPlayer
{
    public class FragmentLifecycleManager : FragmentManager.FragmentLifecycleCallbacks
    {
        private MainActivity _activity;

        public FragmentLifecycleManager(MainActivity activity)
        {
            _activity = activity;
        }

        public override void OnFragmentViewCreated(FragmentManager fm, Fragment f, View v, Bundle savedInstanceState)
        {
            base.OnFragmentViewCreated(fm, f, v, savedInstanceState);

            _activity.InvalidateActionBar();
        }
    }
}