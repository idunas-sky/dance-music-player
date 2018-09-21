using Android.Content;
using Android.Preferences;
using System;

namespace Idunas.DanceMusicPlayer.Services.Settings
{
    public class SettingsService : Java.Lang.Object, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        private readonly Context _context;
        private ISharedPreferences _preferences;

        public event EventHandler<AppSettings> SettingsChanged;

        public AppSettings Settings
        {
            get
            {
                return new AppSettings
                {
                    SpeedMin = GetStringAsInt(_preferences, "speed_min", 60),
                    SpeedMax = GetStringAsInt(_preferences, "speed_max", 110)
                };
            }
        }

        public SettingsService(Context context)
        {
            _context = context;
            _preferences = PreferenceManager.GetDefaultSharedPreferences(_context);
            _preferences.RegisterOnSharedPreferenceChangeListener(this);
        }

        private int GetStringAsInt(ISharedPreferences preferences, string key, int defaultValue)
        {
            var value = preferences.GetString(key, null);
            if (value == null)
            {
                return defaultValue;
            }

            if (int.TryParse(value, out var result))
            {
                return result;
            }

            return defaultValue;
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            _preferences = sharedPreferences;
            SettingsChanged?.Invoke(this, Settings);
        }
    }
}