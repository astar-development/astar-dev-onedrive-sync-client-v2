using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;

namespace AStar.Dev.OneDrive.Sync.Client.UI.AccountManagement;

[ExcludeFromCodeCoverage]
public partial class AccountDialogView : Window
{
    private AccountDialogViewModel? _viewModel;

    public AccountDialogView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if(_viewModel is not null)
        {
            _viewModel.CloseRequested -= OnCloseRequested;
        }

        _viewModel = DataContext as AccountDialogViewModel;
        if(_viewModel is not null)
        {
            _viewModel.CloseRequested += OnCloseRequested;
        }
    }

    private void OnCloseRequested(object? sender, bool saved)
    {
        if(saved)
        {
            Close(true);
            return;
        }

        Close(false);
    }

    protected override void OnClosed(EventArgs e)
    {
        if(_viewModel is not null)
        {
            _viewModel.CloseRequested -= OnCloseRequested;
        }

        base.OnClosed(e);
    }
}