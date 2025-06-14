using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using NAudio.Wave;
using Newtonsoft.Json;
using PtasieRadio.Services.AddRadioService;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Uno;
using Windows.Devices.Radios;
using Microsoft.UI.Dispatching;

namespace PtasieRadio.Presentation;

public sealed partial class MainPage : Page
{
    private double pageAnimationTime;
    private Dictionary<string, StackPanel> stationPanels = new();
    private string? currentIndex;
    private const string folderName = "PtasieRadio";

    public MainPage()
    {
        this.pageAnimationTime = 0.4;
        this.InitializeComponent();
        this.SizeChanged += MainPage_SizeChanged;
        _ = CreateOwnStationOnViewLoad();
    }
    
    private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        double windowHeight = this.ActualHeight;

        // Parametry skalowania
        double minWindow = 400;  // Minimalna wysokość okna, od której zaczyna się skalowanie
        double maxWindow = 1000; // Maksymalna wysokość okna, powyżej której slider już nie rośnie
        double minSlider = 60;  // Minimalna wysokość slidera
        double maxSlider = 200;  // Maksymalna wysokość slidera

        // Interpolacja liniowa
        double t = (windowHeight - minWindow) / (maxWindow - minWindow);
        t = Math.Clamp(t, 0, 1); // Ograniczamy do zakresu 0-1

        double sliderHeight = minSlider + (maxSlider - minSlider) * t;

