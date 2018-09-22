using Android.App;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;

namespace Idunas.DanceMusicPlayer.Util
{
    public static class KeyboardUtils
    {
        public static void ShowKeyboard(Context context, View view)
        {
            var inputManager = (InputMethodManager)context.GetSystemService(Context.InputMethodService);
            inputManager.ToggleSoftInput(ShowFlags.Forced, 0);
        }

        public static void HideKeyboard(Activity activity)
        {
            HideKeyboard(activity, activity.CurrentFocus);
        }

        public static void HideKeyboard(Context context, View view)
        {
            var inputManager = (InputMethodManager)context.GetSystemService(Context.InputMethodService);

            if (view == null)
            {
                view = new View(context);
            }

            inputManager.HideSoftInputFromWindow(view.WindowToken, 0);
        }
    }
}