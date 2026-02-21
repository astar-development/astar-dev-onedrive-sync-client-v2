using System.Diagnostics.CodeAnalysis;
using Avalonia.ReactiveUI;

namespace AstarOneDrive.UI.Home;

[ExcludeFromCodeCoverage]
public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
