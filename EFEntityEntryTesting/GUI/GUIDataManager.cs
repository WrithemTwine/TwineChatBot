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

        public GUIDataManager()
        {
            // comment this SetObsCols() method and build to get XAML designer to display GUI (reload designer)
            SetObsCols();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        internal void SetObsCols()
        {
            ThreadManager.CreateThreadStart(".ctor_GUIDataManager_SetObsCol", () =>
            {
                Users = MainWindow.DataManager.GetUsersObsCol();
                Currency = MainWindow.DataManager.GetCurrObsCol();
                CurrencyTypes = MainWindow.DataManager.GetCurrTypeObsCol();
            });
        }

        public void NotifyChange(string table)
        {
            PropertyChanged?.Invoke(this, new(table));
        }
    }
}
