using Android.Content;
using Android.OS;
using Android.Support.V4.Content;
using System;
using System.Threading.Tasks;

namespace Idunas.DanceMusicPlayer.Services.Player
{
    public class PlayerServiceController : Java.Lang.Object, IServiceConnection
    {
        public event EventHandler Connected;

        private readonly Context _context;
        private PlayerServiceBinder _binder;
        private TaskCompletionSource<PlayerServiceBinder> _connectionCompletionSource;

        public IPlayerService Service
        {
            get { return _binder?.Service; }
        }

        public PlayerServiceController(Context context)
        {
            _context = context;
            _binder = null;
        }

        public void Start()
        {
            var intent = new Intent(_context, typeof(PlayerService));
            intent.SetAction(PlayerService.START_FOREGROUND_ACTION);

            ContextCompat.StartForegroundService(_context, intent);

            _connectionCompletionSource = new TaskCompletionSource<PlayerServiceBinder>();
            _context.BindService(intent, this, Bind.None);
        }

        public void Stop()
        {
            _context.UnbindService(this);
            _context.StopService(new Intent(_context, typeof(PlayerService)));
        }

        #region --- IServiceConnection implementation

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            _binder = (PlayerServiceBinder)service;
            Connected?.Invoke(this, EventArgs.Empty);
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            _binder = null;
        }

        #endregion
    }
}