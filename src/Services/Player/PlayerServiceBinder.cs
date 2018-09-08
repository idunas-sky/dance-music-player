using Android.OS;

namespace Idunas.DanceMusicPlayer.Services.Player
{
    public class PlayerServiceBinder : Binder
    {
        public PlayerService Service { get; private set; }

        public PlayerServiceBinder(PlayerService service)
        {
            Service = service;
        }
    }
}