using System.Windows;

namespace StreamerBot;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static Version Version { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        // Get the version of the application
        Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        base.OnStartup(e);
    }
}

