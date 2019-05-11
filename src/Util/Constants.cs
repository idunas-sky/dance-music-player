namespace Idunas.DanceMusicPlayer.Util
{
    public static class Constants
    {
        public static class PermissionRequests
        {
            public const int StartForegroundService = 2;

            // Storage access
            public const int ImportPlaylists = 10;
            public const int ExportPlaylists = 11;
            public const int PlaySongs = 12;
            public const int AddSongs = 13;
        }

        public static class Prefs
        {
            public const string SpeedMin = "speed_min";
            public const string SpeedMax = "speed_max";
        }
    }
}