using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Uno.Extensions.Navigation;
using PtasieRadio.Services;

namespace PtasieRadio.Presentation;

public class ChangeThemeModel : ObservableObject
{
 private async Task RefreshPage()
{
    await _navigator.NavigateRouteAsync(this, "/ChangeTheme");
}

    private void ApplySavedThemeState()
    {
        var theme = ThemeService.LoadTheme();

        // Wyłącz efekty uboczne: bez setterów
        _isJasnyChecked = false;
        _isDarkChecked = false;
        _isBlueberryLightChecked = false;
        _isBlueberryDarkChecked = false;
        _isLimeDarkChecked = false;

        switch (theme)
        {
            case "Jasny":
                _isJasnyChecked = true;
                break;
            case "Dark":
                _isDarkChecked = true;
                break;
            case "BlueberryLight":
                _isBlueberryLightChecked = true;
                break;
            case "BlueberryDark":
                _isBlueberryDarkChecked = true;
                break;
            case "LimeDark":
                _isLimeDarkChecked = true;
                break;
        }
        OnPropertyChanged(nameof(IsJasnyChecked));
        OnPropertyChanged(nameof(IsDarkChecked));
        OnPropertyChanged(nameof(IsBlueberryLightChecked));
        OnPropertyChanged(nameof(IsBlueberryDarkChecked));
        OnPropertyChanged(nameof(IsLimeDarkChecked));
    }
  private readonly INavigator _navigator;

  public IAsyncRelayCommand NavigateToSecondCommand { get; }

    public ChangeThemeModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        INavigator navigator)
    {
        _navigator = navigator;

        NavigateToSecondCommand = new AsyncRelayCommand(GoToSecond);

        Title = "Change Theme";
        Title += $" - {localizer["ApplicationName"]}";
        Title += $" - {appInfo?.Value?.Environment}";
     ApplySavedThemeState();
  }

  public string? Title { get; }

  private async Task GoToSecond()
  {
    await _navigator.NavigateRouteAsync(this, "/Second");
  }

    private bool _isJasnyChecked;
    public bool IsJasnyChecked
    {
        get => _isJasnyChecked;
        set
        {
            if (SetProperty(ref _isJasnyChecked, value) && value)
            {
                IsDarkChecked = false;
                IsBlueberryLightChecked = false;
                IsBlueberryDarkChecked = false;
                IsLimeDarkChecked = false;
                ThemeService.ApplyTheme("Jasny");
                ThemeService.SaveThemeAsync("Jasny");
                _ = RefreshPage();
            }
        }
    }

    private bool _isDarkChecked;
    public bool IsDarkChecked
    {
        get => _isDarkChecked;
        set
        {
            if (SetProperty(ref _isDarkChecked, value) && value)
            {
                IsJasnyChecked = false;
                IsBlueberryLightChecked = false;
                IsBlueberryDarkChecked = false;
                IsLimeDarkChecked = false;
                ThemeService.ApplyTheme("Dark");
                ThemeService.SaveThemeAsync("Dark");
                _ = RefreshPage();
            }
        }
    }

    private bool _isBlueberryLightChecked;
    public bool IsBlueberryLightChecked
    {
        get => _isBlueberryLightChecked;
        set
        {
            if (SetProperty(ref _isBlueberryLightChecked, value) && value)
            {
                IsJasnyChecked = false;
                IsDarkChecked = false;
                IsBlueberryDarkChecked = false;
                IsLimeDarkChecked = false;
                ThemeService.ApplyTheme("BlueberryLight");
                ThemeService.SaveThemeAsync("BlueberryLight");
                _ = RefreshPage();
            }
        }
    }

    private bool _isBlueberryDarkChecked;
    public bool IsBlueberryDarkChecked
    {
        get => _isBlueberryDarkChecked;
        set
        {
            if (SetProperty(ref _isBlueberryDarkChecked, value) && value)
            {
                IsJasnyChecked = false;
                IsDarkChecked = false;
                IsBlueberryLightChecked = false;
                IsLimeDarkChecked = false;
                ThemeService.ApplyTheme("BlueberryDark");
                ThemeService.SaveThemeAsync("BlueberryDark");
                _ = RefreshPage();
            }
        }
    }

    private bool _isLimeDarkChecked;
    public bool IsLimeDarkChecked
    {
        get => _isLimeDarkChecked;
        set
        {
            if (SetProperty(ref _isLimeDarkChecked, value) && value)
            {
                IsJasnyChecked = false;
                IsDarkChecked = false;
                IsBlueberryLightChecked = false;
                IsBlueberryDarkChecked = false;
                ThemeService.ApplyTheme("LimeDark");
                ThemeService.SaveThemeAsync("LimeDark");
                _ = RefreshPage();
            }
        }
    }
}



