using Android.Support.V4.Media.Session;
using Idunas.DanceMusicPlayer.Util;

namespace Idunas.DanceMusicPlayer.Services.Player
{
    public class MediaSessionCallback : MediaSessionCompat.Callback
    {
        private readonly IMusicPlayer _musicPlayer;

        public MediaSessionCallback(IMusicPlayer musicPlayer)
        {
            _musicPlayer = musicPlayer;
        }

        public override void OnPlay()
        {
            base.OnPlay();
            AsyncHelper.RunAndContinue(() => _musicPlayer.Play(true));
        }

        public override void OnPause()
        {
            base.OnPause();
            _musicPlayer.Pause();
        }

        public override void OnStop()
        {
            base.OnStop();
            _musicPlayer.Stop();
        }

        public override void OnSeekTo(long pos)
        {
            base.OnSeekTo(pos);
            _musicPlayer.SeekTo(pos);
        }

        public override void OnSetRepeatMode(int repeatMode)
        {
            base.OnSetRepeatMode(repeatMode);

            switch (repeatMode)
            {
                case PlaybackStateCompat.RepeatModeNone:
                default:
                {
                    _musicPlayer.IsLooping = false;
                    break;
                }
                case PlaybackStateCompat.RepeatModeOne:
                case PlaybackStateCompat.RepeatModeGroup:
                case PlaybackStateCompat.RepeatModeAll:
                {
                    _musicPlayer.IsLooping = true;
                    break;
                }
            }
        }

        public override void OnSetShuffleMode(int shuffleMode)
        {
            base.OnSetShuffleMode(shuffleMode);
        }

        public override void OnSkipToNext()
        {
            base.OnSkipToNext();
            AsyncHelper.RunAndContinue(() => _musicPlayer.PlayNextSong());
        }

        public override void OnSkipToPrevious()
        {
            base.OnSkipToPrevious();
            AsyncHelper.RunAndContinue(() => _musicPlayer.PlayPreviousSong());
        }
    }
}