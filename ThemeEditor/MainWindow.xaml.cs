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
        private readonly ThemeFile _themeFiles = new();

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
            set { _selectedColor = value; OnPropertyChanged(); }
        }

        private WriteableBitmap _bitmap;

        public WriteableBitmap bitmap
        {
            get => _bitmap;
            set { _bitmap = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new(name));
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            ComboBoxSelectThemeFile.ItemsSource = _themeFiles.ThemeFiles;

            // Create a gradient spectrum programmatically
            int width = 256;
            int height = 256;
            bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

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

        private void AdjustThemeResource(string themefilename)
        {
            Resources.MergedDictionaries[0] =
                 new ResourceDictionary
                 {
                     Source = new Uri(ThemeFile.ThemeFilePath(themefilename), UriKind.Absolute)
                 };
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
        }

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
            }
        }

        #endregion

    }
}