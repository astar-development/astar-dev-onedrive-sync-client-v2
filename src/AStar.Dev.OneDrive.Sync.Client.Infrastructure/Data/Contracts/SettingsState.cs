namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Contracts;

public sealed record SettingsState(
    string SelectedTheme,
    string SelectedLanguage,
    string SelectedLayout,
    string UserName);
