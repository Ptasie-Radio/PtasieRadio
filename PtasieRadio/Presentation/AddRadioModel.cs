using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Uno.Extensions.Navigation;
using Uno.Extensions.Reactive;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Data;
using PtasieRadio.Models;
using Windows.Storage;
using WinRT.Interop;

namespace PtasieRadio.Presentation;

[Bindable]
public class AddRadioModel : ObservableObject
{
    private INavigator _navigator;
    public IAsyncRelayCommand NavigateToSecondCommand { get; }
    public IAsyncRelayCommand NavigateToMainCommand { get; }
    public IAsyncRelayCommand<SaveEntryData> OnSaveToFileCommand { get; }
    private readonly IAddRadioService _addRadio;
    public AddRadioModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        INavigator navigator,
        IAddRadioService addRadio)
    {
        _navigator = navigator;
        _addRadio = addRadio;
        NavigateToSecondCommand = new AsyncRelayCommand(GoToSecond);
        NavigateToMainCommand = new AsyncRelayCommand(GoToMain);
        OnSaveToFileCommand = new AsyncRelayCommand<SaveEntryData>(async (data) =>
        {
            if (data is null) return;
            await OnSaveToFile(data.StreamUrl, data.Name, data.Description, data.ImagePath, _navigator);
        });
        Title = "AddRadio";
        Title += $" - {localizer["ApplicationName"]}";
        Title += $" - {appInfo?.Value?.Environment}";
    }

    public string? Title { get; }

    public IState<string> Name => State<string>.Value(this, () => string.Empty);

    public async Task OnSaveToFile(string url, string name, string description, string imagePath,INavigator navigator)
    {
        await _addRadio.AddOneRadioToJson(url, name, description, imagePath);
        await GoToMain(_navigator);
    }

	public async Task GoToMain(INavigator _navigator)
	{
		await _navigator.NavigateRouteAsync(this, "/Main");
	}

	public async Task GoToSecond()
    {
        var name = await Name;
        await _navigator.NavigateRouteAsync(this, "/Second");
    }
    
     public async Task GoToMain()
    {
        var name = await Name;
        await _navigator.NavigateRouteAsync(this, "/Main");
    }
}
