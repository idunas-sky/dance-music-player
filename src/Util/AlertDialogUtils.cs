using Android.App;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using System.Threading.Tasks;

namespace Idunas.DanceMusicPlayer.Util
{
    public static class AlertDialogUtils
    {
        public enum AlertDialogResult
        {
            Positive,
            Negative
        }

        public class AlertDialogEditTextResult
        {
            public AlertDialogResult DialogResult { get; set; }

            public string Text { get; set; }
        }

        public static async Task<AlertDialogEditTextResult> ShowEditTextDialog(
            Context context,
            int titleResourceId,
            int inputLabelResourceId,
            int positiveButtonResourceId,
            int? negativeButtonResourceId = null)
        {
            // Prepare editor
            var layout = new LinearLayout(context);
            var txtInput = new EditText(Application.Context);
            txtInput.FocusChange += (sender, e) => ShowKeyboard(context, txtInput);
            txtInput.Hint = context.GetString(inputLabelResourceId);
            var layoutParams = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent);
            layoutParams.SetMargins(
                (int)context.Resources.GetDimension(Resource.Dimension.spacing_large), 
                0,
                (int)context.Resources.GetDimension(Resource.Dimension.spacing_large), 
                0);
            txtInput.LayoutParameters = layoutParams;
            layout.AddView(txtInput);

            var tcs = new TaskCompletionSource<AlertDialogEditTextResult>();

            var dialogBuilder = new AlertDialog.Builder(context)
                .SetTitle(titleResourceId)
                .SetView(layout)
                .SetPositiveButton(positiveButtonResourceId, (sender, e) =>
                {
                    HideKeyboard(context, txtInput);

                    tcs.TrySetResult(new AlertDialogEditTextResult
                    {
                        DialogResult = AlertDialogResult.Positive,
                        Text = txtInput.Text
                    });
                });

            if (negativeButtonResourceId != null)
            {
                dialogBuilder.SetNegativeButton(
                    negativeButtonResourceId.Value,
                    (sender, e) =>
                    {
                        HideKeyboard(context, txtInput);

                        tcs.TrySetResult(new AlertDialogEditTextResult
                        {
                            DialogResult = AlertDialogResult.Negative,
                            Text = txtInput.Text
                        });
                    });
            }

            dialogBuilder.Show();
            return await tcs.Task;
        }

        private static void ShowKeyboard(Context context, EditText txtInput)
        {
            var inputManager = (InputMethodManager)context.GetSystemService(Context.InputMethodService);
            inputManager.ToggleSoftInput(ShowFlags.Forced, 0);
        }

        private static void HideKeyboard(Context context, EditText txtInput)
        {
            var inputManager = (InputMethodManager)context.GetSystemService(Context.InputMethodService);
            inputManager.HideSoftInputFromWindow(txtInput.WindowToken, 0);
        }
    }
}