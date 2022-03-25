using System.ComponentModel;

namespace StreamerBotLib.Models
{
    public class AppStat<T> : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public T Value { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void UpdateValue(T NewValue)
        {
            Value = NewValue;

            PropertyChanged?.Invoke(this, new(nameof(Value)));
        }
    }
}
