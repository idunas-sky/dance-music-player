using Android.App;
using Android.Content;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;
using Android.Text;
using Android.Widget;
using Idunas.DanceMusicPlayer.Activities;
using Idunas.DanceMusicPlayer.Models;
using Idunas.DanceMusicPlayer.Services.Player;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace Idunas.DanceMusicPlayer.Services.AudioService
{
    [Service(Exported = true)]
    [IntentFilter(new string[] {
        Intent.ActionMediaButton,
        AudioManager.ActionAudioBecomingNoisy,
        MediaBrowserServiceCompat.ServiceInterface
    })]
    public class ForegroundAudioService : MediaBrowserServiceCompat
    {
        private AudioBecomeNoisyBroadcastReceiver _noisyReceiver;

        public IMusicPlayer MusicPlayer { get { return MainActivity.MusicPlayer; } }

        public IBinder Binder { get; private set; }

        #region Abstract methods & interface implementations

        public override BrowserRoot OnGetRoot(string clientPackageName, int clientUid, Bundle rootHints)
        {
            if (clientPackageName == PackageName)
            {
                return new BrowserRoot(GetString(Resource.String.app_name), null);
            }

            return null;
        }

        public override void OnLoadChildren(string parentId, Result result)
        {
            // Not important for general audio service
            result.SendResult(null);
        }

        #endregion

        #region Startup / Shutdown

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            MediaButtonReceiver.HandleIntent(MusicPlayer.MediaSession, intent);

            switch (intent.Action)
            {
                case Constants.START_FOREGROUND_ACTION:
                {
                    StartForeground(
                        Constants.SERVICE_RUNNING_NOTIFICATION_ID,
                        MediaSessionHelper.GetNotification(this, MusicPlayer));
                    break;
                }
            }

            return StartCommandResult.Sticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            Binder = new ForegroundAudioServiceBinder(this);
            return Binder;
        }

        public override void OnCreate()
        {
            base.OnCreate();

            // Init media session
            SessionToken = MusicPlayer.MediaSession.SessionToken;

            // Init noisy receiver
            _noisyReceiver = new AudioBecomeNoisyBroadcastReceiver();
            RegisterReceiver(_noisyReceiver, new IntentFilter(AudioManager.ActionAudioBecomingNoisy));
        }



        public override void OnDestroy()
        {
            Binder = null;
            UnregisterReceiver(_noisyReceiver);

            base.OnDestroy();
        }

        #endregion
    }
}