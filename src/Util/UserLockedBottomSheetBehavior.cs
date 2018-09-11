using Android.Content;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Views;

namespace Idunas.DanceMusicPlayer.Util
{
    public class UserLockedBottomSheetBehavior : BottomSheetBehavior
    {
        public UserLockedBottomSheetBehavior()
        {
        }

        public UserLockedBottomSheetBehavior(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public override bool OnInterceptTouchEvent(CoordinatorLayout parent, Java.Lang.Object child, MotionEvent ev)
        {
            return false;
        }

        public override bool OnTouchEvent(CoordinatorLayout parent, Java.Lang.Object child, MotionEvent ev)
        {
            return false;
        }

        public override bool OnStartNestedScroll(CoordinatorLayout coordinatorLayout, Java.Lang.Object child, View directTargetChild, View target, int axes, int type)
        {
            return false;
        }

        public override void OnNestedPreScroll(CoordinatorLayout coordinatorLayout, Java.Lang.Object child, View target, int dx, int dy, int[] consumed, int type)
        {
            // Do nothing
        }

        public override void OnStopNestedScroll(CoordinatorLayout coordinatorLayout, Java.Lang.Object child, View target, int type)
        {
            // Do nothing
        }

        public override bool OnNestedPreFling(CoordinatorLayout coordinatorLayout, Java.Lang.Object child, View target, float velocityX, float velocityY)
        {
            return false;
        }
    }
}