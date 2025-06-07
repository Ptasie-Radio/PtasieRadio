using Microsoft.UI.Xaml.Controls;
using PtasieRadio.Services;

namespace PtasieRadio.Presentation;

public sealed partial class ChangeThemePage : Page
{
  public ChangeThemePage()
  {
    this.InitializeComponent();
  }

private void CheckBox_Click(object sender, RoutedEventArgs e)
{
    var cb = sender as CheckBox;
    // jeśli kliknięto zaznaczony już motyw – NIE pozwól go odznaczyć:
    if (cb.IsChecked == false)
        cb.IsChecked = true;
}     
}
