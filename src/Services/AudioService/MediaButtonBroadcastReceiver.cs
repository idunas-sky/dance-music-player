
using Android.App;
using Android.Content;
using Android.Util;
using Android.Views;
using Idunas.DanceMusicPlayer.Activities;
using Idunas.DanceMusicPlayer.Util;
using System;

namespace Idunas.DanceMusicPlayer.Services.AudioService
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

            var keyEvent = (KeyEvent)intent.GetParcelableExtra(Intent.ExtraKeyEvent);
            var musicPlayer = MainActivity.MusicPlayer;
            if (musicPlayer == null)
            {
                return;
            }

            switch (keyEvent.KeyCode)
            {
                default:
                {
                    // Unhandled media button, just do nothing
                    return;
                }
                case Keycode.MediaPlay:
                {
                    AsyncHelper.RunAndWait(() => musicPlayer.Play(true));
                    return;
                }
                case Keycode.MediaPause:
                {
                    musicPlayer.Pause();
                    return;
                }
                case Keycode.MediaPlayPause:
                {
                    if (musicPlayer.State == Player.PlayerState.Playing)
                    {
                        musicPlayer.Pause();
                    }
                    else
                    {
                        AsyncHelper.RunAndWait(() => musicPlayer.Play(true));
                    }

                    return;
                }
                case Keycode.MediaStop:
                {
                    musicPlayer.Stop();
                    return;
                }
                case Keycode.MediaPrevious:
                {
                    musicPlayer.PlayNextSong();
                    return;
                }
                case Keycode.MediaNext:
                {
                    musicPlayer.PlayPreviousSong();
                    return;
                }
            }
        }
    }
}