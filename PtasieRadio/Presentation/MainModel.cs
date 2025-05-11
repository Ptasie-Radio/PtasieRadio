using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;

namespace PtasieRadio.Presentation;

public partial record MainModel
{
    private INavigator _navigator;

    private Point _lastPointerPosition;
    private ScrollViewer? _currentScrollViewer;

    public IRelayCommand<PointerRoutedEventArgs> ScrollStartCommand { get; }
    public IRelayCommand<PointerRoutedEventArgs> ScrollMoveCommand { get; }
    public RelayCommand<PointerRoutedEventArgs> ScrollStopCommand { get; }

    public MainModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        INavigator navigator)
    {
        _navigator = navigator;
        Title = "Main";
        Title += $" - {localizer["ApplicationName"]}";
        Title += $" - {appInfo?.Value?.Environment}";

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
    public string? Title { get; }

    public IState<string> Name => State<string>.Value(this, () => string.Empty);

    public async Task GoToSecond()
    {
        var name = await Name;
        await _navigator.NavigateViewModelAsync<SecondModel>(this, data: new Entity(name!));
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
