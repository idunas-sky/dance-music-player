using Android.OS;

namespace Idunas.DanceMusicPlayer.Services.AudioService
{
    public class ForegroundAudioServiceBinder : Binder
    {
        public ForegroundAudioService Service { get; private set; }

        public ForegroundAudioServiceBinder(ForegroundAudioService service)
        {
            Service = service;
        }
    }
}