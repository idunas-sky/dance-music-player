using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;
using Idunas.DanceMusicPlayer.Activities;
using System;

namespace Idunas.DanceMusicPlayer.Services.Player
{
    public class MediaSessionHelper
    {
        public static Notification GetNotification(Context context, IMusicPlayer musicPlayer)
        {
            var mediaSession = musicPlayer.MediaSession;
            var description = mediaSession.Controller.Metadata.Description;
            var currentPosition = string.Format(
                @"{0:mm\:ss} | {1:mm\:ss}",
                TimeSpan.FromMilliseconds(mediaSession.Controller.PlaybackState.Position),
                TimeSpan.FromMilliseconds(mediaSession.Controller.Metadata.GetLong(MediaMetadataCompat.MetadataKeyDuration)));

            var builder = new NotificationCompat.Builder(context, MainActivity.NOTIFICATION_CHANNEL_ID)
                .SetContentTitle(description.Title)
                .SetContentText(currentPosition)
                .SetLargeIcon(description.IconBitmap)
                .SetContentIntent(mediaSession.Controller.SessionActivity)
                .SetVisibility(NotificationCompat.VisibilityPublic)
                .SetSmallIcon(Resource.Mipmap.ic_launcher)
                .SetOngoing(true)
                .SetOnlyAlertOnce(true);

            if (musicPlayer.CurrentSong != null)
            {
                builder.AddAction(new NotificationCompat.Action(
                    Resource.Drawable.ic_skip_previous,
                    context.GetString(Resource.String.prev_song),
                    MediaButtonReceiver.BuildMediaButtonPendingIntent(context, PlaybackStateCompat.ActionSkipToPrevious)));

                if (musicPlayer.State == PlayerState.Playing)
                {
                    builder.AddAction(new NotificationCompat.Action(
                        Resource.Drawable.ic_pause,
                        context.GetString(Resource.String.pause),
                        MediaButtonReceiver.BuildMediaButtonPendingIntent(context, PlaybackStateCompat.ActionPause)));
                }
                else
                {
                    builder.AddAction(new NotificationCompat.Action(
                        Resource.Drawable.ic_play,
                        context.GetString(Resource.String.play),
                        MediaButtonReceiver.BuildMediaButtonPendingIntent(context, PlaybackStateCompat.ActionPlay)));
                }

                builder.AddAction(new NotificationCompat.Action(
                    Resource.Drawable.ic_skip_next,
                    context.GetString(Resource.String.next_song),
                    MediaButtonReceiver.BuildMediaButtonPendingIntent(context, PlaybackStateCompat.ActionSkipToNext)));
            }

            builder.SetStyle(
                    new Android.Support.V4.Media.App.NotificationCompat.MediaStyle()
                        .SetMediaSession(mediaSession.SessionToken));

            return builder.Build();
        }
    }
}