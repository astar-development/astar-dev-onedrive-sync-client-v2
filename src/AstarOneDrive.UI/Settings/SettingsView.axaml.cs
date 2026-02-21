using System.Diagnostics.CodeAnalysis;
using Avalonia.ReactiveUI;

namespace AstarOneDrive.UI.Settings;

[ExcludeFromCodeCoverage]
public partial class SettingsView : ReactiveUserControl<SettingsViewModel>
{
    public SettingsView()
    {
        InitializeComponent();
    }
}
