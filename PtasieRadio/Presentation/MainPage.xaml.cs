using System;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using Uno;
using Newtonsoft.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Devices.Radios;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Imaging;
using PtasieRadio.Services.AddRadioService;


namespace PtasieRadio.Presentation;

public sealed partial class MainPage : Page
{
    private double pageAnimationTime;

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
        //Animacja przed zniknięciem
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


    private void ShuffleButtonTappedEvent(object sender, TappedRoutedEventArgs e)
    {
        //TODO:
        //Dodaj tutaj shuffl'a
        e.Handled = true;
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
			if (text != "") filteredEntries = entries.Where(kv => kv.Value.Category == text).ToDictionary(kv => kv.Key, kv => kv.Value);
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
        try
        {
            var file = await folder.GetFileAsync(localFileName);
            string json = await FileIO.ReadTextAsync(file);

            var entries = JsonConvert.DeserializeObject<Dictionary<string, SaveEntryData>>(json)
                          ?? new Dictionary<string, SaveEntryData>();

            if (entries.ContainsKey(id))
            {
                entries.Remove(id);
                string newJson = JsonConvert.SerializeObject(entries, Formatting.Indented);
                await FileIO.WriteTextAsync(file, newJson);
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
        var x = new List<string> { "NO", "POP", "Własne" };
        int j = 0;
        using (await AddRadioService.jsonSemaphore.Lock())
        {
		    foreach (StackPanel panel in new List<StackPanel> {PopularnePanel,NajczesciejGranePanel,  WlasnePanel})
            {
                var folder = await OpenFolder();
                var entries = await LoadFromJson(folder, x[j]);
                await AddStation(folder, entries, panel);
                j++;
            }
        }
    }

    private async Task AddStation(StorageFolder folder, Dictionary<string, SaveEntryData> entries, StackPanel category)
    {
        var files = await folder.GetFilesAsync();
        foreach (var entry in entries)
        {
            BitmapImage bitmap;
            StorageFile? file;

            try
            {
                file = await StorageFile.GetFileFromPathAsync(entry.Value.ImagePath);
            }
            catch (FileNotFoundException)
            {
                file = null;
            }
            try
            {
                using (var stream = new MemoryStream(File.ReadAllBytes(file?.Path ?? "Brak")))
                {
                    bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(stream);
                }

            }
            catch (FileNotFoundException)
            {
                bitmap = new BitmapImage(new Uri("ms-appx:///Assets/Images/radio_placeholder_square"));
            }
            catch (System.NullReferenceException)
            {
                bitmap = new BitmapImage(new Uri("ms-appx:///Assets/Images/radio_placeholder_square"));
            }

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

            var targetPanels = new HashSet<StackPanel> { NajczesciejGranePanel, WlasnePanel };
            if (targetPanels.Contains(category))
            {
                var menu = new MenuFlyoutItem { Text = "Usuń" };
                menu.Click += async (s, e) =>
                {
                    string id = stationPanel.Name.Replace("Stacja_", "");
                    await RemoveEntryById(folder, id);
                    var parent = stationPanel.Parent as Panel;
                    parent?.Children.Remove(stationPanel);//Usuwamy ten nasz station Panel
                };
                var menuFlyout = new MenuFlyout();
                menuFlyout.Items.Add(menu);
                stationPanel.ContextFlyout = menuFlyout;
            }
            stationPanel.Tapped += OnPanelTapped;
            stationPanel.Children.Add(border);
            stationPanel.Children.Add(textBlock);
            category.Children.Add(stationPanel);
        }
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
                currentIndex = index;
                viewModel.ToggleChangeUrlCommand.Execute(entry.StreamUrl);
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