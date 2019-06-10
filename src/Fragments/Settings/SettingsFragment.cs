using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Preferences;
using Idunas.DanceMusicPlayer.Fragments.Playlists;
using Idunas.DanceMusicPlayer.Services;
using Idunas.DanceMusicPlayer.Util;

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

        private Preference _btnImportPlaylist;
        private Preference _btnExportPlaylist;
        private readonly string _exportFilePath;

        public SettingsFragment()
        {
            _exportFilePath = Path.Combine(
                   Android.OS.Environment.ExternalStorageDirectory.AbsolutePath,
                   Android.OS.Environment.DirectoryDownloads);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            AddPreferencesFromResource(Resource.Xml.settings);

            for (var i = 0; i < PreferenceScreen.PreferenceCount; i++)
            {
                UpdatePreference(PreferenceScreen.GetPreference(i));
            }

            // Add button handlers
            _btnImportPlaylist = FindPreference(GetString(Resource.String.key_import_playlist));
            _btnImportPlaylist.PreferenceClick += HandleImportPlaylistClick;

            _btnExportPlaylist = FindPreference(GetString(Resource.String.key_export_playlist));
            _btnExportPlaylist.PreferenceClick += HandleExportPlaylistClick;
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            // Do nothing
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

        private string FormatValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return GetString(Resource.String.default_value);
            }

            return value.Replace("%", "%%");
        }

        #region --- Permission management

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (PermissionRequest.WasGranted(grantResults))
            {
                switch (requestCode)
                {
                    case Constants.PermissionRequests.ExportPlaylists:
                    {
                        ExportPlaylists().ConfigureAwait(false);
                        return;
                    }
                    case Constants.PermissionRequests.ImportPlaylists:
                    {
                        _btnImportPlaylist.PerformClick();
                        return;
                    }
                }
            }
        }

        #endregion

        #region --- Importing / Exporting playlists

        private async void HandleImportPlaylistClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            if (!PermissionRequest.Storage.Request(
                Activity,
                Resource.String.rationale_playlist_export,
                Constants.PermissionRequests.ImportPlaylists))
            {
                return;
            }

            var directory = new DirectoryInfo(_exportFilePath);
            var files = directory.GetFiles("*.json").OrderBy(x => x.Name);

            if (!files.Any())
            {
                MessageBox
                    .Build(Activity)
                    .SetErrorMessageAndTitle(Resource.String.error_no_import_files, directory.FullName)
                    .Show();
                return;
            }

            var dialogResult = await MessageBox
                .Build(Activity)
                .SetTitle(Resource.String.import_playlist)
                .ShowWithSelectOptions(
                    files.Select(x => x.FullName).ToArray(),
                    files.Select(x => x.Name.Substring(0, x.Name.Length - ".json".Length)).ToArray());

            if (dialogResult.DialogResult == MessageBox.MessageBoxResult.Positive)
            {
                PlaylistsService.Instance.Import(dialogResult.SelectedKey);
            }
        }

        private async void HandleExportPlaylistClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            if (PlaylistsService.Instance.Playlists.Count == 0)
            {
                MessageBox
                    .Build(Activity)
                    .SetErrorMessageAndTitle(Resource.String.message_no_playlists_for_export)
                    .Show();
                return;
            }

            if (!PermissionRequest.Storage.Request(
                Activity,
                Resource.String.rationale_playlist_export,
                Constants.PermissionRequests.ExportPlaylists))
            {
                return;
            }

            // We do have the permission already, export ...
            await ExportPlaylists();
        }

        private async Task ExportPlaylists()
        {
            // Ask the user for a filename
            var dialogResult = await MessageBox
                .Build(Activity)
                .SetTitle(Resource.String.export_playlist)
                .ShowWithEditText(
                    Resource.String.name,
                    Resource.String.save,
                    Resource.String.cancel);

            if (dialogResult.DialogResult == MessageBox.MessageBoxResult.Positive)
            {
                if (string.IsNullOrEmpty(dialogResult.Text))
                {
                    MessageBox
                        .Build(Activity)
                        .SetErrorMessageAndTitle(Resource.String.error_invalid_filename, dialogResult.Text)
                        .Show();
                    return;
                }

                var filePath = Path.Combine(_exportFilePath, $"{dialogResult.Text}.json");
                PlaylistsService.Instance.Export(filePath);
            }
        }

        #endregion
    }
}