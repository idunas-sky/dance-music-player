using Android.Content;
using Android.Preferences;
using System;
using System.Collections.Generic;

namespace Idunas.DanceMusicPlayer.Services.Settings
{
    public class SettingsService : Java.Lang.Object, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        private readonly Context _context;
        private ISharedPreferences _preferences;

        public event EventHandler<AppSettings> SettingsChanged;

        public AppSettings Settings { get; private set; }

        public SettingsService(Context context)
        {
            _context = context;
            _preferences = PreferenceManager.GetDefaultSharedPreferences(_context);
            _preferences.RegisterOnSharedPreferenceChangeListener(this);

            Settings = InitAppSettings();
        }

        private AppSettings InitAppSettings()
        {
            return new AppSettings
            {
                SpeedMin = GetIntValue("speed_min", 60),
                SpeedMax = GetIntValue("speed_max", 110),
                PositionClickAction = GetEnumValue("position_click_action", PositionDurationClickAction.ChangeSeconds10),
                DurationClickAction = GetEnumValue("duration_click_action", PositionDurationClickAction.ChangeSeconds10),
                EnableLockscreenSkipToBookmark = _preferences.GetBoolean("enable_lockscreen_skip_to_bookmark", true)
            };
        }

        private int GetIntValue(string key, int defaultValue)
        {
            var value = _preferences.GetString(key, null);
            if (value == null)
            {
                return defaultValue;
            }

            if (!int.TryParse(value, out var result))
            {
                return defaultValue;
            }

            return result;
        }

        private T GetEnumValue<T>(string key, T defaultValue) where T : struct, IConvertible
        {
            var value = _preferences.GetString(key, null);
            if (value == null)
            {
                return defaultValue;
            }

            if (!int.TryParse(value, out var intValue))
            {
                return defaultValue;
            }

            return (T)Enum.ToObject(typeof(T), intValue);
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            _preferences = sharedPreferences;
            Settings = InitAppSettings();

            SettingsChanged?.Invoke(this, Settings);
        }
    }
}