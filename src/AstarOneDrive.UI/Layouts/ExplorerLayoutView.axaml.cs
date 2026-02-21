using System.Diagnostics.CodeAnalysis;
using Avalonia.ReactiveUI;

namespace AstarOneDrive.UI.Layouts;

[ExcludeFromCodeCoverage]
public partial class ExplorerLayoutView : ReactiveUserControl<ExplorerLayoutViewModel>
{
    public ExplorerLayoutView()
    {
        InitializeComponent();
    }
}
