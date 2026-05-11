using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace StreamerBotLib.GUI.Helpers
{
    public class InsertionAdorner : Adorner
    {
        private readonly bool _isBelow;
        private readonly Pen _pen;

        public InsertionAdorner(UIElement adornedElement, bool isBelow, SolidColorBrush resourceSource = null)
                : base(adornedElement)
        {
            _isBelow = isBelow;

            // Use your dynamic theme colors
            var glyphBrush = resourceSource ?? Brushes.DodgerBlue;

            _pen = new Pen(glyphBrush, 2.5)
            {
                DashStyle = DashStyles.Dash
            };
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var rect = new Rect(AdornedElement.RenderSize);

            if (_isBelow)
                drawingContext.DrawLine(_pen, rect.BottomLeft, rect.BottomRight);
            else
                drawingContext.DrawLine(_pen, rect.TopLeft, rect.TopRight);
        }
    }
}
