// 📁 Plik: Services/ThemeService.cs

using Windows.Storage;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace PtasieRadio.Services;

public static class ThemeService
{
    private const string ThemeKey = "AppTheme";

    public static Task SaveThemeAsync(string themeName)
    {
        ApplicationData.Current.LocalSettings.Values[ThemeKey] = themeName;
        return Task.CompletedTask;
    }

    public static string? LoadTheme()
    {
        return ApplicationData.Current.LocalSettings.Values[ThemeKey] as string;
    }

    public static void ApplyTheme(string? theme)
    {
        if (string.IsNullOrWhiteSpace(theme)) return;

        var res = Application.Current.Resources;

        switch (theme)
        {
            case "Jasny":
                res["PrimaryColor"] = new SolidColorBrush(Microsoft.UI.Colors.White);
                res["SecondaryColor"] = new SolidColorBrush(Microsoft.UI.Colors.Black);
                break;
            case "Ciemny":
                res["PrimaryColor"] = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 34, 34, 34));
                res["SecondaryColor"] = new SolidColorBrush(Microsoft.UI.Colors.White);
                break;
            case "CherryDark":
                res["PrimaryColor"] = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 76, 0, 0));
                res["SecondaryColor"] = new SolidColorBrush(Microsoft.UI.Colors.White);
                break;
            case "CherryLight":
                res["PrimaryColor"] = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 192, 117, 117));
                res["SecondaryColor"] = new SolidColorBrush(Microsoft.UI.Colors.White);
                break;
            case "Graphite":
                res["PrimaryColor"] = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 26, 27, 42));
                res["SecondaryColor"] = new SolidColorBrush(Microsoft.UI.Colors.White);
                break;
        }
    }
}
