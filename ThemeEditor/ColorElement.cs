using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace ThemeEditor
{
    public class ColorElement : INotifyPropertyChanged
    {
        private Color _color;

        public ColorElement(string key, Color color)
        {
            Key = key;
            Color = color;
        }

        public string Key { get; set; }

        public Color Color
        {
            get => _color;
            set
            {
                if (_color == value) return;
                _color = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Hex));
            }
        }
        public string Hex
        {
            get => $"#{_color.R:X2}{_color.G:X2}{_color.B:X2}";
            set
            {
                try
                {
                    Color = (Color)ColorConverter.ConvertFromString(value);
                }
                catch { /* ignore bad input while typing */ }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
