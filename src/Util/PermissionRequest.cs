using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;

namespace Idunas.DanceMusicPlayer.Util
{
    public class PermissionRequest
    {
        public static PermissionRequest Storage { get; } = new PermissionRequest(Android.Manifest.Permission.WriteExternalStorage);


        private readonly string _permission;

        public PermissionRequest(string permission)
        {
            _permission = permission;
        }

        public bool Request(Activity activity, int rationaleResourceId, int requestCode)
        {
            if (ContextCompat.CheckSelfPermission(activity, _permission) == Permission.Granted)
            {
                return true;
            }

            if (ActivityCompat.ShouldShowRequestPermissionRationale(activity, _permission))
            {
                // Show rationale
                MessageBox
                    .Build(activity)
                    .SetTitle(Resource.String.permission_required)
                    .SetMessage(rationaleResourceId)
                    .SetNegativeAction(Resource.String.cancel)
                    .SetPositiveAction(Resource.String.ok, () => RequestPermissionsInternal(activity, requestCode))
                    .Show();
                return false;
            }

            // Request permission
            RequestPermissionsInternal(activity, requestCode);
            return false;
        }

        public static bool WasGranted(Permission[] grantResults)
        {
            return grantResults.Length > 0 && grantResults[0] == Permission.Granted;
        }

        private void RequestPermissionsInternal(Activity activity, int requestCode)
        {
            ActivityCompat.RequestPermissions(
                    activity,
                    new[] { _permission },
                    requestCode);
        }
    }
}