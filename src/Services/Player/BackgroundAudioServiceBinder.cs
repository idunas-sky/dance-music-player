using Android.OS;

namespace Idunas.DanceMusicPlayer.Services.Player
{
    public class BackgroundAudioServiceBinder : Binder
    {
        public BackgroundAudioService Service { get; private set; }

        public BackgroundAudioServiceBinder(BackgroundAudioService service)
        {
            Service = service;
        }
    }
}