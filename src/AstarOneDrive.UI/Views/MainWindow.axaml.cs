using Avalonia.ReactiveUI;
using AstarOneDrive.UI.ViewModels;

namespace AstarOneDrive.UI.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
