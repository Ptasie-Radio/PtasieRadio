using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Uno.Extensions.Navigation;
using Uno.Extensions.Reactive;
using Microsoft.UI.Xaml.Data;
using CommunityToolkit.Mvvm.ComponentModel;
 
namespace PtasieRadio.Presentation;


[Bindable]
public partial class SecondModel : ObservableObject
{
    private INavigator _navigator;
    public IAsyncRelayCommand NavigateToMainCommand { get; }//Tworzenie komendy nawigacyjnej
    public IAsyncRelayCommand NavigateToAddRadioCommand { get; }
    public SecondModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        INavigator navigator)
    {
        _navigator = navigator;
        NavigateToMainCommand = new AsyncRelayCommand(GoToMain);//Komenda do zmieniania na widok główny
        NavigateToAddRadioCommand = new AsyncRelayCommand(GoToAddRadio);
        Title = "Second";
        Title += $" - {localizer["ApplicationName"]}";
        Title += $" - {appInfo?.Value?.Environment}";
    }

    public string? Title { get; }

    public IState<string> Name => State<string>.Value(this, () => string.Empty);

    public async Task GoToMain()
    {
        var name = await Name;
        await _navigator.NavigateRouteAsync(this, "/Main");//Ten / musi tu być, aby zawsze odnajdował nasz model w ścieżce, bez tego po jednokrotnym
        //Wykonaniu, ścieżka się zmienia
    }

public async Task GoToChangeTheme()
{
    var name = await Name;
    await _navigator.NavigateRouteAsync(this, "/ChangeTheme");
}


    public async Task GoToAddRadio()
    {
        var name = await Name;
        await _navigator.NavigateRouteAsync(this, "/AddRadio");
    }


}
