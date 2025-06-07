using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Uno.Extensions.Navigation;
using Uno.Extensions.Reactive;
using Microsoft.UI.Xaml.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using PtasieRadio.Services.UserProfileService;
using System.Collections.ObjectModel;

namespace PtasieRadio.Presentation;


[Bindable]
public partial class SecondModel : ObservableObject
{
    public IRelayCommand<string> SelectProfileCommand { get; set; }
    public ObservableCollection<KeyValuePair<string, User>> ProfileList { get; set; }
    private INavigator _navigator;
    internal IUserProfileService _profileService;

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
        _ = LoadProfilesAsync();
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

    private User _selectedProfile;
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
