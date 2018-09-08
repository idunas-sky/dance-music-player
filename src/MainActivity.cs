using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Idunas.DanceMusicPlayer.Fragments;
using Idunas.DanceMusicPlayer.Fragments.Player;
using Idunas.DanceMusicPlayer.Fragments.Playlists;
using Idunas.DanceMusicPlayer.Fragments.SongBar;
using Idunas.DanceMusicPlayer.Models;
using Idunas.DanceMusicPlayer.Util;
using System;
using System.Collections.Generic;

namespace Idunas.DanceMusicPlayer.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        #region --- Private member

        private FrameLayout _containerSongBar;

        private SongBarFragment _fragmentSongBar;
        private PlayerFragment _fragmentPlayer;

        private Dictionary<Type, NavFragment> _fragments = new Dictionary<Type, NavFragment>();

        #endregion

        public Android.Support.V4.App.Fragment MainFragment
        {
            get
            {
                if (_fragmentPlayer.IsVisible)
                {
                    return _fragmentPlayer;
                }

                return SupportFragmentManager.FindFragmentById(Resource.Id.fragment_container);
            }
        }

        public static View MainLayout { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);
            SupportFragmentManager.RegisterFragmentLifecycleCallbacks(new FragmentLifecycleManager(this), false);

            MainLayout = FindViewById<View>(Resource.Id.layout_main);
            _containerSongBar = FindViewById<FrameLayout>(Resource.Id.song_bar_container);

            // Show initial main fragment
            ShowFragment(typeof(PlaylistsFragment), NavDirection.Forward, null);

            // Pepare songbar & player
            PrepareSongBar();
        }

        #region --- SongBar / Player handling

        private void PrepareSongBar()
        {
            _fragmentSongBar = new SongBarFragment();
            _fragmentPlayer = new PlayerFragment();

            HidePlayer();
        }

        public void ShowPlayer(Song song = null, Playlist playlist = null)
        {
            if (_fragmentPlayer.IsVisible)
            {
                return;
            }

            _containerSongBar.LayoutParameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent);
            _containerSongBar.RequestLayout();

            if (song != null)
            {
                _fragmentPlayer.SetSong(song, playlist);
            }

            SupportFragmentManager
                .BeginTransaction()
                .Replace(Resource.Id.song_bar_container, _fragmentPlayer)
                .Commit();
        }

        public void HidePlayer()
        {
            if (_fragmentSongBar.IsVisible)
            {
                return;
            }

            _containerSongBar.LayoutParameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                DipConvert.ToPixel(this, 70));
            _containerSongBar.RequestLayout();

            SupportFragmentManager
                .BeginTransaction()
                .Replace(Resource.Id.song_bar_container, _fragmentSongBar)
                .Commit();
        }

        #endregion

        #region --- Fragment navigation

        private void ShowFragment(Type fragmentType, NavDirection direction, Action<object> initializer)
        {
            // Only create a fragment if it doesn't exist yet
            if (!_fragments.TryGetValue(fragmentType, out var fragment))
            {
                // Prepare new fragment
                fragment = (NavFragment)Activator.CreateInstance(fragmentType);
                fragment.NavigationRequested += HandleNavigationRequested;
                _fragments.Add(fragmentType, fragment);
            }

            // Init fragment
            initializer?.Invoke(fragment);

            // Navigate to new fragment
            var enterAnimation = direction == NavDirection.Forward
                ? Resource.Animation.enter_from_right
                : Resource.Animation.enter_from_left;
            var exitAnimation = direction == NavDirection.Forward
                ? Resource.Animation.exit_to_left
                : Resource.Animation.exit_to_right;

            SupportFragmentManager
                .BeginTransaction()
                .SetCustomAnimations(enterAnimation, exitAnimation)
                .Replace(Resource.Id.fragment_container, fragment)
                .Commit();
        }

        private void HandleNavigationRequested(object sender, NavigationEventArgs e)
        {
            ShowFragment(e.Target, e.Direction, e.Initializer);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MainFragment?.OnCreateOptionsMenu(menu, MenuInflater);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                OnBackNavigationRequested();
            }

            MainFragment?.OnOptionsItemSelected(item);

            return base.OnOptionsItemSelected(item);
        }

        public void InvalidateActionBar()
        {
            InvalidateOptionsMenu();

            if (MainFragment is IAppBarFragment fragment)
            {
                SupportActionBar.SetDisplayHomeAsUpEnabled(fragment.ShowBackNavigation);
                SupportActionBar.SetHomeAsUpIndicator(fragment.BackNavigationIcon);
                SupportActionBar.Title = fragment.Title;
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            MainFragment?.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override void OnBackPressed()
        {
            if(OnBackNavigationRequested())
            {
                return;
            }

            base.OnBackPressed();
        }

        private bool OnBackNavigationRequested()
        {
            if (!(MainFragment is IAppBarFragment fragment))
            {
                return false;
            }

            if (!fragment.ShowBackNavigation)
            {
                return false;
            }

            fragment.OnBackNavigationPressed();
            return true;
        }

        #endregion
    }
}