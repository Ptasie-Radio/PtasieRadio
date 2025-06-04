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
using System.Threading.Tasks;

namespace PtasieRadio.Presentation;

[Bindable]

public class MainModel : ObservableObject
{
    private INavigator _navigator;
    public IAsyncRelayCommand NavigateCommand { get; }//Tworzenie komendy nawigacyjnej
    public IAsyncRelayCommand PlayRadioCommand { get; }
    private readonly IRadioPlayerService _radioService;
    private readonly IShowPromptService _promptService;
    private string? url;
    private bool isChangingUrl = false;
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
        IRadioPlayerService radioService,
        IShowPromptService promptService)
    {
        _navigator = navigator;
        NavigateCommand = new AsyncRelayCommand(GoToSecond);

        _radioService = radioService;
        _promptService = promptService;
        url = _radioService.GetUrl();
        if(url == null)url = "http://chi.cdn.eurozet.pl/chi-net.mp3";
        
        ToggleMuteCommand = new RelayCommand(ToggleMute);
        ToggleChangeUrlCommand = new AsyncRelayCommand<string?>(ToggleChangeUrl);
        PlayRadioCommand = new AsyncRelayCommand(PlayRadio);
        Title = "Main";
        Title += $" - {localizer["ApplicationName"]}";
        Title += $" - {appInfo?.Value?.Environment}";

        IsMuted = _radioService.GetIsMuted();
        isPlaying = _radioService.GetIsPlaying();
        _Volume = _radioService.GetVolume();

        ScrollStartCommand = new RelayCommand<PointerRoutedEventArgs>(ScrollStart);
        ScrollMoveCommand = new RelayCommand<PointerRoutedEventArgs>(ScrollMove);
        ScrollStopCommand = new RelayCommand<PointerRoutedEventArgs>(ScrollStop);
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
        var name = await Name;
        await _navigator.NavigateRouteAsync(this, "/Second");
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
