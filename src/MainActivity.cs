using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Idunas.DanceMusicPlayer.Fragments;
using Idunas.DanceMusicPlayer.Fragments.Player;
using Idunas.DanceMusicPlayer.Fragments.Playlists;
using Idunas.DanceMusicPlayer.Fragments.SongBar;
using Idunas.DanceMusicPlayer.Models;
using System;
using System.Collections.Generic;

namespace Idunas.DanceMusicPlayer.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        #region --- Private member

        private SongBarFragment FragmentSongBar { get; set; }
        private PlayerFragment FragmentPlayer { get; set; }
        private View ViewBottomShadow { get; set; }

        private BottomSheetBehavior _bottomSheetBehavior;
        private Dictionary<Type, NavFragment> _fragments = new Dictionary<Type, NavFragment>();

        #endregion

        public Android.Support.V4.App.Fragment MainFragment
        {
            get
            {
                if (_bottomSheetBehavior.State == BottomSheetBehavior.StateExpanded)
                {
                    return FragmentPlayer;
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

            // Top actionbar / toolbar
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);


            // TODO
            //MainLayout = FindViewById<View>(Resource.Id.layout_main);

            // Show initial main fragment
            ShowFragment(typeof(PlaylistsFragment), NavDirection.Forward, null);

            // Bottom sheet (Songbar)
            PrepareSongBar();
        }

        private void PrepareSongBar()
        {
            var bottomBar = FindViewById<LinearLayout>(Resource.Id.bottom_sheet);
            ViewBottomShadow = FindViewById<View>(Resource.Id.pnl_shadow);
            _bottomSheetBehavior = BottomSheetBehavior.From(bottomBar);
            FragmentSongBar = (SongBarFragment)SupportFragmentManager.FindFragmentById(Resource.Id.fragment_song_bar);
            FragmentPlayer = (PlayerFragment)SupportFragmentManager.FindFragmentById(Resource.Id.fragment_player);

            _bottomSheetBehavior.SetBottomSheetCallback(new SongbarSheetCallback(this));
        }


        #region --- Bottom sheet layout handling

        private class SongbarSheetCallback : BottomSheetBehavior.BottomSheetCallback
        {
            private readonly MainActivity _mainActivity;
            private int _originalSongbarHeight = 0;

            public SongbarSheetCallback(MainActivity mainActivity)
            {
                _mainActivity = mainActivity;
            }

            public override void OnSlide(View bottomSheet, float slideOffset)
            {
                // Nothing to do
            }

            public override void OnStateChanged(View bottomSheet, int newState)
            {
                // We need to hide the shadow when expanding to allow the bottom-toolbar to slide
                // under the main toolbar. Respectively we need to show it again when collapsing
                switch (newState)
                {
                    case BottomSheetBehavior.StateExpanded:
                    {
                        // Save previous height to restore it when collapsing
                        var layoutParams = _mainActivity.FragmentSongBar.View.LayoutParameters;
                        if (_originalSongbarHeight <= 0)
                        {
                            _originalSongbarHeight = layoutParams.Height;
                        }

                        // Adjust height ...
                        layoutParams.Height = _mainActivity.SupportActionBar.Height;
                        _mainActivity.FragmentSongBar.View.LayoutParameters = layoutParams;

                        // ... and hide the shadow
                        _mainActivity.ViewBottomShadow.Visibility = ViewStates.Gone;
                        _mainActivity.InvalidateActionBar();
                        break;
                    }
                    case BottomSheetBehavior.StateCollapsed:
                    {
                        // Restore previous height ...
                        var layoutParams = _mainActivity.FragmentSongBar.View.LayoutParameters;
                        if (_originalSongbarHeight > 0)
                        {
                            layoutParams.Height = _originalSongbarHeight;
                        }
                        _mainActivity.FragmentSongBar.View.LayoutParameters = layoutParams;

                        // ... and show the shadow
                        _mainActivity.ViewBottomShadow.Visibility = ViewStates.Visible;
                        _mainActivity.InvalidateActionBar();
                        break;
                    }
                }
            }
        }

        #endregion

        #region --- SongBar / Player handling

        public void ShowPlayer(Song song = null, Playlist playlist = null)
        {
            _bottomSheetBehavior.State = BottomSheetBehavior.StateExpanded;

            if (song != null)
            {
                FragmentPlayer.PlaySong(song, playlist);
            }
        }

        public void HidePlayer()
        {
            _bottomSheetBehavior.State = BottomSheetBehavior.StateCollapsed;
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
            if (OnBackNavigationRequested())
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