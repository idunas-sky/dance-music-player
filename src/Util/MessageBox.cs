using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Idunas.DanceMusicPlayer.Util
{
    public sealed class MessageBox
    {
        private readonly AlertDialog.Builder _builder;
        private bool _hasButtons;

        private MessageBox(Context context)
        {
            _builder = new AlertDialog.Builder(context);
        }

        public static MessageBox Build(Context context)
        {
            return new MessageBox(context);
        }

        public MessageBox SetTitle(int resourceId)
        {
            _builder.SetTitle(resourceId);
            return this;
        }

        public MessageBox SetMessage(int resourceId, params object[] arguments)
        {
            if (arguments != null && arguments.Any())
            {
                _builder.SetMessage(string.Format(_builder.Context.GetString(resourceId), arguments));
            }
            else
            {
                _builder.SetMessage(resourceId);
            }

            return this;
        }

        public MessageBox SetErrorMessageAndTitle(int resourceid, params object[] arguments)
        {
            SetTitle(Resource.String.error);
            SetMessage(resourceid, arguments);
            return this;
        }

        public MessageBox SetSuccessMessageAndTitle(int resourceid, params object[] arguments)
        {
            SetTitle(Resource.String.success);
            SetMessage(resourceid, arguments);
            return this;
        }

        public MessageBox SetPositiveAction(int resourceId, Action action = null)
        {
            _builder.SetPositiveButton(resourceId, (sender, e) => action?.Invoke());
            _hasButtons = true;
            return this;
        }

        public MessageBox SetNegativeAction(int resourceId, Action action = null)
        {
            _builder.SetNegativeButton(resourceId, (sender, e) => action?.Invoke());
            _hasButtons = true;
            return this;
        }

        public async Task<MessageBoxEditTextResult> ShowWithEditText(
            int inputLabelResourceId,
            int positiveButtonResourceId,
            int? negativeButtonResourceId = null)
        {
            // Prepare editor
            var layout = new LinearLayout(_builder.Context);
            var txtInput = new EditText(Application.Context);
            txtInput.FocusChange += (sender, e) => KeyboardUtils.ShowKeyboard(_builder.Context, txtInput);
            txtInput.Hint = _builder.Context.GetString(inputLabelResourceId);
            var layoutParams = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent);
            layoutParams.SetMargins(
                (int)_builder.Context.Resources.GetDimension(Resource.Dimension.spacing_large),
                0,
                (int)_builder.Context.Resources.GetDimension(Resource.Dimension.spacing_large),
                0);
            txtInput.LayoutParameters = layoutParams;
            layout.AddView(txtInput);

            var tcs = new TaskCompletionSource<MessageBoxEditTextResult>();

            _builder.SetView(layout);
            SetPositiveAction(positiveButtonResourceId, () =>
            {
                KeyboardUtils.HideKeyboard(_builder.Context, txtInput);

                tcs.TrySetResult(new MessageBoxEditTextResult
                {
                    DialogResult = MessageBoxResult.Positive,
                    Text = txtInput.Text
                });
            });

            if (negativeButtonResourceId != null)
            {
                SetNegativeAction(negativeButtonResourceId.Value, () =>
                {
                    KeyboardUtils.HideKeyboard(_builder.Context, txtInput);

                    tcs.TrySetResult(new MessageBoxEditTextResult
                    {
                        DialogResult = MessageBoxResult.Negative,
                        Text = txtInput.Text
                    });
                });
            }

            Show();
            return await tcs.Task;
        }

        public async Task<MessageBoxSelectOptionResult> ShowWithSelectOptions(
            string[] keys,
            string[] values)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (keys.Length != values.Length)
            {
                throw new Exception($"Keys and values must have the same number of items (Keys={keys.Length}, Values={values.Length}");
            }

            var tcs = new TaskCompletionSource<MessageBoxSelectOptionResult>();

            _builder.SetItems(values, (sender, e) =>
            {
                tcs.TrySetResult(new MessageBoxSelectOptionResult
                {
                    DialogResult = MessageBoxResult.Positive,
                    SelectedKey = keys[e.Which],
                    SelectedValue = values[e.Which]
                });
            });

            SetNegativeAction(Resource.String.cancel, () =>
            {
                tcs.TrySetResult(new MessageBoxSelectOptionResult
                {
                    DialogResult = MessageBoxResult.Negative
                });
            });

            Show();
            return await tcs.Task;
        }

        public void Show()
        {
            if (!_hasButtons)
            {
                SetPositiveAction(Resource.String.ok);
            }

            _builder.Create().Show();
        }

        #region Helper classes for special Messageboxes

        public enum MessageBoxResult
        {
            Positive,
            Negative
        }

        public class MessageBoxResultBase
        {
            public MessageBoxResult DialogResult { get; set; }
        }

        public class MessageBoxEditTextResult : MessageBoxResultBase
        {
            public string Text { get; set; }
        }

        public class MessageBoxSelectOptionResult : MessageBoxResultBase
        {
            public string SelectedKey { get; set; }

            public string SelectedValue { get; set; }
        }

        #endregion
    }
}