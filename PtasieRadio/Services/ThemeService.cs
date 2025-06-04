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
                res["PrimaryColor"] = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255,217, 217, 217));
                res["SecondaryColor"] = new SolidColorBrush(Microsoft.UI.Colors.Black);
                res["TextColor"] = new SolidColorBrush(Colors.Black);
                res["IconColor"] = new SolidColorBrush(Microsoft.UI.Colors.Black);

                break;
            case "Dark":
                res["PrimaryColor"] = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 28, 27, 31));
                res["SecondaryColor"] = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 27, 32, 41));
                res["TextColor"] = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 249, 249, 249));
                res["IconColor"] = new SolidColorBrush(Colors.White);
                break;
            case "BlueberryLight":
                res["PrimaryColor"] = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 217, 226, 251));
                res["SecondaryColor"] = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 207, 220, 254));
                res["TextColor"] = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 10, 23, 57));
                res["IconColor"] = new SolidColorBrush(Colors.Black);
                break;
            case "BlueberryDark":
                res["PrimaryColor"] = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 1, 11, 34));
                res["SecondaryColor"] = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 6, 28, 59));
                res["TextColor"] = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 209, 218, 244));
                res["IconColor"] = new SolidColorBrush(Colors.White);
                break;
            case "LimeDark":
                res["PrimaryColor"] = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 1, 27, 19));
                res["SecondaryColor"] = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 7, 56, 40));
                res["TextColor"] = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 211, 245, 233));
                res["IconColor"] = new SolidColorBrush(Colors.White);
                break;
        }
    }
}
