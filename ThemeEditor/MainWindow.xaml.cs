using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ThemeEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private int _r, _g, _b, _H;

        private bool _updatingFromColor;   // re-entrancy guard

        public int H
        {
            get => _H;
            set { _H = value; OnPropertyChanged(); }
        }

        public int R
        {
            get => _r;
            set
            {
                if (_r == value) return;
                _r = value;
                OnPropertyChanged();
                PushRgbToSelected();
            }
        }

        public int G
        {
            get => _g;
            set
            {
                if (_g == value) return;
                _g = value;
                OnPropertyChanged();
                PushRgbToSelected();
            }
        }

        public int B
        {
            get => _b;
            set
            {
                if (_b == value) return;
                _b = value;
                OnPropertyChanged();
                PushRgbToSelected();
            }
        }

        private readonly ThemeFile _themeFiles = new();

        public List<Item> Items { get; } = [];

        private ObservableCollection<ColorElement> _currentColors = new();
        private ColorElement? _selectedColor;

        public ObservableCollection<ColorElement> CurrentColors
        {
            get => _currentColors;
            set { _currentColors = value; OnPropertyChanged(); }
        }

        public ColorElement? SelectedColor
        {
            get => _selectedColor;
            set
            {
                _selectedColor = value;
                CaptureOriginalForUndo();
                PullRgbFromSelected();
                OnPropertyChanged(); OnPropertyChanged(nameof(R));
                OnPropertyChanged(nameof(G)); OnPropertyChanged(nameof(B));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new(name));
        }

        public MainWindow()
        {
            Items.AddRange(Enumerable.Repeat(new Item(), 50));

            InitializeComponent();
            DataContext = this;
            ComboBoxSelectThemeFile.ItemsSource = _themeFiles.ThemeFiles;

            InitializeBitmap();
        }

        private void AdjustThemeResource(string themefilename)
        {
            Resources.MergedDictionaries[0] =
                 new ResourceDictionary
                 {
                     Source = new Uri(ThemeFile.ThemeFilePath(themefilename), UriKind.Absolute)
                 };
        }

        private void PushRgbToSelected()
        {
            if (_updatingFromColor || SelectedColor is null) return;
            SelectedColor.Color = Color.FromRgb((byte)_r, (byte)_g, (byte)_b);
        }

        private void PullRgbFromSelected()
        {
            if (SelectedColor is null) return;

            _updatingFromColor = true;
            try
            {
                var c = SelectedColor.Color;
                _r = c.R; _g = c.G; _b = c.B;
                OnPropertyChanged(nameof(R));
                OnPropertyChanged(nameof(G));
                OnPropertyChanged(nameof(B));

                ColorToHsv(c, out _hue, out double s, out double v);
                H = (int)(_hue * 255);
                _markerPos = new Point(s * 255, (1 - v) * 255);
                RebuildBitmap();   // square matches this hue (blues appear when hue is blue)

                _markerPos = new Point(s * 255, (1 - v) * 255);
                Canvas.SetLeft(PickerMarker, _markerPos.X - PickerMarker.Width / 2);
                Canvas.SetTop(PickerMarker, _markerPos.Y - PickerMarker.Height / 2);
                // no SampleFromBitmap — keeps exact theme hex
            }
            finally { _updatingFromColor = false; }
        }

        #region Bitmap colorpicker

        private double _hue;
        private Point _markerPos = new(128, 128); // last canvas position
        private bool _picking;
        private WriteableBitmap _bitmap;

        public WriteableBitmap bitmap
        {
            get => _bitmap;
            set { _bitmap = value; OnPropertyChanged(); }
        }

        private void InitializeBitmap()
        {
            RebuildBitmap();
        }

        private void ColorImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _picking = true;
            SampleColorAt(e.GetPosition(ColorImage));
            ColorImage.CaptureMouse();
        }

        private void ColorImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (_picking)
            {
                SampleFromBitmap(e.GetPosition(ColorImage));
                //SampleColorAt(e.GetPosition(ColorImage));
                //if (SelectedColor is null || ColorImage.ActualWidth == 0) return;

                //Point p = e.GetPosition(ColorImage);
                //int px = Math.Clamp((int)(p.X * _bitmap.PixelWidth / ColorImage.ActualWidth), 0, _bitmap.PixelWidth - 1);
                //int py = Math.Clamp((int)(p.Y * _bitmap.PixelHeight / ColorImage.ActualHeight), 0, _bitmap.PixelHeight - 1);

                //var pixels = new byte[4];
                //_bitmap.CopyPixels(new Int32Rect(px, py, 1, 1), pixels, 4, 0);

                //Color color = Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
                //SelectedColor.Color = color;

                //_markerPos = new Point(Canvas.GetLeft(PickerMarker), Canvas.GetTop(PickerMarker));
                //SampleFromBitmap(_markerPos);

                //PullRgbFromSelected();
            }
        }

        private void ColorImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _picking = false;
            ColorImage.ReleaseMouseCapture();
        }

        private void SampleColorAt(Point point)
        {
            Canvas.SetTop(PickerMarker, point.Y - PickerMarker.Height / 2);
            Canvas.SetLeft(PickerMarker, point.X - PickerMarker.Width / 2);
        }

        private void SliderBrightnessChange_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _hue = e.NewValue / 255.0;
            RebuildBitmap();
            SampleFromBitmap(_markerPos);
        }

        private void RebuildBitmap()
        {
            int w = 256, h = 256;
            if (bitmap is null)
                bitmap = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null);

            byte[] pixels = new byte[w * h * 4];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    double s = x / 255.0;
                    double v = 1.0 - y / 255.0;          // top = bright
                                                         // optional: v *= H / 255.0;
                    Color c = HsvToColor(_hue, s, v);

                    int i = (y * w + x) * 4;
                    pixels[i] = c.B;
                    pixels[i + 1] = c.G;
                    pixels[i + 2] = c.R;
                    pixels[i + 3] = 255;
                }
            bitmap.WritePixels(new Int32Rect(0, 0, w, h), pixels, w * 4, 0);
            ColorImage.Source = bitmap;
        }

        private void SampleFromBitmap(Point canvasPos)
        {
            if (SelectedColor is null) return;

            double w = ColorImage.ActualWidth;
            double h = ColorImage.ActualHeight;
            if (w <= 0 || h <= 0) return;

            // stay inside the image
            canvasPos = new Point(
                Math.Clamp(canvasPos.X, 0, w),
                Math.Clamp(canvasPos.Y, 0, h));

            int px = Math.Clamp((int)(canvasPos.X * 256 / w), 0, 255);
            int py = Math.Clamp((int)(canvasPos.Y * 256 / h), 0, 255);

            _markerPos = canvasPos;
            double s = px / 255.0;
            double v = 1.0 - py / 255.0;

            Color c = HsvToColor(_hue, s, v);

            _updatingFromColor = true;
            try
            {
                _r = c.R; _g = c.G; _b = c.B;
                OnPropertyChanged(nameof(R));
                OnPropertyChanged(nameof(G));
                OnPropertyChanged(nameof(B));
                SelectedColor.Color = c;
            }
            finally { _updatingFromColor = false; }

            Canvas.SetLeft(PickerMarker, canvasPos.X - PickerMarker.Width / 2);
            Canvas.SetTop(PickerMarker, canvasPos.Y - PickerMarker.Height / 2);
        }

        static void ColorToHsv(Color c, out double h, out double s, out double v)
        {
            double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b));
            v = max;
            double d = max - min;
            s = max == 0 ? 0 : d / max;
            if (d == 0) { h = 0; return; }
            h = max == r ? (g - b) / d + (g < b ? 6 : 0)
              : max == g ? (b - r) / d + 2
              : (r - g) / d + 4;
            h /= 6.0; // 0..1
        }

        static Color HsvToColor(double h, double s, double v)
        {
            h = (h % 1 + 1) % 1;
            int i = (int)(h * 6);
            double f = h * 6 - i;
            double p = v * (1 - s), q = v * (1 - f * s), t = v * (1 - (1 - f) * s);
            double r, g, b;
            switch (i % 6)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                default: r = v; g = p; b = q; break;
            }
            return Color.FromRgb(ToByte(r), ToByte(g), ToByte(b));
        }
        static byte ToByte(double x) => (byte)Math.Clamp((int)(x * 255 + 0.5), 0, 255);

        #endregion

        #region ThemeFile Collection
        private void ComboBoxSelectThemeFile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxSelectThemeFile.SelectedItem is string name)
            {
                CurrentColors = new ObservableCollection<ColorElement>(_themeFiles.OpenThemeFile(name));
                SelectedColor = CurrentColors.FirstOrDefault();
                AdjustThemeResource(name);
            }
        }

        private void Button_SelectedThemeSave_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxSelectThemeFile.SelectedItem is string name)
            {
                _themeFiles.SaveThemeFile(name, [.. _currentColors]);
                AdjustThemeResource(name);
            }
        }

        #endregion

        #region UndoColor

        private Color? _originalColor;

        private void CaptureOriginalForUndo()
        {
            _originalColor = SelectedColor?.Color;
        }

        private void Button_UndoColor_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedColor is null || _originalColor is null) return;
            SelectedColor.Color = _originalColor.Value;
            PullRgbFromSelected();
        }

        #endregion
    }
}