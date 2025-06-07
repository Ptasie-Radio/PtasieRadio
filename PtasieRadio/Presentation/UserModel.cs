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
using Newtonsoft.Json;
using PtasieRadio.Services.UserProfileService;

namespace PtasieRadio.Presentation;

[Bindable]
public class UserModel : ObservableObject
{
    public const string localFileName = "users.json";
    private INavigator _navigator;
    private IUserProfileService _profileService;

    // private string _selectedKeyUser;

    public IAsyncRelayCommand NavigateToSecondCommand { get; }
    public IAsyncRelayCommand NavigateToMainCommand { get; }
    public IAsyncRelayCommand<User> OnSaveToFileCommand { get; }
    public IAsyncRelayCommand OnDeleteToFileCommand { get; }
    // private readonly IAddRadioService _addRadio;
    public User? SelectedProfile
    {
        get
        {
            var key = _profileService.SelectedKey;
            if (!string.IsNullOrEmpty(key) && _profileService.Profiles.TryGetValue(key, out var profile))
                return profile;
            return null;
        }
    }

    // Wrappery na ProfileName i ProfileImagePath
    public string ProfileName
    {
        get => SelectedProfile?.Name ?? string.Empty;
        set
        {
            if (SelectedProfile != null && SelectedProfile.Name != value)
            {
                SelectedProfile.Name = value;
                OnPropertyChanged(nameof(ProfileName));
            }
        }
    }
    public string ProfileImagePath
    {
        get => SelectedProfile?.ImagePath ?? string.Empty;
        set
        {
            if (SelectedProfile != null && SelectedProfile.ImagePath != value)
            {
                SelectedProfile.ImagePath = value;
                OnPropertyChanged(nameof(ProfileImagePath));
            }
        }
    }

    public UserModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        IUserProfileService profileService,
        INavigator navigator)
    {
        // co zrobic: 
        // usuniecie uswa wszystkie profile
        // potencjalnie ukryc/zaimplementowac wybor innych profilow; jakos wprowadzic dodawanie profilow
        // ogarnac jak zrobic by zmiana wartosci nazwy/zdjecia  nie wykonywala sie do momentu kliknieca w 'zapisz'
        _profileService = profileService;
        _navigator = navigator;
        // _addRadio = addRadio;
        NavigateToSecondCommand = new AsyncRelayCommand(GoToSecond);
        NavigateToMainCommand = new AsyncRelayCommand(GoToMain);
        OnDeleteToFileCommand = new AsyncRelayCommand(() => RemoveProfile(_profileService.SelectedKey));
        // OnSaveToFileCommand = new AsyncRelayCommand<SaveEntryData>(async (data) =>
        OnSaveToFileCommand = new AsyncRelayCommand<User>(async (data) =>
        {
            if (data is null) return;
            await OnSaveToFile(data.Name, data.ImagePath, _navigator);
        });
        Title = "AddRadio";
        Title += $" - {localizer["ApplicationName"]}";
        Title += $" - {appInfo?.Value?.Environment}";
    }

    public string? Title { get; }

    public IState<string> Name => State<string>.Value(this, () => string.Empty);

    public async Task OnSaveToFile(string name, string imagePath, INavigator navigator)
    {
        await _profileService.EditProfileAsync(_profileService.SelectedKey, name, imagePath);
        await GoToMain(_navigator);
    }
    public async Task EditProfile(string key, string newName, string newImagePath)
    {
        await _profileService.EditProfileAsync(key, newName, newImagePath);
    }

    public async Task AddProfile(string name, string imagePath)
    {
        var profile = new User { Name = name, ImagePath = imagePath, Misc = "" };
        await _profileService.AddProfileAsync(profile);
    }

    public async Task RemoveProfile(string key)
    {
        await _profileService.RemoveProfileAsync(key);
        await GoToMain(_navigator);
    }

    public async Task SelectActiveProfile(string key)
    {
        await _profileService.SelectProfileAsync(key);
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
