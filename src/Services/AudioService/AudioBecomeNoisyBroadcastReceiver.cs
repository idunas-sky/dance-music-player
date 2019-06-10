
using Android.App;
using Android.Content;
using Android.Media;

namespace Idunas.DanceMusicPlayer.Services.AudioService
{
    [BroadcastReceiver]
    [IntentFilter(new[] { AudioManager.ActionAudioBecomingNoisy })]
    public class AudioBecomeNoisyBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action != AudioManager.ActionAudioBecomingNoisy)
            {
                return;
            }

            var serviceBinder = PeekService(context, new Intent(context, typeof(ForegroundAudioService))) as ForegroundAudioServiceBinder;
            if (serviceBinder == null)
            {
                return;
            }

            serviceBinder.Service?.MusicPlayer?.Pause();
        }
    }
}