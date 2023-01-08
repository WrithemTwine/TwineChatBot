using System.ComponentModel;

namespace SimpleTestFeature
{
    public class TestText: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string LogData { get; set; } = "";

        public void AddLog(string Entry)
        {
            LogData += $"{Entry} \n";
            OnPropertyChanged(nameof(LogData));
        }


        private void OnPropertyChanged(string PropName)
        {
            PropertyChanged?.Invoke(this, new(PropName));
        }
    }
}
