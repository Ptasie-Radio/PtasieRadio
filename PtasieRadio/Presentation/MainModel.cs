using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Uno.Extensions.Navigation;
using Uno.Extensions.Reactive;
using PtasieRadio.Services.RadioService;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Data;

namespace PtasieRadio.Presentation;

[Bindable]

public class MainModel : ObservableObject
{
    private INavigator _navigator;
    public IAsyncRelayCommand NavigateCommand { get; }//Tworzenie komendy nawigacyjnej
    public IAsyncRelayCommand PlayRadioCommand { get; }
    private readonly IRadioPlayerService _radioService;
    private string? url;

    public IRelayCommand ToggleMuteCommand { get; }
    public RelayCommand<string?> ToggleChangeUrlCommand { get; }

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
        url = _radioService.GetUrl();
        if(url == null)url = "http://chi.cdn.eurozet.pl/chi-net.mp3";//Początkowa wartość url. Możemy zrobić że minipage się wyświetla na dole dopiero po wybraniu radia (Problemem może być to tylko wtedy, gdy żadnego radia nie będzie do wyboru)
        
        ToggleMuteCommand = new RelayCommand(ToggleMute);
        ToggleChangeUrlCommand = new RelayCommand<string?>(ToggleChangeUrl);
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
        if (url == null) url = "";
        if (_radioService.GetUrl() == null) _radioService.SetUrl(url);
        await _radioService.PlayOrPauseAsync();
        if (IsMuted && !_radioService.GetIsMuted())
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

    private void ToggleChangeUrl(string? url)
    {
        if (url == null) url = "";
        this.url = url;
        _radioService.Reset();
        _radioService.SetUrl(url);
        OnPropertyChanged(nameof(MuteButtonImage));
        OnPropertyChanged(nameof(PlayPauseButtonImage));
        OnPropertyChanged(nameof(MiniPlayPauseButtonImage));
        IsMuted = _radioService.GetIsMuted();
        isPlaying = _radioService.GetIsPlaying();
        _Volume = _radioService.GetVolume();
    }

}