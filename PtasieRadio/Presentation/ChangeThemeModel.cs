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
        _isCiemnyChecked = false;
        _isCherryDarkChecked = false;
        _isCherryLightChecked = false;
        _isGraphiteChecked = false;

        switch (theme)
        {
            case "Jasny":
                _isJasnyChecked = true;
                break;
            case "Ciemny":
                _isCiemnyChecked = true;
                break;
            case "CherryDark":
                _isCherryDarkChecked = true;
                break;
            case "CherryLight":
                _isCherryLightChecked = true;
                break;
            case "Graphite":
                _isGraphiteChecked = true;
                break;
        }
        OnPropertyChanged(nameof(IsJasnyChecked));
        OnPropertyChanged(nameof(IsCiemnyChecked));
        OnPropertyChanged(nameof(IsCherryDarkChecked));
        OnPropertyChanged(nameof(IsCherryLightChecked));
        OnPropertyChanged(nameof(IsGraphiteChecked));
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
                IsCiemnyChecked = false;
                IsCherryDarkChecked = false;
                IsCherryLightChecked = false;
                IsGraphiteChecked = false;
                ThemeService.ApplyTheme("Jasny");
                ThemeService.SaveThemeAsync("Jasny");
                _ = RefreshPage();
            }
        }
    }

    private bool _isCiemnyChecked;
    public bool IsCiemnyChecked
    {
        get => _isCiemnyChecked;
        set
        {
            if (SetProperty(ref _isCiemnyChecked, value) && value)
            {
                IsJasnyChecked = false;
                IsCherryDarkChecked = false;
                IsCherryLightChecked = false;
                IsGraphiteChecked = false;
                ThemeService.ApplyTheme("Ciemny");
                ThemeService.SaveThemeAsync("Ciemny");
                _ = RefreshPage();
            }
        }
    }

    private bool _isCherryDarkChecked;
    public bool IsCherryDarkChecked
    {
        get => _isCherryDarkChecked;
        set
        {
            if (SetProperty(ref _isCherryDarkChecked, value) && value)
            {
                IsJasnyChecked = false;
                IsCiemnyChecked = false;
                IsCherryLightChecked = false;
                IsGraphiteChecked = false;
                ThemeService.ApplyTheme("CherryDark");
                ThemeService.SaveThemeAsync("CherryDark");
                _ = RefreshPage();
            }
        }
    }

    private bool _isCherryLightChecked;
    public bool IsCherryLightChecked
    {
        get => _isCherryLightChecked;
        set
        {
            if (SetProperty(ref _isCherryLightChecked, value) && value)
            {
                IsJasnyChecked = false;
                IsCiemnyChecked = false;
                IsCherryDarkChecked = false;
                IsGraphiteChecked = false;
                ThemeService.ApplyTheme("CherryLight");
                ThemeService.SaveThemeAsync("CherryLight");
                _ = RefreshPage();
            }
        }
    }

    private bool _isGraphiteChecked;
    public bool IsGraphiteChecked
    {
        get => _isGraphiteChecked;
        set
        {
            if (SetProperty(ref _isGraphiteChecked, value) && value)
            {
                IsJasnyChecked = false;
                IsCiemnyChecked = false;
                IsCherryDarkChecked = false;
                IsCherryLightChecked = false;
                ThemeService.ApplyTheme("Graphite");
                ThemeService.SaveThemeAsync("Graphite");
                _ = RefreshPage();
            }
        }
    }
}



