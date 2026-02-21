using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;

namespace AstarOneDrive.UI.AccountManagement;

[ExcludeFromCodeCoverage]
public partial class AccountListView : UserControl
{
    public AccountListView()
    {
        InitializeComponent();
    }

    public AccountListViewModel? ViewModel => DataContext as AccountListViewModel;
}
