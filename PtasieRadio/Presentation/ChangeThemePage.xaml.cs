using Microsoft.UI.Xaml.Controls;

namespace PtasieRadio.Presentation;

public sealed partial class ChangeThemePage : Page
{
  public ChangeThemePage()
  {
    this.InitializeComponent();
  }
  private void GoBack_Click(object sender, RoutedEventArgs e)
  {
    if (Frame.CanGoBack)
    {
      Frame.GoBack();
    }
  }
        
}
