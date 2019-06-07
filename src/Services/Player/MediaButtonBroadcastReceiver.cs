
using Android.App;
using Android.Content;
using Android.Views;

namespace Idunas.DanceMusicPlayer.Services.Player
{
    [BroadcastReceiver]
    [IntentFilter(new[] { Intent.ActionMediaButton })]
    public class MediaButtonBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action != Intent.ActionMediaButton)
            {
                return;
            }

            // Redirect to media session callback
            context.StartService(new Intent(
                context,
                typeof(BackgroundAudioService))
                    .SetAction(intent.Action)
                    .PutExtra(Intent.ExtraKeyEvent, (KeyEvent)intent.GetParcelableExtra(Intent.ExtraKeyEvent)));
        }
    }
}