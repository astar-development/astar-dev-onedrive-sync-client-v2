using AStar.Dev.OneDrive.Sync.Client.UI.Localization;

namespace AStar.Dev.OneDrive.Sync.Client.UI.Tests.Localization;

public sealed class LocalizationManagerShould
{
    [Fact]
    public void SetLanguageToEnGbAndNotThrow()
    {
        Action action = () => LocalizationManager.SetLanguage("en-GB");

        action.ShouldNotThrow();
    }

    [Fact]
    public void ReturnNonEmptyStringWhenGetStringWithValidKey()
    {
        LocalizationManager.SetLanguage("en-GB");

        var result = LocalizationManager.GetString("Menu_File");

        result.ShouldNotBeNullOrWhiteSpace();
        result.ShouldNotBe("Menu_File"); 
    }

    [Fact]
    public void ReturnFallbackValueWhenGetStringWithInvalidKey()
    {
        LocalizationManager.SetLanguage("en-GB");
        const string invalidKey = "NonExistent_Key_12345";

        var result = LocalizationManager.GetString(invalidKey);

        result.ShouldBe(invalidKey);
    }

    [Fact]
    public void NotThrowWhenSetLanguageIsCalledMultipleTimesWithSameCulture()
    {
        LocalizationManager.SetLanguage("en-GB");
        
        Action action = () => LocalizationManager.SetLanguage("en-GB");

        action.ShouldNotThrow();
    }

    [Fact]
    public void ReflectSetLanguageInCurrentLanguage()
    {
        LocalizationManager.SetLanguage("en-GB");

        LocalizationManager.CurrentLanguage.ShouldBe("en-GB");
    }

    [Fact]
    public void ReturnConsistentValuesWhenGetStringIsCalledAfterSetLanguage()
    {
        LocalizationManager.SetLanguage("en-GB");

        var result1 = LocalizationManager.GetString("Menu_File");
        var result2 = LocalizationManager.GetString("Menu_File");

        result1.ShouldBe(result2);
        result1.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ReturnDifferentValuesWhenGetStringIsCalledWithMultipleKeys()
    {
        LocalizationManager.SetLanguage("en-GB");

        var menuFile = LocalizationManager.GetString("Menu_File");
        var menuLayouts = LocalizationManager.GetString("Menu_Layouts");
        var btnSync = LocalizationManager.GetString("Btn_SyncNow");

        menuFile.ShouldNotBe(menuLayouts);
        menuLayouts.ShouldNotBe(btnSync);
        menuFile.ShouldNotBeNullOrWhiteSpace();
    }
}
