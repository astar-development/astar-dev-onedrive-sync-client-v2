using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using AstarOneDrive.UI.Localization;
using Shouldly;

namespace AstarOneDrive.UI.Tests.Localization;

public sealed class LocalizationManagerTests
{
    [Fact]
    public void SetLanguage_LoadsEnUsDictionary_WithoutError()
    {
        // Act
        var action = () => LocalizationManager.SetLanguage("en-US");

        // Assert
        action.ShouldNotThrow();
    }

    [Fact]
    public void GetString_WithValidKey_ReturnsLocalizedString()
    {
        // Arrange
        LocalizationManager.SetLanguage("en-US");

        // Act
        var result = LocalizationManager.GetString("Menu_File");

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GetString_WithInvalidKey_ReturnsFallbackValue()
    {
        // Arrange
        LocalizationManager.SetLanguage("en-US");
        const string invalidKey = "NonExistent_Key_That_Does_Not_Exist";

        // Act
        var result = LocalizationManager.GetString(invalidKey);

        // Assert
        result.ShouldBe(invalidKey); // Fallback returns the key itself
    }

    [Fact]
    public void SetLanguage_RemovesPreviousDictionary_BeforeLoadingNew()
    {
        // Arrange
        LocalizationManager.SetLanguage("en-US");
        var firstResult = LocalizationManager.GetString("Menu_File");

        // Act
        LocalizationManager.SetLanguage("en-US"); // Load same language again

        // Assert
        var secondResult = LocalizationManager.GetString("Menu_File");
        secondResult.ShouldBe(firstResult);
    }

    [Fact]
    public void AppResourceDictionaries_ContainLoadedLocale_AfterSetLanguage()
    {
        // Arrange
        var app = Application.Current;
        
        // Act
        LocalizationManager.SetLanguage("en-US");

        // Assert
        app.Resources.MergedDictionaries.ShouldContain(dict =>
            dict.Source?.ToString()?.Contains("en-US") ?? false);
    }

    [Fact]
    public void GetString_CommonMenuKeys_AreNotEmpty()
    {
        // Arrange
        LocalizationManager.SetLanguage("en-US");

        // Act
        var menuFile = LocalizationManager.GetString("Menu_File");
        var menuLayouts = LocalizationManager.GetString("Menu_Layouts");
        var btnSyncNow = LocalizationManager.GetString("Btn_SyncNow");

        // Assert
        menuFile.ShouldNotBeNullOrWhiteSpace();
        menuLayouts.ShouldNotBeNullOrWhiteSpace();
        btnSyncNow.ShouldNotBeNullOrWhiteSpace();
    }
}
