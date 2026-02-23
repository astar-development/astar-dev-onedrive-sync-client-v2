using System.Windows.Input;
using AstarOneDrive.UI.Common;
using AstarOneDrive.UI.Home;
using ReactiveUI;


namespace AstarOneDrive.UI.Layouts;

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
