using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace PtasieRadio.Presentation;


public sealed partial class SecondPage : Page
{
    public SecondPage()
    {
        this.InitializeComponent();
    }
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (DataContext is SecondModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }

    // byc moze pozniej rusze by dzialalo
    // private void ProfileButton_Click(object sender, RoutedEventArgs e)
    // {
    //     var viewModel = DataContext as SecondModel;
    //     if (viewModel == null)
    //     {
    //         // Możesz wyświetlić komunikat "Brak profili"
    //         System.Diagnostics.Debug.WriteLine("brak profili");
    //         return;
    //     }

    //     viewModel.ProfileList.Clear();
    //     foreach (var kvp in viewModel._profileService.Profiles)
    //         viewModel.ProfileList.Add(kvp);

    //     var menuFlyout = new MenuFlyout();
    //     foreach (var profile in viewModel.ProfileList)
    //     {
    //         var item = new MenuFlyoutItem
    //         {
    //             Text = profile.Value.Name,
    //             Command = viewModel.SelectProfileCommand,
    //             CommandParameter = profile.Key
    //         };
    //         menuFlyout.Items.Add(item);
    //     }
    //     menuFlyout.ShowAt(ProfileButton);
    //     //  _ = this.Navigator()?.NavigateViewAsync<SecondPage>(this, qualifier: Qualifiers.Dialog); // nie dziala
    // }

    private void OnEditUserTapped(object sender, TappedRoutedEventArgs e)
    {
        var viewModel = DataContext as SecondViewModel;
        if (viewModel == null) return;
        viewModel.NavigateToUserCommand.Execute(null);
    }

    // private void OnAddRadioTapped(object sender, TappedRoutedEventArgs e)


private void NavigateToChangeTheme(object sender, TappedRoutedEventArgs e)
{
    var viewModel = DataContext as SecondViewModel;
    if (viewModel == null) return;
    viewModel.GoToChangeTheme.Execute(null);
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


