namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Data.Contracts;

/// <summary>
/// Represents the application settings state for persistence.
/// </summary>
/// <param name="SelectedTheme">The currently selected theme name.</param>
/// <param name="SelectedLanguage">The currently selected language code.</param>
/// <param name="SelectedLayout">The currently selected layout name.</param>
/// <param name="UserName">The username for the application.</param>
public sealed record SettingsState(string SelectedTheme, string SelectedLanguage, string SelectedLayout, string UserName);
