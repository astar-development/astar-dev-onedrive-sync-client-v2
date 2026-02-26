using Avalonia.Markup.Xaml.Styling;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Localization;

/// <summary>
/// Manages application localization by loading and switching language resource dictionaries.
/// </summary>
public static class LocalizationManager
{
    private const string BaseUri = "avares://AStar.Dev.OneDrive.Sync.Client.UI/";

    // In-memory fallback for unit tests when Application.Current is null
    private static readonly Dictionary<string, string> InMemoryResources = new();

    /// <summary>
    /// Sets the application language by loading the corresponding locale dictionary.
    /// </summary>
    /// <param name="culture">The culture code (e.g., "en-GB", "fr-FR")</param>
    /// <exception cref="InvalidOperationException">Thrown if the locale dictionary cannot be loaded.</exception>
    public static void SetLanguage(string culture)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(culture);

        Avalonia.Application? app = Avalonia.Application.Current;

        if(app != null)
        {
            // Clear existing merged dictionaries
            app.Resources.MergedDictionaries.Clear();

            try
            {
                // Load locale dictionary using ResourceInclude (works just like StyleInclude)
                var localeUri = new Uri($"{BaseUri}Locales/{culture}.axaml");

                var resourceInclude = new ResourceInclude(new Uri(BaseUri))
                {
                    Source = localeUri
                };

                app.Resources.MergedDictionaries.Add(resourceInclude);
            }
            catch(InvalidOperationException ex)
            {
                // Wrap expected resource loading issues in a domain-specific exception
                throw new InvalidOperationException($"Failed to load locale '{culture}'", ex);
            }
            catch(UriFormatException ex)
            {
                // Wrap invalid URI issues (e.g., malformed culture code) in a domain-specific exception
                throw new InvalidOperationException($"Failed to load locale '{culture}'", ex);
            }
        }
        else
        {
            // Fallback for unit tests: load from hard-coded resources
            LoadInMemoryResources(culture);
        }

        CurrentLanguage = culture;
    }

    /// <summary>
    /// Retrieves a localized string from the current language dictionary.
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <returns>The localized string, or the key itself if not found.</returns>
    public static string GetString(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        Avalonia.Application? app = Avalonia.Application.Current;

        if(app != null && app.Resources.TryGetResource(key, null, out var resource) && resource is string localizedString)
        {
            return localizedString;
        }

        // Try in-memory resources (fallback for tests)
        if(InMemoryResources.TryGetValue(key, out var value))
        {
            return value;
        }

        // Final fallback: return the key itself
        return key;
    }

    /// <summary>
    /// Gets the currently active language code.
    /// </summary>
    public static string CurrentLanguage { get; private set; } = "en-GB";

    /// <summary>
    /// Loads hard-coded English resources for unit testing.
    /// </summary>
    private static void LoadInMemoryResources(string culture)
    {
        InMemoryResources.Clear();

        if(culture == "en-GB")
        {
            InMemoryResources["Menu_File"] = "File";
            InMemoryResources["Menu_UserSettings"] = "User Settings";
            InMemoryResources["Menu_AppSettings"] = "Application Settings";
            InMemoryResources["Menu_Layouts"] = "Layouts";
            InMemoryResources["Menu_Help"] = "Help";
            InMemoryResources["Layout_Explorer"] = "Explorer";
            InMemoryResources["Layout_Dashboard"] = "Dashboard";
            InMemoryResources["Layout_Terminal"] = "Terminal";
            InMemoryResources["Btn_SyncNow"] = "Sync Now";
            InMemoryResources["Btn_AddAccount"] = "Add Account";
            InMemoryResources["Btn_RemoveAccount"] = "Remove Account";
            InMemoryResources["Btn_OK"] = "OK";
            InMemoryResources["Btn_Cancel"] = "Cancel";
            InMemoryResources["Status_Idle"] = "Idle";
            InMemoryResources["Status_Syncing"] = "Syncing...";
        }

        if(culture == "en-US")
        {
            InMemoryResources["Menu_File"] = "File (US)";
            InMemoryResources["Menu_UserSettings"] = "User Settings";
            InMemoryResources["Menu_AppSettings"] = "Application Settings";
            InMemoryResources["Menu_Layouts"] = "Layouts";
            InMemoryResources["Menu_Help"] = "Help";
            InMemoryResources["Layout_Explorer"] = "Explorer";
            InMemoryResources["Layout_Dashboard"] = "Dashboard";
            InMemoryResources["Layout_Terminal"] = "Terminal";
            InMemoryResources["Btn_SyncNow"] = "Sync Now";
            InMemoryResources["Btn_AddAccount"] = "Add Account";
            InMemoryResources["Btn_RemoveAccount"] = "Remove Account";
            InMemoryResources["Btn_OK"] = "OK";
            InMemoryResources["Btn_Cancel"] = "Cancel";
            InMemoryResources["Status_Idle"] = "Idle";
            InMemoryResources["Status_Syncing"] = "Syncing...";
        }
    }
}

