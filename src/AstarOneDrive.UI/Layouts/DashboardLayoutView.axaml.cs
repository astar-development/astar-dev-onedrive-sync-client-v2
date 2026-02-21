using System.Diagnostics.CodeAnalysis;
using Avalonia.ReactiveUI;

namespace AstarOneDrive.UI.Layouts;

[ExcludeFromCodeCoverage]
public partial class DashboardLayoutView : ReactiveUserControl<DashboardLayoutViewModel>
{
    public DashboardLayoutView()
    {
        InitializeComponent();
    }
}
