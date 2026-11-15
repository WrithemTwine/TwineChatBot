using System.ComponentModel;

namespace StreamerBotLib.Models.Statistics
{
    public class AppStat : INotifyPropertyChanged
    {
        public string Name { get; set; }

        private object _value;
        public object Value
        {
            get => _value;
            private set
            {
                if (Equals(_value, value)) return;
                _value = value;
                PropertyChanged?.Invoke(this, new(nameof(Value)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void UpdateValue(object newValue) => Value = newValue;
    }
}
