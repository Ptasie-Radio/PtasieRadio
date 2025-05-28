using System;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using Uno;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.System;
using Windows.Devices.Radios;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

namespace PtasieRadio.Presentation;


public sealed partial class SecondPage : Page
{
    public SecondPage()
    {
        this.InitializeComponent();
    }

    private void AddNewRadioNavigate(object sender, TappedRoutedEventArgs e)
    {
        var viewModel = DataContext as SecondViewModel;
        if (viewModel == null) return;
        viewModel.NavigateToAddRadioCommand.Execute(null);
    }

        private async void SaveFolderTapped(object sender, TappedRoutedEventArgs e)
    {
        var folderName = "PtasieRadio";
        var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(
		folderName, CreationCollisionOption.OpenIfExists);
			System.Diagnostics.Process.Start("explorer.exe", folder.Path);
    }
        //PRZYCISK USTAWIENIA - > INNE
    private async void OnOpiniaTapped(object sender, TappedRoutedEventArgs e)
    {
        var uri = new Uri("https://docs.google.com/forms/d/e/1FAIpQLSduEc6xafNra5ygl1m6dShfAwOl6oSdzMcyGw1p6q178Ya_bw/viewform");
        await Launcher.LaunchUriAsync(uri);
    }
    private void ShowPopup(string tytul, string tresc)
    {
        PopupTytul.Text = tytul;
        PopupTresc.Text = tresc;

        // Ręcznie przypisz wymiary zanim otworzysz popup
        PopupOverlay.Width = this.ActualWidth;
        PopupOverlay.Height = this.ActualHeight;
        PopupOverlay.UpdateLayout(); // wymuś aktualizację

        PopupInformacyjny.IsOpen = true;
        this.SizeChanged += Popup_SizeChanged;
    }

    private void Popup_BackgroundTapped(object sender, TappedRoutedEventArgs e)
    {
        PopupInformacyjny.IsOpen = false;
        this.SizeChanged -= Popup_SizeChanged;
    }

    private void Popup_ContentTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
    }

    private void Popup_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (PopupOverlay != null)
        {
            PopupOverlay.Width = this.ActualWidth;
            PopupOverlay.Height = this.ActualHeight;
            PopupOverlay.UpdateLayout();
        }
    }
    //TRESC POPUPÓW
    private void OnONasTapped(object sender, TappedRoutedEventArgs e)
    {
        ShowPopup("O nas",
            "PtasieRadio to aplikacja stworzona przez studentów w ramach projektu uczelnianego. \n" +
            "Umożliwia słuchanie stacji radiowych, dodawanie własnych linków, zmianę kolorów interfejsu, ikon oraz nazwy użytkownika. \n" +
            "Dziękujemy, że z niej korzystasz! ❤️");
    }



    private void OnPolitykaTapped(object sender, TappedRoutedEventArgs e)
    {
        ShowPopup("Polityka Prywatności",
            "Aplikacja PtasieRadio nie gromadzi żadnych danych osobowych użytkownika. \n" +
            "Nie używamy ciasteczek, nie logujemy, nie zapisujemy adresów IP. Wszystkie ustawienia i dane (np. dodane stacje radiowe) są przechowywane wyłącznie lokalnie na urządzeniu użytkownika.");
    }
}


