using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;

namespace AStar.Dev.OneDrive.Sync.Client.UI.AccountManagement;

[ExcludeFromCodeCoverage]
public partial class AccountListView : UserControl
{
    public AccountListView() => InitializeComponent();

    public AccountListViewModel? ViewModel => DataContext as AccountListViewModel;
}
