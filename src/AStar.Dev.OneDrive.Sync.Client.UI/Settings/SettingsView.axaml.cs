using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Settings;

[ExcludeFromCodeCoverage]
public partial class SettingsView : UserControl
{
    public SettingsView() => InitializeComponent();

    protected override async void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if(DataContext is SettingsViewModel viewModel)
        {
            _ = await viewModel.LoadSettingsAsync();
        }
    }
}
