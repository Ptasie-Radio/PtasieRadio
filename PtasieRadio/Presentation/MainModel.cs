using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

using PtasieRadio.Services.RadioStationsService;


namespace PtasieRadio.Presentation;

public partial class MainModel : ObservableObject
{
    private INavigator _navigator;
    public IAsyncRelayCommand NavigateCommand { get; }//Tworzenie komendy nawigacyjnej
    public IAsyncRelayCommand PlayRadioCommand { get; }
    public IAsyncRelayCommand<RadioStation> PlayStationCommand { get; }
    private readonly IRadioPlayerService _radioService;
    private readonly IRadioStationsService _radioStationsService;

    // Kolekcje dla różnych kategorii stacji
    public ObservableCollection<RadioStation> PopularStations { get; set; } = new();
    public ObservableCollection<RadioStation> MostPlayedStations { get; set; } = new();

    public IRelayCommand ToggleMuteCommand { get; }

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
    ? "ms-appx:///Assets/Images/speaker_muted_round.png"
    : "ms-appx:///Assets/Images/speaker_round.png";

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
                    _radioService.setIsMuted(IsMuted);//Używamy tego, aby przy zmianie dźwięku ustawić na Radiu że nie jest wyciszone
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
            }
        }
    }
    public string PlayPauseButtonImage => isPlaying
    ? "ms-appx:///Assets/Images/pause_round.png"
    : "ms-appx:///Assets/Images/play_round.png";

    public string MiniPlayPauseButtonImage => isPlaying
    ? "ms-appx:///Assets/Images/mini_pause.png"
    : "ms-appx:///Assets/Images/mini_play.png";

    public string? Title { get; }

    public IState<string> Name => State<string>.Value(this, () => string.Empty);

    public MainModel(
            IStringLocalizer localizer,
            IOptions<AppConfig> appInfo,
            INavigator navigator,
            IRadioPlayerService radioService,
            IRadioStationsService radioStationsService)
    {
        _navigator = navigator;
        NavigateCommand = new AsyncRelayCommand(GoToSecond);

        _radioService = radioService;

        ToggleMuteCommand = new RelayCommand(ToggleMute);
        PlayRadioCommand = new AsyncRelayCommand(() => PlayRadio(null));
        PlayStationCommand = new AsyncRelayCommand<RadioStation>(station => PlayRadio(station));
        Title = "Main";
        Title += $" - {localizer["ApplicationName"]}";
        Title += $" - {appInfo?.Value?.Environment}";

        _radioStationsService = radioStationsService;

        IsMuted = _radioService.GetIsMuted();
        isPlaying = _radioService.GetIsPlaying();
        _Volume = _radioService.GetVolume();

        _ = LoadStationsAsync();

    }

    private RadioStation _currentStation;
    public RadioStation CurrentStation
    {
        get => _currentStation;
        set
        {
            _currentStation = value;
            OnPropertyChanged();
        }
    }

    public string StationName => CurrentStation?.Name ?? "Brak stacji";
    public string StationCountry => CurrentStation?.Country ?? "";

   // Jedna metoda obsługująca oba przypadki
public async Task PlayRadio(RadioStation? station = null)
{
    if (station == null)
    {
        // Przypadek bezargumentowy - przełącz play/pause dla aktualnej stacji
        if (CurrentStation == null)
        {
            // Jeśli nie ma wybranej stacji, nie rób nic
            return;
        }
        
        // Przełącz stan odtwarzania dla aktualnej stacji
        isPlaying = !isPlaying;
        OnPropertyChanged(nameof(PlayPauseButtonImage));
        OnPropertyChanged(nameof(MiniPlayPauseButtonImage));
        
        // Odtwórz/zatrzymaj aktualną stację
        await _radioService.PlayOrPauseAsync(CurrentStation.StreamUrl);
    }
    else
    {
        // Przypadek z argumentem - wybór i odtwarzanie nowej stacji
        if (string.IsNullOrEmpty(station.StreamUrl))
            return;

        // Aktualizacja aktualnie wybranej stacji
        CurrentStation = station;
        
        // Odtwarzanie wybranej stacji
        await _radioService.PlayOrPauseAsync(station.StreamUrl);
        
        // Ustawienie stanu odtwarzania na true (zawsze odtwarzaj przy wyborze stacji)
        isPlaying = true;
        OnPropertyChanged(nameof(PlayPauseButtonImage));
        OnPropertyChanged(nameof(MiniPlayPauseButtonImage));
    }
    
    // Obsługa wyciszenia (wspólna dla obu przypadków)
    if (IsMuted && !_radioService.GetIsMuted())
    {
        _radioService.ToggleMute();
    }
    
    // Aktualizacja UI z informacjami o stacji
    OnPropertyChanged(nameof(StationName));
    OnPropertyChanged(nameof(StationCountry));
}

    public async Task GoToSecond()
    {
        var name = await Name;
        await _navigator.NavigateRouteAsync(this, "/Second");
    }

    private void ToggleMute()
    {
        IsMuted = !IsMuted;
        OnPropertyChanged(nameof(MuteButtonImage));
        _radioService.ToggleMute();
    }

    // zaladowanie stacji z json do programu
    private async Task LoadStationsAsync()
    {
        try
        {
            // Ścieżka do pliku JSON z listą stacji
            string filePath = Path.Combine(AppContext.BaseDirectory, "Assets", "Data", "stations.json");

            var stations = await _radioStationsService.LoadStationsAsync(filePath);

            // Czyszczenie kolekcji
            PopularStations.Clear();
            MostPlayedStations.Clear();

            // Podział stacji na kategorie
            foreach (var station in stations)
            {
                if (station.Category == "Popular")
                {
                    PopularStations.Add(station);
                }
                else if (station.Category == "MostPlayed")
                {
                    MostPlayedStations.Add(station);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas ładowania stacji: {ex.Message}");
        }
    }

}
