using Microsoft.UI.Xaml.Controls;
using PtasieRadio.Services;

namespace PtasieRadio.Presentation;

public sealed partial class ChangeThemePage : Page
{
  public ChangeThemePage()
  {
    this.InitializeComponent();
  }
  private void SaveScrollPosition()
  {
      _savedScrollOffset = MainScrollViewer.VerticalOffset;
      ApplicationData.Current.LocalSettings.Values["SavedScroll"] = _savedScrollOffset;
  }
  // save scroll position żeby na małych ekranach nie mogło przewijać do góry:podczas zmiany motywu
  private void CheckBox_Click(object sender, RoutedEventArgs e)
  {
    SaveScrollPosition();
    var cb = sender as CheckBox;
    // jeśli kliknięto zaznaczony już motyw – NIE pozwól go odznaczyć:
    if (cb.IsChecked == false)
      cb.IsChecked = true;
  }

private double _savedScrollOffset = 0;



protected override void OnNavigatedTo(NavigationEventArgs e)
{
    base.OnNavigatedTo(e);

    MainScrollViewer.Loaded += (s, args) =>
    {
        if (ApplicationData.Current.LocalSettings.Values.TryGetValue("SavedScroll", out var saved) && saved is double offset)
        {
            MainScrollViewer.ChangeView(null, offset, null);
        }
    };
}


}
