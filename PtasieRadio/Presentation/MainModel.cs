using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Uno.Extensions.Navigation;
using Uno.Extensions.Reactive;
using PtasieRadio.Services.RadioService;
using CommunityToolkit.Mvvm.ComponentModel;


namespace PtasieRadio.Presentation;

public partial class MainModel : ObservableObject
{
    private INavigator _navigator;
    public IAsyncRelayCommand NavigateCommand { get; }//Tworzenie komendy nawigacyjnej
    public IAsyncRelayCommand PlayRadioCommand { get; }
    private readonly IRadioPlayerService _radioService;

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

                if(IsMuted)
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
            IRadioPlayerService radioService)
    {
        _navigator = navigator;
        NavigateCommand = new AsyncRelayCommand(GoToSecond);

        _radioService = radioService;
        ToggleMuteCommand = new RelayCommand(ToggleMute);
        PlayRadioCommand = new AsyncRelayCommand(PlayRadio);
        Title = "Main";
        Title += $" - {localizer["ApplicationName"]}";
        Title += $" - {appInfo?.Value?.Environment}";


        IsMuted = _radioService.GetIsMuted();
        isPlaying = _radioService.GetIsPlaying();
        _Volume = _radioService.GetVolume();
    }

    public async Task PlayRadio()
    {
        isPlaying = !isPlaying;
        OnPropertyChanged(nameof(PlayPauseButtonImage));
        OnPropertyChanged(nameof(MiniPlayPauseButtonImage));
        string url = "http://chi.cdn.eurozet.pl/chi-net.mp3";//Później się to da jako zmienną
        await _radioService.PlayOrPauseAsync(url);
        if(IsMuted && !_radioService.GetIsMuted())//Nie jestem w 100% pewien czemu to działa, ale działa
        {
            _radioService.ToggleMute();
        }
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

}