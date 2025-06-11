using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Uno.Extensions.Navigation;
using Uno.Extensions.Reactive;
using Microsoft.UI.Xaml.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using PtasieRadio.Services.UserProfileService;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Media.Imaging;

namespace PtasieRadio.Presentation;


[Bindable]
public partial class SecondModel : ObservableObject
{
    public IRelayCommand<string> SelectProfileCommand { get; set; }
    public ObservableCollection<KeyValuePair<string, User>> ProfileList { get; set; }
    private INavigator _navigator;
    internal IUserProfileService _profileService;
    public BitmapImage? _profileImage;
    public BitmapImage? ProfileImage
    {
        get => _profileImage;
        set => SetProperty(ref _profileImage, value);
    }

    public IAsyncRelayCommand NavigateToMainCommand { get; }//Tworzenie komendy nawigacyjnej
    public IAsyncRelayCommand NavigateToAddRadioCommand { get; }
    public IAsyncRelayCommand NavigateToUserCommand { get; }
    public SecondModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        IUserProfileService profileService,
        INavigator navigator)
    {
        _navigator = navigator;
        NavigateToMainCommand = new AsyncRelayCommand(GoToMain);//Komenda do zmieniania na widok główny
        NavigateToAddRadioCommand = new AsyncRelayCommand(GoToAddRadio);
        NavigateToUserCommand = new AsyncRelayCommand(GoToUser);
        Title = "Second";
        Title += $" - {localizer["ApplicationName"]}";
        Title += $" - {appInfo?.Value?.Environment}";
        _profileService = profileService;
        InitializeAsync();
    }
    public Task InitializeAsync()
    {
        _profileService.InitializeAsync(); // Upewnij się, że profile są załadowane
        _ = LoadProfileImageAsync();
        _ = LoadProfilesAsync();
        return Task.CompletedTask;
    }
    private async Task LoadProfileImageAsync()
    {
        if (!string.IsNullOrEmpty(SelectedProfile?.ImagePath))
        {
            ProfileImage = await _profileService.Base64ToBitmapImage(SelectedProfile.ImagePath);
        }
        else
        {
            // Domyślny obraz, np. z assets
            ProfileImage = new BitmapImage(new Uri("ms-appx:///Assets/Images/user.png"));
        }
    }
    private Task LoadProfilesAsync()
    {
        ProfileList = new ObservableCollection<KeyValuePair<string, User>>(_profileService.Profiles);
        // Powiadomienie o zmianie, jeśli korzystasz z INotifyPropertyChanged
        OnPropertyChanged(nameof(ProfileList));
        System.Diagnostics.Debug.WriteLine("zaladowanie profilow");
        System.Diagnostics.Debug.WriteLine($"Profile count in SecondModel: {_profileService.Profiles.Count}");
        return Task.CompletedTask;
    }
    public string? Title { get; }

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

    // Wrapper na ProfileName
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
    public IState<string> Name => State<string>.Value(this, () => string.Empty);

    public async Task GoToMain()
    {
        var name = await Name;
        await _navigator.NavigateRouteAsync(this, "/Main");//Ten / musi tu być, aby zawsze odnajdował nasz model w ścieżce, bez tego po jednokrotnym wykonaniu, ścieżka się zmienia
    }
    public async Task GoToAddRadio()
    {
        var name = await Name;
        await _navigator.NavigateRouteAsync(this, "/AddRadio");
    }
    public async Task GoToUser()
    {
        await _navigator.NavigateRouteAsync(this, "/User");
    }
}
