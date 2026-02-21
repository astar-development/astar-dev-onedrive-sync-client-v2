using System.Diagnostics.CodeAnalysis;
using Avalonia.ReactiveUI;

namespace AstarOneDrive.UI.SyncStatus;

[ExcludeFromCodeCoverage]
public partial class SyncStatusView : ReactiveUserControl<SyncStatusViewModel>
{
    public SyncStatusView()
    {
        InitializeComponent();
    }
}
