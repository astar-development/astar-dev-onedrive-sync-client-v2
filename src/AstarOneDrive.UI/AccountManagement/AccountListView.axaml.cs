using Avalonia.Controls;

namespace AstarOneDrive.UI.AccountManagement;

public partial class AccountListView : UserControl
{
    public AccountListView()
    {
        InitializeComponent();
    }

    public AccountListViewModel? ViewModel => DataContext as AccountListViewModel;
}
