using EFEntityEntryTesting.EF;
using EFEntityEntryTesting.Static;

using System.Collections.ObjectModel;

namespace EFEntityEntryTesting.GUI
{
    internal class GUIDataManager
    {
        public ObservableCollection<Users> Users { get; private set; }
        public ObservableCollection<Currency> Currencies { get; private set; }
        public ObservableCollection<CurrencyType> CurrencyTypes { get; private set; }

        public GUIDataManager()
        {
            // comment this method and build to get XAML designer to display GUI
            SetObsCols();
        }

        internal void SetObsCols()
        {
            ThreadManager.CreateThreadStart(".ctor_GUIDataManager_SetObsCol", () => 
            {
                Users = MainWindow.DataManager.GetUsersObsCol();
                Currencies = MainWindow.DataManager.GetCurrObsCol();
                CurrencyTypes = MainWindow.DataManager.GetCurrTypeObsCol();
            });
        }
    }
}
