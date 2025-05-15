using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Uno.Extensions.Navigation;
using Uno.Extensions.Reactive;

namespace PtasieRadio.Presentation;
public partial record AddRadioModel
{
    private INavigator _navigator;
    public IAsyncRelayCommand NavigateToMainCommand { get; }//Tworzenie komendy nawigacyjnej
    public IAsyncRelayCommand NavigateToSecondCommand { get; }
    public AddRadioModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        INavigator navigator)
    {
        _navigator = navigator;
        NavigateToMainCommand = new AsyncRelayCommand(GoToMain);//Komenda do zmieniania na widok główny
        NavigateToSecondCommand = new AsyncRelayCommand(GoToSecond);
        Title = "AddRadio";
        Title += $" - {localizer["ApplicationName"]}";
        Title += $" - {appInfo?.Value?.Environment}";
    }

    public string? Title { get; }

    public IState<string> Name => State<string>.Value(this, () => string.Empty);

    public async Task GoToMain()
    {
        var name = await Name;;
        await _navigator.NavigateRouteAsync(this, "/Main");
    }
    public async Task GoToSecond()
    {
        var name = await Name;;
        await _navigator.NavigateRouteAsync(this, "/Second");
    }
}
