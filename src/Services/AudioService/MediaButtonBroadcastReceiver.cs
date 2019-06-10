
using Android.App;
using Android.Content;
using Android.Util;
using Android.Views;
using Idunas.DanceMusicPlayer.Activities;
using Idunas.DanceMusicPlayer.Services.Settings;
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
                    if (musicPlayer.HasBookmarks && ShouldSkipToBookmark(context))
                    {
                        musicPlayer.SeekToPreviousBookmark();
                    }
                    else
                    {
                        musicPlayer.PlayPreviousSong();
                    }
                    
                    return;
                }
                case Keycode.MediaNext:
                {
                    if (musicPlayer.HasBookmarks && ShouldSkipToBookmark(context))
                    {
                        musicPlayer.SeekToNextBookmark();
                    }
                    else
                    {
                        musicPlayer.PlayNextSong();
                    }
                    
                    return;
                }
            }
        }

        private bool ShouldSkipToBookmark(Context context)
        {
            return new SettingsService(context).Settings.EnableLockscreenSkipToBookmark;
        }
    }
}