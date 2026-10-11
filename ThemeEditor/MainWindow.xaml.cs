using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ThemeEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WriteableBitmap _bitmap;

        public MainWindow()
        {
            InitializeComponent();
            UpdateColorPreview();

            // Create a gradient spectrum programmatically
            int width = 256;
            int height = 256;
            _bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            byte[] pixels = new byte[width * height * 4];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte r = (byte)x;       // Horizontal axis = Red
                    byte g = (byte)y;       // Vertical axis = Green
                    byte b = (byte)(255 - x); // Blue decreases horizontally

                    int index = (y * width + x) * 4;
                    pixels[index + 0] = b; // Blue
                    pixels[index + 1] = g; // Green
                    pixels[index + 2] = r; // Red
                    pixels[index + 3] = 255; // Alpha
                }
            }
        }

        private void Slider_ValueChanged(object sender, RoutedEventArgs e)
        {
            UpdateColorPreview();
        }

        private void UpdateColorPreview()
        {
            // Get slider values
            byte r = (byte)SliderR.Value;
            byte g = (byte)SliderG.Value;
            byte b = (byte)SliderB.Value;

            // Update text blocks
            TextR.Text = r.ToString();
            TextG.Text = g.ToString();
            TextB.Text = b.ToString();

            // Update preview rectangle
            ColorPreview.Fill = new SolidColorBrush(Color.FromRgb(r, g, b));
        }

        private void ColorImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(ColorImage);

            // Scale click position to bitmap coordinates
            double scaleX = _bitmap.PixelWidth / ColorImage.ActualWidth;
            double scaleY = _bitmap.PixelHeight / ColorImage.ActualHeight;

            int px = (int)(p.X * scaleX);
            int py = (int)(p.Y * scaleY);

            // Read pixel color
            var pixels = new byte[4];
            _bitmap.CopyPixels(new Int32Rect(px, py, 1, 1), pixels, 4, 0);

            Color color = Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);

            // Update preview
            PreviewRect.Fill = new SolidColorBrush(color);
            ColorInfo.Text = $"RGB: {color.R}, {color.G}, {color.B}";
        }
    }
}