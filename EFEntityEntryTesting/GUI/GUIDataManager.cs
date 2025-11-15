using EFEntityEntryTesting.EF;
using EFEntityEntryTesting.Static;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace EFEntityEntryTesting.GUI
{
    internal class GUIDataManager : INotifyPropertyChanged
    {
        public ObservableCollection<Users> Users { get; private set; }
        public ObservableCollection<Currency> Currency { get; private set; }
        public ObservableCollection<CurrencyType> CurrencyTypes { get; private set; }

        public GUIDataManager() { }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        internal void SetObsCols()
        {
            ThreadManager.CreateThreadStart(".ctor_GUIDataManager_SetObsCol", () =>
            {
                Users = MainWindow.DataManager.GetUsersObsCol();
                Currency = MainWindow.DataManager.GetCurrObsCol();
                CurrencyTypes = MainWindow.DataManager.GetCurrTypeObsCol();

                PropertyChanged?.Invoke(this, new(nameof(Users)));
                PropertyChanged?.Invoke(this, new(nameof(Currency)));
                PropertyChanged?.Invoke(this, new(nameof(CurrencyTypes)));
            });
        }

        public void NotifyChange(string table)
        {
            PropertyChanged?.Invoke(this, new(table));
        }
    }
}
