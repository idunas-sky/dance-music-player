using Android.App;
using Android.Support.Design.Widget;
using Android.Widget;
using System;

namespace Idunas.DanceMusicPlayer.Services
{
    public class ErrorService
    {
        #region --- Singleton

        private static Lazy<ErrorService> _instance = new Lazy<ErrorService>(() => new ErrorService());

        public static ErrorService Instance
        {
            get { return _instance.Value; }
        }

        private ErrorService() { }

        #endregion

        public void ShowError(Exception ex, string message, params object[] args)
        {
            message = message + ": " + ex.Message;
            ShowError(message, args);
        }


        public void ShowError(string message, params object[] args)
        {
            Toast
                .MakeText(Application.Context, string.Format(message, args), ToastLength.Long)
                .Show();
        }
    }
}