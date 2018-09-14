using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Idunas.DanceMusicPlayer.Util;
using System;

namespace Idunas.DanceMusicPlayer.Fragments.Player
{
    public class LoopPositionIndicatorView : View
    {
        private const int BUTTON_TOP_BAR_WIDTH = 30;

        private Paint _solidPaint;
        private Paint _dashedPaint;

        private Color _color;
        private int? _loopMarkerStart;
        private int? _loopMarkerEnd;

        #region --- Public properties

        public ImageButton ButtonStartLoopMarker { get; set; }

        public ImageButton ButtonEndLoopMarker { get; set; }

        public SeekBar SeekBarPosition { get; set; }

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                _solidPaint.Color = value;
                _dashedPaint.Color = value;
            }
        }

        public int? LoopMarkerStart
        {
            get { return _loopMarkerStart; }
            set
            {
                _loopMarkerStart = value;
                Invalidate();
            }
        }

        public int? LoopMarkerEnd
        {
            get { return _loopMarkerEnd; }
            set
            {
                _loopMarkerEnd = value;
                Invalidate();
            }
        }

        #endregion

        #region --- Constructors

        public LoopPositionIndicatorView(Context context) : base(context)
        {
            Initialize();
        }

        public LoopPositionIndicatorView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        public LoopPositionIndicatorView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Initialize();
        }

        public LoopPositionIndicatorView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Initialize();
        }

        protected LoopPositionIndicatorView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            Initialize();
        }

        private void Initialize()
        {
            _solidPaint = new Paint(PaintFlags.AntiAlias)
            {
                StrokeWidth = 5
            };

            _dashedPaint = new Paint(PaintFlags.AntiAlias)
            {
                StrokeWidth = 3,
            };
            _dashedPaint.SetStyle(Paint.Style.Stroke);
            _dashedPaint.SetPathEffect(new DashPathEffect(new[] { 5f, 10f }, 0));
        }

        #endregion

        protected override void OnDraw(Canvas canvas)
        {
            if (ButtonStartLoopMarker == null || ButtonEndLoopMarker == null || SeekBarPosition == null)
            {
                return;
            }

            if (LoopMarkerStart == null && LoopMarkerEnd == null)
            {
                // Nothing to draw if no marker is set
                return;
            }

            // Offset between the screen and the view we draw on
            var offset = GetLocation(this);

            /* We want to draw something like this :)
             *   _________________________________
             *   |_______________________________|
             *     |  |__________________
             *     |_______             |
             *          __|__         __|__
             * */


            // We start with the actual positions of the seekbar and the buttons
            var seekBarPositionLocation = GetLocation(SeekBarPosition);
            var buttonStartLocation = GetLocation(ButtonStartLoopMarker);
            var buttonEndLocation = GetLocation(ButtonEndLoopMarker);

            // We need to know the Y value where our seekbar ends
            var ySeekbarBottom = seekBarPositionLocation.Y + SeekBarPosition.Height;

            // Get the space between our seekbar and our buttons and split it into 3 equal parts
            var availableHeight = buttonStartLocation.Y - ySeekbarBottom;
            var heightPerPart = availableHeight / 3f;

            // Get the available width for the seekbar and create a factor regarding the maximum value of the seekbar
            var widthPerPosition = (SeekBarPosition.Width - SeekBarPosition.PaddingLeft - SeekBarPosition.PaddingRight) / (float)SeekBarPosition.Max;

            // Define start values
            var xStartButtonCenter = buttonStartLocation.X + (ButtonStartLoopMarker.Width / 2f);
            var xStartSeekbarPosition = 0f;

            // Define end values
            var xEndSeekbarPosition = 0f;

            // Draw the connector for the start marker
            if (LoopMarkerStart != null)
            {
                xStartSeekbarPosition = seekBarPositionLocation.X + SeekBarPosition.PaddingLeft + (LoopMarkerStart.Value * widthPerPosition);
                var yStartHorizontalConnector = buttonStartLocation.Y - heightPerPart;

                DrawLoopMarkerConnector(
                    canvas,
                    offset,
                    xStartSeekbarPosition,
                    xStartButtonCenter,
                    ySeekbarBottom,
                    yStartHorizontalConnector,
                    buttonStartLocation.Y);
            }

            // Draw the connector for the end marker
            if (LoopMarkerEnd != null)
            {
                xEndSeekbarPosition = seekBarPositionLocation.X + SeekBarPosition.PaddingLeft + (LoopMarkerEnd.Value * widthPerPosition);
                var xEndButtonCenter = buttonEndLocation.X + (ButtonEndLoopMarker.Width / 2f);
                var yEndHorizontalConnector = xEndSeekbarPosition < xStartButtonCenter + 5
                    ? buttonEndLocation.Y - (2 * heightPerPart)
                    : buttonEndLocation.Y - heightPerPart;

                DrawLoopMarkerConnector(
                    canvas,
                    offset,
                    xEndSeekbarPosition,
                    xEndButtonCenter,
                    ySeekbarBottom,
                    yEndHorizontalConnector,
                    buttonEndLocation.Y);
            }

            // Draw the dashed indicator between our connector lines
            var xIndicatorStart = LoopMarkerStart != null
                ? xStartSeekbarPosition + 10
                : seekBarPositionLocation.X + SeekBarPosition.PaddingLeft;

            var xIndicatorEnd = LoopMarkerEnd != null
                ? xEndSeekbarPosition - 10
                : seekBarPositionLocation.X + SeekBarPosition.Width - SeekBarPosition.PaddingRight;

            var indicator = new Path();
            indicator.MoveTo(xIndicatorStart + offset.X, ySeekbarBottom - offset.Y);
            indicator.LineTo(xIndicatorEnd + offset.X, ySeekbarBottom - offset.Y);
            canvas.DrawPath(indicator, _dashedPaint);

            //canvas.DrawLine(
            //    xIndicatorStart + offset.X, 
            //    ySeekbarBottom - offset.Y, 
            //    xIndicatorEnd + offset.X, 
            //    ySeekbarBottom - offset.Y,
            //    _dashedPaint);
        }

        private void DrawLoopMarkerConnector(
            Canvas canvas,
            Location offset,
            float xSeekbar,
            float xButtonCenter,
            int ySeekbar,
            float yHorizontalConnector,
            int yButtonTop)
        {
            // Apply the offset to all our values
            xSeekbar += offset.X;
            xButtonCenter += offset.X;
            ySeekbar -= offset.Y;
            yHorizontalConnector -= offset.Y;
            yButtonTop -= offset.Y;

            // We start drawing lines from the seekbar (top) to the button (bottom)
            var lines = new float[] {
                xSeekbar, ySeekbar, xSeekbar, yHorizontalConnector,
                xSeekbar, yHorizontalConnector, xButtonCenter, yHorizontalConnector,
                xButtonCenter, yHorizontalConnector, xButtonCenter, yButtonTop,
                /*xButtonCenter - BUTTON_TOP_BAR_WIDTH, yButtonTop, xButtonCenter + BUTTON_TOP_BAR_WIDTH, yButtonTop */};

            canvas.DrawLines(lines, _solidPaint);
        }

        private Location GetLocation(View view)
        {
            var location = new int[2];
            view.GetLocationInWindow(location);
            return new Location(location);
        }
    }
}