using System.Diagnostics.CodeAnalysis;
using Avalonia.ReactiveUI;

namespace AstarOneDrive.UI.Layouts;

[ExcludeFromCodeCoverage]
public partial class TerminalLayoutView : ReactiveUserControl<TerminalLayoutViewModel>
{
    public TerminalLayoutView()
    {
        InitializeComponent();
    }
}
