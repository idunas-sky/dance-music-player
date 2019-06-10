using System.ComponentModel;

namespace Idunas.DanceMusicPlayer.Services.Settings
{
    public class AppSettings
    {
        public int SpeedMin { get; set; }

        public int SpeedMax { get; set; }

        public PositionDurationClickAction PositionClickAction { get; set; }

        public PositionDurationClickAction DurationClickAction { get; set; }

        public bool EnableLockscreenSkipToBookmark { get; set; }
    }
}