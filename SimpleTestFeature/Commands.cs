using System.Collections.ObjectModel;

namespace SimpleTestFeature
{
    public class Commands
    {
        public ObservableCollection<DetailUserCommand> UserCommandList { get; set; } = 
            [new() { Command="lurk", Description="User is now lurking."},
             new() { Command="time", Description="Now showing the current time."}
            ];
    }
}
