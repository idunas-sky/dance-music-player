using Android.App;
using Android.Widget;
using System;

namespace Idunas.DanceMusicPlayer.Services
{
    public static class MessageService
    {
        public static void ShowError(Exception ex, string message, params object[] args)
        {
            message = message + ": " + ex.Message;
            ShowLongMessage(message, args);
        }

        public static void ShowLongMessage(string message, params object[] args)
        {
            ShowInternal(ToastLength.Long, message, args);
        }

        public static void ShowHelpText(int resId, params object[] args)
        {
            ShowMessage(Application.Context.GetString(resId), args);
        }

        public static void ShowMessage(string message, params object[] args)
        {
            ShowInternal(ToastLength.Short, message, args);
        }

        private static void ShowInternal(ToastLength length, string message, object[] args)
        {
            Toast
                .MakeText(Application.Context, string.Format(message, args), length)
                .Show();
        }
    }
}