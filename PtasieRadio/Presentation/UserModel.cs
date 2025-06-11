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
using Microsoft.UI.Xaml.Media.Imaging;
using System.ComponentModel.DataAnnotations;

namespace PtasieRadio.Presentation;

[Bindable]
public class UserModel : ObservableObject
{
    public const string localFileName = "users.json";
    private INavigator _navigator;
    public readonly IUserProfileService _profileService;

    // private string _selectedKeyUser;

    public IAsyncRelayCommand NavigateToSecondCommand { get; }
    public IAsyncRelayCommand NavigateToMainCommand { get; }
    public IAsyncRelayCommand<(string name, StorageFile file)> OnSaveToFileCommand { get; }
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

    // Wrappery na ProfileName i ProfileImage
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
    private BitmapImage? _profileImage;

    public BitmapImage? ProfileImage
    {
        get => _profileImage;
        private set => SetProperty(ref _profileImage, value);
    }


    public UserModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        IUserProfileService profileService,
        INavigator navigator)
    {
        _profileService = profileService;
        _navigator = navigator;
        // _addRadio = addRadio;
        NavigateToSecondCommand = new AsyncRelayCommand(GoToSecond);
        NavigateToMainCommand = new AsyncRelayCommand(GoToMain);
        OnDeleteToFileCommand = new AsyncRelayCommand(() => RemoveProfile(_profileService.SelectedKey));
        OnSaveToFileCommand = new AsyncRelayCommand<(string name, StorageFile file)>(async data =>
        {
            if (data == default) return;
            await OnSaveToFile(data.name, data.file, _navigator);
        });
        Title = "AddRadio";
        Title += $" - {localizer["ApplicationName"]}";
        Title += $" - {appInfo?.Value?.Environment}";
        _ = LoadProfileImageAsync();
    }

    public string? Title { get; }

    public IState<string> Name => State<string>.Value(this, () => string.Empty);

    public async Task OnSaveToFile(string name, StorageFile imagePath, INavigator navigator)
    {
        await _profileService.EditProfileAsync(_profileService.SelectedKey, name, imagePath);
        await GoToSecond();
    }
    public async Task EditProfile(string key, string newName, StorageFile newImagePath)
    {
        await _profileService.EditProfileAsync(key, newName, newImagePath);
    }

    public async Task AddProfile(string name, string imagePath)
    {
        var profile = new User { Name = name, ImagePath = imagePath, Misc = "" };
        await _profileService.AddProfileAsync(profile);
    }

    public async Task RemoveProfile(string? key)
    {
        await _profileService.RemoveProfileAsync(key);
        await GoToMain(_navigator);
    }

    public async Task SelectActiveProfile(string key)
    {
        await _profileService.SelectProfileAsync(key);
    }
    public async Task LoadProfileImageAsync()
    {
        if (SelectedProfile?.ImagePath is string base64 && !string.IsNullOrEmpty(base64))
        {
            ProfileImage = await _profileService.Base64ToBitmapImage(base64);
        }
        else
        {
            // ProfileImage = null;
            ProfileImage = new BitmapImage(new Uri("ms-appx:///Assets/Images/placeholder.png"));
        }
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