        SoundLevelSlider.Height = sliderHeight;
        System.Diagnostics.Debug.WriteLine($"Width: {this.ActualWidth}, Height: {this.ActualHeight}, Slider {sliderHeight}");
    }


    private void ArrowTapped(object s, TappedRoutedEventArgs e)
    {
        e.Handled = true;

        if (Microsoft.UI.Xaml.Window.Current == null) return;//Musi tak być, inaczej warning
        double windowHeight = Microsoft.UI.Xaml.Window.Current.Bounds.Height;

        double time = pageAnimationTime;
        var slideOutAnimation = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
        {
            From = 0,
            To = windowHeight,
            Duration = new Microsoft.UI.Xaml.Duration(TimeSpan.FromSeconds(time)),
            EasingFunction = new CircleEase
            {
                EasingMode = EasingMode.EaseInOut
            }
        };
        RadioMainPage.Visibility = Visibility.Visible;

        Storyboard.SetTarget(slideOutAnimation, RadioOverlayTransform);
        Storyboard.SetTargetProperty(slideOutAnimation, "Y");

        var storyboard = new Storyboard();
        storyboard.Children.Add(slideOutAnimation);
        storyboard.Completed += (sender, args) =>
        {
            RadioOverlay.Visibility = Visibility.Collapsed;
        };

        storyboard.Begin();
    }

    private void RackTappedEvent(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
    }

    private void MiniPlayerTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;

        RadioOverlay.Visibility = Visibility.Visible;

        if (Microsoft.UI.Xaml.Window.Current == null) return;//Musi tak być, inaczej warning
        double windowHeight = Microsoft.UI.Xaml.Window.Current.Bounds.Height;
        double time = pageAnimationTime;

        RadioOverlayTransform.Y = windowHeight;
        var slideInAnimation = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
        {
            From = windowHeight,
            To = 0,
            Duration = new Microsoft.UI.Xaml.Duration(TimeSpan.FromSeconds(time)),
            EasingFunction = new CircleEase
            {
                EasingMode = EasingMode.EaseInOut
            }
        };

        Storyboard.SetTarget(slideInAnimation, RadioOverlayTransform);
        Storyboard.SetTargetProperty(slideInAnimation, "Y");

        var storyboard = new Storyboard();
        storyboard.Children.Add(slideInAnimation);
        storyboard.Begin();
        storyboard.Completed += (sender, args) =>
        {
            RadioMainPage.Visibility = Visibility.Collapsed;
        };
    }

    private void OnTabTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is StackPanel panel && panel.Tag is string selected)
        {
            System.Diagnostics.Debug.WriteLine($"Kliknięto: {selected}");
        }
    }

    private async void ShuffleButtonTappedEvent(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        var folder = await OpenFolder();
        var entries = await LoadFromJson(folder, "GetRandomIndex");
        var entry = entries.First();
        System.Diagnostics.Debug.WriteLine($"Entries: {entry.Key}");

        if (stationPanels.TryGetValue(entry.Key, out var targetPanel))
        {
        	OnPanelTapped(targetPanel, null);
        }

    }

    private void PlayPauseButtonTappedEvent(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        
    }

    private void HeartButtonTappedEvent(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
    }

    private void OnSoundLevelSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        var viewModel = DataContext as MainModel;

        if (viewModel == null) return;

        viewModel.Volume = e.NewValue;
    }

    public static async Task<Dictionary<string, SaveEntryData>> LoadFromJson(StorageFolder folder, string text="")
	{
		var localFileName = "radio.json";
		Dictionary<string, SaveEntryData> entries;
		Dictionary<string, SaveEntryData> filteredEntries;
		try
		{

			var file = await folder.GetFileAsync(localFileName);
			string json = await FileIO.ReadTextAsync(file);
			entries = JsonConvert.DeserializeObject<Dictionary<string, SaveEntryData>>(json)
					  ?? new Dictionary<string, SaveEntryData>();
            if (text == "GetRandomIndex")
            {
                var random = new Random();
                int index = random.Next(entries.Count);
                var randomEntry = entries.ElementAt(index);
                filteredEntries = new Dictionary<string, SaveEntryData>
                {
                    { randomEntry.Key, randomEntry.Value }
                };
            }
            else if (text == "NO")
            {
                filteredEntries = entries
                .OrderByDescending(entry => entry.Value.NumberOfTimesPlayed)
                .Where(entry => entry.Value.NumberOfTimesPlayed > 0)
                .Take(10)
                .ToDictionary(entry => entry.Key, entry => entry.Value);
            }
            else if (text != "") filteredEntries = entries.Where(kv => kv.Value.Category == text).ToDictionary(kv => kv.Key, kv => kv.Value);
            else filteredEntries = entries;
		}
		catch (FileNotFoundException)
		{
			filteredEntries = new Dictionary<string, SaveEntryData>();
		}

        return filteredEntries;
    }


    public static async Task RemoveEntryById(StorageFolder folder, string id)
    {
        var localFileName = "radio.json";
        var userFileName = "../users.json";
        try
        {
            var file = await folder.GetFileAsync(localFileName);
            string json = await FileIO.ReadTextAsync(file);

            var entries = JsonConvert.DeserializeObject<Dictionary<string, SaveEntryData>>(json)
                          ?? new Dictionary<string, SaveEntryData>();

            var fileUser = await folder.GetFileAsync(userFileName);
            string jsonUser = await FileIO.ReadTextAsync(fileUser);
            var userData = JsonConvert.DeserializeObject<Dictionary<string, User>>(jsonUser)
                          ?? new Dictionary<string, User>();

            if (entries.ContainsKey(id))
            {
                entries.Remove(id);
                string newJson = JsonConvert.SerializeObject(entries, Formatting.Indented);
                await FileIO.WriteTextAsync(file, newJson);
            }
            // dla czyszczenia profilu
            bool changed = false;
            foreach (var user in userData.Values)
            {
                // Jeśli chcesz też z UserRadioStationKeys:
                if (user.UserRadioStationKeys.Contains(id))
                {
                    user.UserRadioStationKeys.Remove(id);
                    changed = true;
                }
            }
            if (changed)
            {
                string newJson = JsonConvert.SerializeObject(userData, Formatting.Indented);
                await FileIO.WriteTextAsync(fileUser, newJson);
            }
        }
        catch (FileNotFoundException)
        {
            System.Diagnostics.Debug.WriteLine($"Exception: Nie udało się znaleźć pliku");
        }
    }

    public static async Task<StorageFolder> OpenFolder() =>
        await ApplicationData.Current.LocalFolder.CreateFolderAsync(
            folderName, CreationCollisionOption.OpenIfExists);

    private async Task CreateOwnStationOnViewLoad()
    {
        var x = new List<string> { "NO", "POP", "Własne", "" }; // ulubione sa dla poprawnosci dzialania tego kodu nie znajdziemy tej opcji w radio.json
        int j = 0;
        using (await AddRadioService.jsonSemaphore.Lock())
        {
            foreach (StackPanel panel in new List<StackPanel> { NajczesciejGranePanel, PopularnePanel, WlasnePanel, UlubionePanel })
            {
                var folder = await OpenFolder();
                var entries = await LoadFromJson(folder, x[j]);
                if (x[j] == "Własne")
                {
                    var viewModel = DataContext as MainModel;
                    if (viewModel == null) return;
                    // Pobierz aktualny profil użytkownika
                    var currentProfileKey = viewModel._profileService.SelectedKey;
                    if (currentProfileKey != null && viewModel._profileService.Profiles.TryGetValue(currentProfileKey, out var userProfile))
                    {
                        // Filtruj tylko stacje z kluczami użytkownika
                        entries = entries
                            .Where(entry => userProfile.UserRadioStationKeys.Contains(entry.Key))
                            .ToDictionary(kv => kv.Key, kv => kv.Value);
                    }
                    else
                    {
                        entries = new Dictionary<string, SaveEntryData>();
                    }
                }
                else if (x[j] == "")
                {
                    var viewModel = DataContext as MainModel;
                    if (viewModel == null) return;
                    // Pobierz aktualny profil użytkownika
                    var currentProfileKey = viewModel._profileService.SelectedKey;
                    if (currentProfileKey != null && viewModel._profileService.Profiles.TryGetValue(currentProfileKey, out var userProfile))
                    {
                        // Filtruj tylko stacje z kluczami użytkownika
                        entries = entries
                            .Where(entry => userProfile.FavoriteStationIds.Contains(entry.Key))
                            .ToDictionary(kv => kv.Key, kv => kv.Value);

                    }
                    else
                    {
                        entries = new Dictionary<string, SaveEntryData>();
                    }
                }
                await AddStation(folder, entries, panel);
                j++;
            }
        }
    }
    private void FilterVisibleStations(string searchText)
    {
        var categoryPanels = new List<StackPanel>
    {
        NajczesciejGranePanel,
        PopularnePanel,
        WlasnePanel
    };

        foreach (var categoryPanel in categoryPanels)
        {
            FilterStationsInCategory(categoryPanel, searchText);
        }
    }

    private void FilterStationsInCategory(StackPanel categoryPanel, string searchText)
    {
        foreach (var child in categoryPanel.Children)
        {
            if (child is StackPanel stationPanel && stationPanel.Name.StartsWith("Stacja_"))
            {
                // Znajdujemy TextBlock z nazwą stacji (drugi element w StackPanel)
                var textBlock = stationPanel.Children.OfType<TextBlock>().FirstOrDefault();

                if (textBlock != null)
                {
                    string stationName = textBlock.Text.ToLower();

                    // Pokazujemy/ukrywamy stację na podstawie dopasowania
                    bool isMatch = string.IsNullOrEmpty(searchText) ||
                              stationName.Contains(searchText) ||
                              FuzzySearch.CalculateSimilarity(stationName, searchText) >= 0.4;

                    if (isMatch)
                        stationPanel.Visibility = Visibility.Visible;
                    else
                        stationPanel.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
    private void SearchAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            string searchText = sender.Text.ToLower().Trim();

            // Filtrowanie widocznych stacji
            FilterVisibleStations(searchText);

            // Opcjonalnie: tworzenie podpowiedzi
            if (!string.IsNullOrEmpty(searchText))
            {
                var suggestions = GetMatchingStationNames(searchText);
                sender.ItemsSource = suggestions.Take(5); // Maksymalnie 5 podpowiedzi
            }
            else
            {
                sender.ItemsSource = null;
            }
        }
    }

    private List<string> GetMatchingStationNames(string searchText)
    {
        var matchingNames = new List<string>();
        var categoryPanels = new List<StackPanel> { NajczesciejGranePanel, PopularnePanel, WlasnePanel, UlubionePanel };

        foreach (var categoryPanel in categoryPanels)
        {
            foreach (var child in categoryPanel.Children)
            {
                if (child is StackPanel stationPanel && stationPanel.Name.StartsWith("Stacja_"))
                {
                    var textBlock = stationPanel.Children.OfType<TextBlock>().FirstOrDefault();
                    if (textBlock != null)
                    {
                        string stationName = textBlock.Text;
                        if (stationName.ToLower().Contains(searchText))
                        {
                            matchingNames.Add(stationName);
                        }
                    }
                }
            }
        }

        return matchingNames.Distinct().OrderBy(x => x).ToList();
    }

    private async Task AddStation(StorageFolder folder, Dictionary<string, SaveEntryData> entries, StackPanel category)
    {
        var files = await folder.GetFilesAsync();
        var dispatcher = DispatcherQueue.GetForCurrentThread();
        foreach (var entry in entries)
        {
            BitmapImage bitmap = new BitmapImage(new Uri("ms-appx:///Assets/Images/radio_placeholder_square"));

            try
            {
                var file = await StorageFile.GetFileFromPathAsync(entry.Value.ImagePath);
                using var stream = await file.OpenAsync(FileAccessMode.Read);

                bitmap = new BitmapImage();
                await bitmap.SetSourceAsync(stream);

                CreateStationPanel(entry, category, folder, bitmap);
            }
            catch (Exception)
            {
                bitmap = new BitmapImage(new Uri("ms-appx:///Assets/Images/radio_placeholder_square"));
                CreateStationPanel(entry, category, folder, bitmap);
            }
        }
    }

    private void CreateStationPanel(KeyValuePair<string, SaveEntryData> entry, StackPanel category, StorageFolder folder, BitmapImage bitmap)
    {
        var image = new Image
        {
            Width = 120,
            Height = 130,
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Source = bitmap
        };

        var border = new Border
        {
            Width = 100,
            Height = 100,
            Background = new SolidColorBrush(Windows.UI.ColorHelper.FromArgb(255, 224, 224, 224)),
            CornerRadius = new CornerRadius(20),
            Child = image
        };

        var textBlock = new TextBlock
        {
            Text = entry.Value.Name,
            TextAlignment = TextAlignment.Center,
            FontSize = 12,
            Foreground = (Brush)Application.Current.Resources["TextColor"],
        };

        var stationPanel = new StackPanel
        {
            Width = 100,
            Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
            Spacing = 5,
            Name = $"Stacja_{entry.Key}",
            ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.System,
        };

        var targetPanels = new HashSet<StackPanel> { NajczesciejGranePanel, WlasnePanel, UlubionePanel };
        if (targetPanels.Contains(category))
        {
            var menu = new MenuFlyoutItem { Text = "Usuń" };
            menu.Click += async (s, e) =>
            {
                string id = stationPanel.Name.Replace("Stacja_", "");
                await RemoveEntryById(folder, id);
                var parent = stationPanel.Parent as Panel;
                parent?.Children.Remove(stationPanel);
            };
            var menuFlyout = new MenuFlyout();
            menuFlyout.Items.Add(menu);
            stationPanel.ContextFlyout = menuFlyout;
        }

        stationPanel.Tapped += OnPanelTapped;
        stationPanel.Children.Add(border);
        stationPanel.Children.Add(textBlock);
        category.Children.Add(stationPanel);
        stationPanels[entry.Key] = stationPanel;
    }
    private async void OnPanelTapped(object sender, TappedRoutedEventArgs e)
    {

        if (sender is StackPanel panel)
        {
            string index = panel.Name.Replace("Stacja_", "");
            if (currentIndex == index) return;//Nie pozwalamy na to, aby użytkownik spamował na ten sam przycisk.
            var folder = await OpenFolder();
            var entries = await LoadFromJson(folder);

            if (entries.TryGetValue(index, out var entry))
            {
                var viewModel = DataContext as MainModel;
                if (viewModel == null) return;

                entry.NumberOfTimesPlayed += 1;

                await AddRadioService.SaveToJson(folder, entries);

                currentIndex = index;
                viewModel.ToggleChangeUrlCommand.Execute(entry.StreamUrl);
                MiniPlayer.Visibility = Visibility.Visible;
                viewModel.ToggleChangeStationNameCommand.Execute(entry.Name);
                viewModel.ToggleChangeCountryCommand.Execute(entry.Country);
                viewModel.ToggleChangeImagePathCommand.Execute(entry.ImagePath);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Nie udało się");
            }
        }
    }
}