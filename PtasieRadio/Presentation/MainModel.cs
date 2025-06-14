using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Uno.Extensions.Navigation;
using Uno.Extensions.Reactive;
using PtasieRadio.Services.RadioService;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Data;
using Windows.Foundation;
using Microsoft.UI.Xaml.Input;
using PtasieRadio.Services.UserProfileService;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PtasieRadio.Presentation;

[Bindable]

public class MainModel : ObservableObject
{
    private INavigator _navigator;
    public IAsyncRelayCommand NavigateCommand { get; }//Tworzenie komendy nawigacyjnej
    public IAsyncRelayCommand PlayRadioCommand { get; }

    private readonly IRadioPlayerService _radioService;
    public readonly IUserProfileService _profileService;
    private readonly IShowPromptService _promptService;
    public ICommand ToggleChangeStationNameCommand { get; }
    public ICommand ToggleChangeCountryCommand { get; }
    public ICommand ToggleChangeImagePathCommand { get; }
    private string? url;
    private bool isChangingUrl = false;
    public IAsyncRelayCommand HeartButtonCommand { get; }
    public string HeartButtonImage => _isFavourite ?
    "M740 2789 c-272 -23 -514 -200 -645 -473 -65 -135 -89 -243 -89 -401 0 -146 18 -232 77 -372 153 -363 570 -794 1127 -1169 201 -134 253 -164 291 -164 84 0 594 367 864 621 343 323 531 595 606 879 34 127 32 309 -4 443 -47 172 -126 307 -248 422 -79 75 -106 94 -197 140 -155 77 -358 99 -526 56 -166 -43 -354 -175 -451 -315 -21 -31 -41 -56 -45 -56 -4 0 -24 25 -45 56 -65 94 -184 196 -294 252 -139 71 -263 95 -421 81z" :
    "M636 2809 c-268 -66 -496 -284 -584 -559 -136 -425 -8 -802 417 -1229 178 -179 885 -796 954 -833 43 -23 109 -22 155 1 38 20 463 382 753 642 346 310 533 550 615 792 151 442 -33 933 -420 1125 -209 104 -472 111 -678 19 -109 -49 -234 -148 -316 -252 l-32 -40 -32 40 c-117 147 -272 251 -443 296 -96 26 -281 25 -389 -2z m391 -179 c126 -44 247 -141 339 -272 64 -91 86 -108 134 -108 48 0 70 17 134 108 93 133 214 229 344 274 97 33 277 33 377 0 106 -36 172 -77 261 -166 138 -137 204 -307 204 -527 0 -331 -162 -585 -645 -1014 -250 -221 -666 -575 -676 -575 -10 0 -408 340 -674 575 -483 428 -645 683 -645 1014 0 220 66 390 204 527 89 88 157 132 256 163 93 29 103 30 220 26 74 -2 123 -10 167 -25z";
    private bool _isFavourite;
    public bool isFavourite
    {
        get => _isFavourite;
        set
        {
            _isFavourite = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HeartButtonImage));
        }
    }
    private string _stationName;

    public string StationName
    {
        get => _stationName;
        set
        {
            _stationName = value;
            _radioService.StationName = value;
            OnPropertyChanged();
        }
    }

    private string _country;
    public string Country
    {
        get => _country;
        set
        {
            _country = value;
            _radioService.StationCountry = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StationFlagUrl)); // Dodaj tę linię
        }
    }
    private string _imagePath;
    public string ImagePath
    {
        get => _imagePath;
        set
        {
            _imagePath = value;
            _radioService.StationImagePath = value;
            OnPropertyChanged();
        }
    }


    public IRelayCommand ToggleMuteCommand { get; }
    public IAsyncRelayCommand<string?> ToggleChangeUrlCommand { get; }

    private Point _lastPointerPosition;
    private ScrollViewer? _currentScrollViewer;

    public IRelayCommand<PointerRoutedEventArgs> ScrollStartCommand { get; }
    public IRelayCommand<PointerRoutedEventArgs> ScrollMoveCommand { get; }
    public RelayCommand<PointerRoutedEventArgs> ScrollStopCommand { get; }

    private bool _isMuted;
    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            if (SetProperty(ref _isMuted, value))
            {
                OnPropertyChanged(nameof(MuteButtonImage));
            }
        }
    }
    public string MuteButtonImage => IsMuted
    ? "M1355 2993 c-201 -24 -375 -76 -544 -163 -411 -213 -697 -599 -788 -1065 -24 -125 -24 -405 0 -530 29 -148 74 -279 142 -413 316 -622 1021 -945 1696 -777 127 32 209 64 339 131 401 208 687 598 777 1059 25 128 24 396 -1 530 -135 721 -763 1242 -1486 1233 -58 0 -118 -3 -135 -5z m-342 -597 c18 -8 102 -84 187 -170 85 -86 159 -156 163 -156 5 0 102 74 215 164 114 90 217 166 229 170 26 8 78 -15 87 -39 3 -9 6 -202 6 -428 l0 -411 220 -221 c209 -210 220 -223 220 -259 0 -48 -13 -72 -52 -92 -66 -34 -46 -51 -751 654 -626 626 -657 659 -657 693 0 40 29 85 64 99 29 12 32 12 69 -4z m67 -896 l0 -340 -181 0 -181 0 -29 29 -29 29 0 282 0 282 29 29 29 29 181 0 181 0 0 -340z m820 -658 c0 -106 -4 -201 -10 -211 -13 -25 -61 -44 -87 -34 -11 4 -149 109 -307 233 l-285 225 -1 335 0 335 345 -345 345 -345 0 -193z"
    // muted
    : "M1355 2993 c-201 -24 -375 -76 -544 -163 -411 -213 -697 -599 -788 -1065 -24 -125 -24 -405 0 -530 29 -148 74 -279 142 -413 316 -622 1021 -945 1696 -777 127 32 209 64 339 131 401 208 687 598 777 1059 25 128 24 396 -1 530 -135 721 -763 1242 -1486 1233 -58 0 -118 -3 -135 -5z m220 -738 l25 -24 0 -731 0 -731 -26 -25 c-19 -20 -31 -24 -50 -20 -14 4 -138 95 -275 204 l-249 197 0 375 0 375 246 195 c136 107 254 198 263 202 28 12 41 9 66 -17z m612 -184 c141 -111 239 -293 263 -487 30 -242 -77 -508 -263 -655 -57 -46 -93 -50 -131 -14 -18 17 -26 34 -26 59 0 29 11 45 79 114 119 120 174 250 174 412 0 162 -55 292 -174 412 -68 69 -79 85 -79 114 0 25 8 42 26 59 38 36 74 32 131 -14z m-247 -266 c91 -67 150 -187 150 -305 0 -118 -59 -238 -149 -304 -60 -44 -91 -52 -129 -32 -34 17 -42 33 -42 81 0 29 7 40 47 70 80 61 98 95 98 185 0 90 -18 124 -98 185 -41 32 -47 40 -47 74 0 39 12 61 45 80 30 17 71 5 125 -34z m-1050 -305 l0 -290 -152 0 c-160 1 -174 4 -196 47 -16 30 -16 456 0 486 22 43 36 46 196 47 l152 0 0 -290z";
    // unmuted

    private double _Volume;
    public double Volume
    {
        get => _Volume;
        set
        {
            if (_Volume != value)
            {
                _Volume = value;
                _radioService.SetVolume(value);

                if (IsMuted)
                {
                    IsMuted = false;
                    _radioService.SetIsMuted(IsMuted);//Używamy tego, aby przy zmianie dźwięku ustawić na Radiu że nie jest wyciszone
                    OnPropertyChanged(nameof(MuteButtonImage));
                }
            }
        }
    }

    private bool _isPlaying;

    public bool isPlaying
    {
        get => _isPlaying;
        set
        {
            if (SetProperty(ref _isPlaying, value))
            {
                OnPropertyChanged(nameof(PlayPauseButtonImage));
                OnPropertyChanged(nameof(MiniPlayPauseButtonImage));
                Console.WriteLine(Country);
            }
        }
    }
    public string PlayPauseButtonImage => isPlaying
    ? "M1295 2984 c-335 -51 -593 -177 -828 -404 -259 -249 -399 -522 -451 -881 -34 -232 -5 -500 79 -725 42 -109 130 -271 199 -364 83 -111 259 -280 366 -351 458 -305 1025 -342 1513 -98 192 96 401 272 533 449 228 306 333 716 278 1089 -49 338 -175 597 -404 834 -249 259 -522 399 -881 451 -128 19 -281 19 -404 0z m-37 -971 c15 -11 37 -35 50 -53 22 -33 22 -36 23 -429 1 -218 0 -409 -1 -426 -6 -93 -99 -159 -197 -140 -49 9 -108 68 -122 123 -8 29 -11 165 -9 440 l3 398 25 37 c38 55 85 78 148 74 32 -2 63 -11 80 -24z m652 4 c30 -16 51 -37 67 -67 23 -43 23 -45 23 -449 0 -402 0 -407 -22 -451 -61 -120 -234 -117 -292 3 -14 31 -16 86 -16 445 0 267 4 421 11 439 34 92 139 128 229 80z"
    // pause_round.svg 
    : "M1295 2984 c-335 -51 -593 -177 -828 -404 -259 -249 -399 -522 -451 -881 -34 -232 -5 -500 79 -725 42 -109 130 -271 199 -364 83 -111 259 -280 366 -351 458 -305 1025 -342 1513 -98 192 96 401 272 533 449 228 306 333 716 278 1089 -49 338 -175 597 -404 834 -249 259 -522 399 -881 451 -128 19 -281 19 -404 0z m11 -960 c213 -128 710 -443 730 -463 33 -33 33 -89 0 -122 -20 -20 -517 -335 -730 -463 -92 -56 -154 -44 -176 32 -14 50 -14 934 0 984 22 76 84 88 176 32z";
    //play_round.svg 
    public string MiniPlayPauseButtonImage => isPlaying
    ? "M598 2981 c-122 -39 -206 -113 -261 -229 l-32 -67 0 -1185 0 -1185 32 -67 c89 -189 295 -288 484 -232 145 43 243 135 290 272 18 53 19 102 19 1212 0 1110 -1 1159 -19 1212 -74 215 -304 335 -513 269z M2170 2981 c-77 -25 -115 -46 -168 -94 -55 -51 -88 -102 -113 -175 -18 -53 -19 -102 -19 -1212 0 -1110 1 -1159 19 -1212 47 -137 145 -229 290 -272 189 -56 395 43 484 232 l32 67 0 1185 0 1185 -32 67 c-91 192 -303 291 -493 229z"
    : "M570 2994 c-53 -10 -147 -48 -198 -80 -111 -71 -189 -180 -222 -310 -20 -79 -20 -103 -18 -1135 l3 -1054 23 -60 c84 -224 298 -367 527 -352 44 2 106 14 137 25 32 11 455 249 940 529 798 460 888 514 950 574 209 203 208 535 -2 739 -56 55 -121 96 -401 257 -184 105 -581 334 -883 508 -302 174 -574 326 -605 337 -55 19 -199 32 -251 22z";
    // mini_play.svg mini_pause.svg
    public string? Title { get; }

    public IState<string> Name => State<string>.Value(this, () => string.Empty);
    public string CountryFlagUrl => $"https://flagcdn.com/h20/{Country?.ToLower()}.png"; // nie dziala
    public string StationFlagUrl => CountryFlagUrl ?? "";


    public MainModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        INavigator navigator,
        IUserProfileService profileService,
        IRadioPlayerService radioService,
        IShowPromptService promptService)
    {
        _navigator = navigator;
        NavigateCommand = new AsyncRelayCommand(GoToSecond);


        _radioService = radioService;
        _promptService = promptService;
        url = _radioService.GetUrl();
        _profileService = profileService;

        if (url == null) url = "http://chi.cdn.eurozet.pl/chi-net.mp3";
        if (_radioService.StationName != null) _stationName = _radioService.StationName;
        if (_radioService.StationImagePath != null) _imagePath = _radioService.StationImagePath;
        else
        {
            _imagePath = "Assets\\Images\\radio_placeholder_square.png";
        }
        if (_radioService.StationCountry != null) _country = _radioService.StationCountry;

        ToggleMuteCommand = new RelayCommand(ToggleMute);
        ToggleChangeUrlCommand = new AsyncRelayCommand<string?>(ToggleChangeUrl);
        ToggleChangeStationNameCommand = new RelayCommand<string?>(ToggleChangeName);
        ToggleChangeCountryCommand = new RelayCommand<string?>(ToggleChangeCountry);
        ToggleChangeImagePathCommand = new RelayCommand<string?>(ToggleChangeImagePath);
        PlayRadioCommand = new AsyncRelayCommand(PlayRadio);
        HeartButtonCommand = new AsyncRelayCommand(HeartButton);

        Title = "Main";
        Title += $" - {localizer["ApplicationName"]}";
        Title += $" - {appInfo?.Value?.Environment}";

        IsMuted = _radioService.GetIsMuted();
        isPlaying = _radioService.GetIsPlaying();
        _Volume = _radioService.GetVolume();
        isFavourite = _isFavourite;

        ScrollStartCommand = new RelayCommand<PointerRoutedEventArgs>(ScrollStart);
        ScrollMoveCommand = new RelayCommand<PointerRoutedEventArgs>(ScrollMove);
        ScrollStopCommand = new RelayCommand<PointerRoutedEventArgs>(ScrollStop);
    }

    public static async Task<StorageFolder> OpenFolder() =>
        await ApplicationData.Current.LocalFolder.CreateFolderAsync(
            "PtasieRadio", CreationCollisionOption.OpenIfExists);
    public async Task HeartButton()
    {
        isFavourite = !isFavourite;
        System.Diagnostics.Debug.WriteLine(isFavourite);

        var folder = await OpenFolder();
        var file = await folder.GetFileAsync("radio.json");
        string json = await FileIO.ReadTextAsync(file);

        var entries = JsonConvert.DeserializeObject<Dictionary<string, SaveEntryData>>(json)
                      ?? new Dictionary<string, SaveEntryData>();

        _ = _profileService.FavouriteStationKeyToCurrentProfile(entries.FirstOrDefault(x => x.Value.StreamUrl == url).Key);
        _ = Refresh();
    }
    // Poczatek scrolli
    private void ScrollStop(PointerRoutedEventArgs? e)
    {
        if (_currentScrollViewer != null && e != null && e.OriginalSource is UIElement element)
        {
            // _currentScrollViewer.ChangeView(_currentScrollViewer.HorizontalOffset, null, null,false);
            element.ReleasePointerCapture(e.Pointer);
            _currentScrollViewer = null;
            _lastPointerPosition = default;
        }
    }

    private void ScrollStart(PointerRoutedEventArgs? e)
    {
        // Pobierz ScrollViewer z parametru (przekazanego przez XAML)
        if (e != null && e.OriginalSource is FrameworkElement element &&
            element.FindParent<ScrollViewer>() is ScrollViewer scrollViewer)
        {
            _currentScrollViewer = scrollViewer;
            _lastPointerPosition = e.GetCurrentPoint(scrollViewer).Position;
            // Dodaj jawne przechwytywanie wskaźnika
            if (element is UIElement uiElement)
            {
                uiElement.CapturePointer(e.Pointer);
            }
        }
    }

    private void ScrollMove(PointerRoutedEventArgs? e)
    {
        if (e != null && _currentScrollViewer != null)
        {
            var currentPosition = e.GetCurrentPoint(_currentScrollViewer).Position;
            var deltaX = _lastPointerPosition.X - currentPosition.X;

            _currentScrollViewer.ChangeView(
                _currentScrollViewer.HorizontalOffset + deltaX,
                null,
                null,
                true
            );

            _lastPointerPosition = currentPosition;
        }
    }

    // Koniec scrolli

    public async Task PlayRadio()
    {
        if (isChangingUrl) return;
        isPlaying = !isPlaying;
        OnPropertyChanged(nameof(PlayPauseButtonImage));
        OnPropertyChanged(nameof(MiniPlayPauseButtonImage));
        if (url == null) url = "";
        if (_radioService.GetUrl() == null) _radioService.SetUrl(url);
        await _radioService.PlayOrPauseAsync();
        if (IsMuted && !_radioService.GetIsMuted())
        {
            _radioService.ToggleMute();
        }
        if (!_radioService.GetIsInitialized() && isPlaying)
        {
            await _promptService.ShowMessageAsync("Nie udało się załadować radia", "Błąd");
        }
    }
    public async Task GoToSecond()
    {
        await _navigator.NavigateRouteAsync(this, "/Second");
    }
    public async Task Refresh()
    {
        await _navigator.NavigateRouteAsync(this, "/Main");
    }

    private void ToggleMute()
    {
        IsMuted = !IsMuted;
        OnPropertyChanged(nameof(MuteButtonImage));
        _radioService.ToggleMute();
    }

    private async Task ToggleChangeUrl(string? url)
    {
        if (isChangingUrl) return;
        try
        {
            isChangingUrl = true;
            if (url == null) url = "";
            this.url = url;
            await _radioService.Reset();
            _radioService.SetUrl(url);

            if (isPlaying)
            {
                await _radioService.PlayOrPauseAsync();
                if (!_radioService.GetIsInitialized())
                {
                    await _promptService.ShowMessageAsync("Nie udało się załadować radia", "Błąd");
                }
            }
            OnPropertyChanged(nameof(MuteButtonImage));
            OnPropertyChanged(nameof(PlayPauseButtonImage));
            OnPropertyChanged(nameof(MiniPlayPauseButtonImage));
        }
        finally
        {
            isChangingUrl = false;
        }
    }
    private void ToggleChangeImagePath(string? imagePath) { ImagePath = imagePath; }

    private void ToggleChangeName(string? name) { StationName = name; }
    private void ToggleChangeCountry(string? country) { Country = country; }

}

// Extension method do znajdowania ScrollViewera w drzewie wizualnym
public static class Extensions
{
    public static T? FindParent<T>(this DependencyObject element) where T : DependencyObject
    {
        while (element != null)
        {
            if (element is T parent) return parent;
            element = VisualTreeHelper.GetParent(element);
        }
        return default;
    }
}
