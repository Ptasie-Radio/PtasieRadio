using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Uno.Extensions.Navigation;

namespace PtasieRadio.Presentation;

public class ChangeThemeModel : ObservableObject
{
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
        }
    }
}


 

}
