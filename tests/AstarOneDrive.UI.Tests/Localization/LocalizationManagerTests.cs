using AstarOneDrive.UI.Localization;
using Shouldly;

namespace AstarOneDrive.UI.Tests.Localization;

public sealed class LocalizationManagerTests
{
    [Fact]
    public void SetLanguage_WithEnGb_DoesNotThrow()
    {
        // Act & Assert
        var action = () => LocalizationManager.SetLanguage("en-GB");
        action.ShouldNotThrow();
    }

    [Fact]
    public void GetString_WithValidKey_ReturnsNonEmptyString()
    {
        // Arrange
        LocalizationManager.SetLanguage("en-GB");

        // Act
        var result = LocalizationManager.GetString("Menu_File");

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();
        result.ShouldNotBe("Menu_File"); // Should be actual value, not fallback
    }

    [Fact]
    public void GetString_WithInvalidKey_ReturnsFallbackValue()
    {
        // Arrange
        LocalizationManager.SetLanguage("en-GB");
        const string invalidKey = "NonExistent_Key_12345";

        // Act
        var result = LocalizationManager.GetString(invalidKey);

        // Assert
        result.ShouldBe(invalidKey);
    }

    [Fact]
    public void SetLanguage_MultipleCallsWithSameCulture_DoesNotThrow()
    {
        // Act - call multiple times
        LocalizationManager.SetLanguage("en-GB");
        var action = () => LocalizationManager.SetLanguage("en-GB");

        // Assert
        action.ShouldNotThrow();
    }

    [Fact]
    public void CurrentLanguage_ReflecsSetLanguage()
    {
        // Act
        LocalizationManager.SetLanguage("en-GB");

        // Assert
        LocalizationManager.CurrentLanguage.ShouldBe("en-GB");
    }

    [Fact]
    public void GetString_AfterSetLanguage_ReturnsConsistentValues()
    {
        // Arrange
        LocalizationManager.SetLanguage("en-GB");

        // Act
        var result1 = LocalizationManager.GetString("Menu_File");
        var result2 = LocalizationManager.GetString("Menu_File");

        // Assert
        result1.ShouldBe(result2);
        result1.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GetString_MultipleKeys_ReturnDifferentValues()
    {
        // Arrange
        LocalizationManager.SetLanguage("en-GB");

        // Act
        var menuFile = LocalizationManager.GetString("Menu_File");
        var menuLayouts = LocalizationManager.GetString("Menu_Layouts");
        var btnSync = LocalizationManager.GetString("Btn_SyncNow");

        // Assert
        menuFile.ShouldNotBe(menuLayouts);
        menuLayouts.ShouldNotBe(btnSync);
        menuFile.ShouldNotBeNullOrWhiteSpace();
    }
}
