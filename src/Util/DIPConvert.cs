
using Android.Content;
using Android.Util;

namespace Idunas.DanceMusicPlayer.Util
{
    public static class DipConvert
    {
        public static int ToPixel(Context context, float value)
        {
            return (int)TypedValue.ApplyDimension(
                ComplexUnitType.Dip,
                value,
                context.Resources.DisplayMetrics);
        }
    }
}