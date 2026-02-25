using System.Windows.Input;
using AStar.Dev.OneDrive.Sync.Client.UI.Common;
using AStar.Dev.OneDrive.Sync.Client.UI.Home;
using ReactiveUI;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Layouts;

public class TerminalLayoutViewModel : ViewModelBase
{
    public MainWindowViewModel MainWindow { get; }

    public string TerminalStatus
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "Ready";

    public ICommand RunHealthCheckCommand { get; }

    public TerminalLayoutViewModel(MainWindowViewModel mainWindow)
    {
        MainWindow = mainWindow;
        RunHealthCheckCommand = new RelayCommand(_ => TerminalStatus = "Connected");
    }
}
