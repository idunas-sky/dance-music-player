using Android.Content;
using Android.OS;
using Android.Support.V4.Content;
using Idunas.DanceMusicPlayer.Services.Player;
using System;

namespace Idunas.DanceMusicPlayer.Services.AudioService
{
    public class ForegroundAudioServiceController : Java.Lang.Object, IServiceConnection
    {
        public event EventHandler Connected;

        private readonly Context _context;
        private ForegroundAudioServiceBinder _binder;

        public IMusicPlayer MusicPlayer
        {
            get { return _binder?.Service?.MusicPlayer; }
        }

        public ForegroundAudioServiceController(Context context)
        {
            _context = context;
            _binder = null;
        }

        public void Start()
        {
            var intent = new Intent(_context, typeof(ForegroundAudioService));
            intent.SetAction(Constants.START_FOREGROUND_ACTION);

            ContextCompat.StartForegroundService(_context, intent);
            _context.BindService(intent, this, Bind.None);
        }

        public void Stop()
        {
            _context.UnbindService(this);
            _context.StopService(new Intent(_context, typeof(ForegroundAudioService)));
        }

        #region --- IServiceConnection implementation

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            _binder = (ForegroundAudioServiceBinder)service;
            Connected?.Invoke(this, EventArgs.Empty);
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            _binder = null;
        }

        #endregion
    }
}