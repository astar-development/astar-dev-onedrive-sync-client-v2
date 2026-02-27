using Avalonia.Markup.Xaml.Styling;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Localization;

/// <summary>
/// Manages application localization by loading and switching language resource dictionaries.
/// </summary>
public static class LocalizationManager
{
    private readonly static string BaseUri = $"{ApplicationMetadata.AvaresPrefix}://{ApplicationMetadata.UiProject}/";

    private static readonly Dictionary<string, string> InMemoryResources = [];

    /// <summary>
    /// Sets the application language by loading the corresponding locale dictionary.
    /// </summary>
    /// <param name="culture">The culture code (e.g., "en-GB", "fr-FR")</param>
    /// <exception cref="InvalidOperationException">Thrown if the locale dictionary cannot be loaded.</exception>
    public static void SetLanguage(string culture)
    {
        Avalonia.Application? app = Avalonia.Application.Current;

        if(app != null)
        {
            app.Resources.MergedDictionaries.Clear();

            try
            {
                var localeUri = new Uri($"{BaseUri}Locales/{culture}.axaml");

                var resourceInclude = new ResourceInclude(new Uri(BaseUri))
                {
                    Source = localeUri
                };

                app.Resources.MergedDictionaries.Add(resourceInclude);
            }
            catch(InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Failed to load locale '{culture}'", ex);
            }
            catch(UriFormatException ex)
            {
                throw new InvalidOperationException($"Failed to load locale '{culture}' - Format error", ex);
            }
        }
        else
        {
            FallbackToLoadingFromInMemoryResources(culture);
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
        (var found, var localizedString) = HaveLocalisedString(key, app);

        return found ? localizedString : Fallback(key);
    }

    /// <summary>
    /// Gets the currently active language code.
    /// </summary>
    public static string CurrentLanguage { get; private set; } = "en-GB";

    private static void FallbackToLoadingFromInMemoryResources(string culture)
    {
        InMemoryResources.Clear();

        if(culture == "en-GB")
        {
            DefineGBCultureMessages();
        }

        if(culture == "en-US")
        {
            DefineUSCultureMessages();
        }
    }

    private static void DefineGBCultureMessages()
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
        InMemoryResources["Btn_Apply"] = "Apply";
        InMemoryResources["Status_Idle"] = "Idle";
        InMemoryResources["Status_Syncing"] = "Syncing...";
        InMemoryResources["Settings_Title"] = "Settings";
        InMemoryResources["Settings_User"] = "User";
        InMemoryResources["Settings_UserName"] = "User Name";
        InMemoryResources["Settings_App"] = "Application";
        InMemoryResources["Settings_Theme"] = "Theme";
        InMemoryResources["Settings_Language"] = "Language";
        InMemoryResources["Settings_Layout"] = "Layout";
    }

    private static void DefineUSCultureMessages()
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
        InMemoryResources["Btn_Apply"] = "Apply";
        InMemoryResources["Status_Idle"] = "Idle";
        InMemoryResources["Status_Syncing"] = "Syncing...";
        InMemoryResources["Settings_Title"] = "Settings";
        InMemoryResources["Settings_User"] = "User";
        InMemoryResources["Settings_UserName"] = "User Name";
        InMemoryResources["Settings_App"] = "Application";
        InMemoryResources["Settings_Theme"] = "Theme";
        InMemoryResources["Settings_Language"] = "Language";
        InMemoryResources["Settings_Layout"] = "Layout";
    }

    private static (bool, string) HaveLocalisedString(string key, Avalonia.Application? app)
    {
        if (app != null && app.Resources.TryGetResource(key, null, out var resource) && resource is string localizedString)
        {
            return (true, localizedString);
        }

        return (false, string.Empty);
    }

    private static string Fallback(string key) => InMemoryResources.TryGetValue(key, out var value) ? value : key;
}

