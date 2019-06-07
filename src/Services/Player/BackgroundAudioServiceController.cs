using Android.Content;
using Android.OS;
using Android.Support.V4.Content;
using System;
using System.Threading.Tasks;

namespace Idunas.DanceMusicPlayer.Services.Player
{
    public class BackgroundAudioServiceController : Java.Lang.Object, IServiceConnection
    {
        public event EventHandler Connected;

        private readonly Context _context;
        private BackgroundAudioServiceBinder _binder;

        public IMusicPlayer MusicPlayer
        {
            get { return _binder?.Service?.MusicPlayer; }
        }

        public BackgroundAudioServiceController(Context context)
        {
            _context = context;
            _binder = null;
        }

        public void Start()
        {
            var intent = new Intent(_context, typeof(BackgroundAudioService));
            intent.SetAction(BackgroundAudioService.START_FOREGROUND_ACTION);

            ContextCompat.StartForegroundService(_context, intent);
            _context.BindService(intent, this, Bind.None);
        }

        public void Stop()
        {
            _context.UnbindService(this);
            _context.StopService(new Intent(_context, typeof(BackgroundAudioService)));
        }

        #region --- IServiceConnection implementation

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            _binder = (BackgroundAudioServiceBinder)service;
            Connected?.Invoke(this, EventArgs.Empty);
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            _binder = null;
        }

        #endregion
    }
}