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
using System;
using System.Threading.Tasks;
using System.Timers;

namespace Idunas.DanceMusicPlayer.Services.Player
{
    [Service(Exported = true)]
    [IntentFilter(new string[] {
        Intent.ActionMediaButton,
        AudioManager.ActionAudioBecomingNoisy,
        MediaBrowserServiceCompat.ServiceInterface
    })]
    public class BackgroundAudioService : MediaBrowserServiceCompat, AudioManager.IOnAudioFocusChangeListener
    {
        public const string START_FOREGROUND_ACTION = "de.idunas.dancemusicplayer.action.startforeground";
        public const int SERVICE_RUNNING_NOTIFICATION_ID = 1;

        public static AudioAttributes PlaybackAttributes = new AudioAttributes.Builder()
            .SetUsage(AudioUsageKind.Media)
            .SetContentType(AudioContentType.Music)
            .Build();

        private MediaSessionCompat _mediaSession;
        private MediaSessionCallback _mediaSessionCallback;
        private AudioBecomeNoisyBroadcastReceiver _noisyReceiver;

        public IMusicPlayer MusicPlayer { get; private set; }

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
            MediaButtonReceiver.HandleIntent(_mediaSession, intent);

            switch (intent.Action)
            {
                case START_FOREGROUND_ACTION:
                {
                    StartForeground(
                        SERVICE_RUNNING_NOTIFICATION_ID,
                        MediaSessionHelper.GetNotification(this, _mediaSession, MusicPlayer));
                    break;
                }
            }

            return StartCommandResult.Sticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            Binder = new BackgroundAudioServiceBinder(this);
            return Binder;
        }

        public override void OnCreate()
        {
            base.OnCreate();

            // Init media session
            _mediaSession = InitMediaSession();
            SessionToken = _mediaSession.SessionToken;

            // Init music player
            MusicPlayer = new MusicPlayer(this, _mediaSession, this);

            // Init callback receiver
            _mediaSessionCallback = new MediaSessionCallback(MusicPlayer);
            _mediaSession.SetCallback(_mediaSessionCallback);
            _mediaSession.Active = true;

            // Init noisy receiver
            _noisyReceiver = new AudioBecomeNoisyBroadcastReceiver();
            RegisterReceiver(_noisyReceiver, new IntentFilter(AudioManager.ActionAudioBecomingNoisy));
        }

        private MediaSessionCompat InitMediaSession()
        {
            var mediaButtonReceiver = new ComponentName(this, Java.Lang.Class.FromType(typeof(MediaButtonReceiver)));
            var mediaSession = new MediaSessionCompat(this, GetString(Resource.String.app_name), mediaButtonReceiver, null);
            mediaSession.SetFlags(MediaSessionCompat.FlagHandlesMediaButtons | MediaSessionCompat.FlagHandlesTransportControls);
            mediaSession.SetSessionActivity(PendingIntent.GetActivity(this, 0, new Intent(this, typeof(MainActivity)), 0));

            var mediaButtonIntent = new Intent(Intent.ActionMediaButton);
            mediaButtonIntent.SetClass(this, Java.Lang.Class.FromType(typeof(MediaButtonReceiver)));
            var pendingIntent = PendingIntent.GetBroadcast(this, 0, mediaButtonIntent, 0);
            mediaSession.SetMediaButtonReceiver(pendingIntent);

            mediaSession.SetMetadata(new MediaMetadataCompat.Builder()
                .PutString(MediaMetadataCompat.MetadataKeyDisplayTitle, GetString(Resource.String.no_song_selected))
                .Build());

            return mediaSession;
        }

        public override void OnDestroy()
        {
            Binder = null;
            UnregisterReceiver(_noisyReceiver);

            MusicPlayer.Dispose();
            _mediaSession.Release();

            base.OnDestroy();
        }

        #endregion


        #region --- IOnAudioFocusChangeListener implementation

        public void OnAudioFocusChange([GeneratedEnum] AudioFocus focusChange)
        {
            if (MusicPlayer == null)
            {
                return;
            }

            switch (focusChange)
            {
                case AudioFocus.Gain:
                {
                    MusicPlayer.Play(false);
                    return;
                }
                case AudioFocus.Loss:
                {
                    MusicPlayer.Stop();
                    return;
                }
                case AudioFocus.LossTransient:
                {
                    MusicPlayer.Pause();
                    return;
                }
                case AudioFocus.LossTransientCanDuck:
                {
                    MusicPlayer.LowerVolume();
                    return;
                }
            }
        }

        #endregion
    }
}