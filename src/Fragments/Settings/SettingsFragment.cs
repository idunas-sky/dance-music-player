using Android.Content;
using Android.OS;
using Android.Support.V7.Preferences;
using Idunas.DanceMusicPlayer.Fragments.Playlists;

namespace Idunas.DanceMusicPlayer.Fragments.Settings
{
    public class SettingsFragment : PreferenceFragmentCompat,
        INavFragment,
        ISharedPreferencesOnSharedPreferenceChangeListener
    {
        #region --- Navigation

        public string Title => Context.GetString(Resource.String.settings);

        public bool ShowBackNavigation => true;

        public int BackNavigationIcon => Resource.Drawable.ic_arrow_left;

        public void OnBackNavigationPressed()
        {
            NavManager.Instance.NavigateTo<PlaylistsFragment>(NavDirection.Backward);
        }

        #endregion

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            AddPreferencesFromResource(Resource.Xml.settings);

            for (var i = 0; i < PreferenceScreen.PreferenceCount; i++)
            {
                UpdatePreference(PreferenceScreen.GetPreference(i));
            }
        }

        public override void OnResume()
        {
            base.OnResume();
            PreferenceScreen.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
        }

        public override void OnPause()
        {
            base.OnPause();
            PreferenceScreen.SharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            UpdatePreference(FindPreference(key));
        }

        private void UpdatePreference(Preference preference)
        {
            if (preference is PreferenceCategory category)
            {
                for (var i = 0; i < category.PreferenceCount; i++)
                {
                    UpdatePreference(category.GetPreference(i));
                }

                return;
            }

            if (preference is EditTextPreference editTextPreference)
            {
                editTextPreference.Summary = FormatValue(editTextPreference.Text);
            }

            if (preference is ListPreference listPreference)
            {
                listPreference.Summary = FormatValue(listPreference.Entry);
            }
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
        }

        private string FormatValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return GetString(Resource.String.default_value);
            }

            return value.Replace("%", "%%");
        }
    }
}