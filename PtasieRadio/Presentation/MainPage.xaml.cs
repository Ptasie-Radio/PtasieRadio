using System;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using Uno;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Devices.Radios;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace PtasieRadio.Presentation;

public sealed partial class MainPage : Page
{
    //TODO:
    //Jak mamy te zdjęcia to one mają działać jak przyciski, pojawiać się kolejne mają w nieskończoność
    //Zrób tak, że pobierana jest lista iluś tam tych naszych radii
    //Ja mam zrobić Animację jakoś żeby można było podłączyć
    //Dobra, ja mam zrobić tak, że ten minipage ma się robić na cały ekran

    //Dobra, w teori możemy zrobić taką tablicę gdzie będziemy przechowywać kilka stacji po przycisku, tylko ta nasza current by się odpalała
    //Ale to już na inny sprint
    private bool start;
    private bool test;
    private bool muted;
    private WaveOutEvent? waveOut;//Znak zapytania, aby warning nie dawało
    private MediaFoundationReader? reader;
    
    public MainPage()
    {  
        start = false;
        this.muted = false;
        test = true;//Zmienna test na czas pierwszego sprinta. Zmienić później na przycisk zmieniający radia
        this.InitializeComponent();
        this.SizeChanged += MainPage_SizeChanged; // element wymagany do debugowania rozmiaru okna
    }
    
    private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Width: {this.ActualWidth}, Height: {this.ActualHeight}");        
    }

private void ArrowTapped(object s, TappedRoutedEventArgs e)
{
    e.Handled = true;

    if(Microsoft.UI.Xaml.Window.Current == null)return;//Musi tak być, inaczej warning
    double windowHeight = Microsoft.UI.Xaml.Window.Current.Bounds.Height;

    double time = windowHeight > 300 ? 0.25:0.1;
    //Animacja przed zniknięciem
    var slideOutAnimation = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
    {
        From = 0,
        To = windowHeight,
        Duration = new Microsoft.UI.Xaml.Duration(TimeSpan.FromSeconds(time))
    };
    RadioMainPage.Visibility = Visibility.Visible;
    // Uruchom animację dla TranslateTransform
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

private void MiniPlayerTapped(object sender, TappedRoutedEventArgs e)
{
    e.Handled = true;

    RadioOverlay.Visibility = Visibility.Visible;
    
    if(Microsoft.UI.Xaml.Window.Current == null)return;//Musi tak być, inaczej warning
    double windowHeight = Microsoft.UI.Xaml.Window.Current.Bounds.Height;
    double time = windowHeight > 300 ? 0.25:0.1;

    RadioOverlayTransform.Y = windowHeight;
    var slideInAnimation = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
    {
        From = windowHeight,  // Pozycja z dołu
        To = 0,      // Końcowa Pozycja
        Duration = new Microsoft.UI.Xaml.Duration(TimeSpan.FromSeconds(time))  //Czas trwania
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

    private void ShuffleButtonTappedEvent(object sender, TappedRoutedEventArgs e)
    {
        e.Handled=true;
    }

    public async Task StopAudioAsync() // Dodane słowo kluczowe async
    {
        await Task.Run(() =>
        {
            waveOut?.Stop();
        });
    }
     private async void PlayPauseButtonTappedEvent(object sender, TappedRoutedEventArgs e)
	{

		e.Handled = true;

		if (sender is Button playButton)
		{
			playButton.IsEnabled = false;
			//string url = "https://playerservices.streamtheworld.com/api/livestream-redirect/WUAL_HD3.mp3";
			string url = "http://chi.cdn.eurozet.pl/chi-net.mp3";
			try
			{
				//Tutaj bierze zatrzymuje jeśli coś już nam gra, inaczej się psuło

				if (start == true)
				{
					//Zmieniamy obydwa przyciski, bo użyszkodnik może powiększyć w trakcie pauzy/playu
					PlayButtonImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/Images/play_round.png"));
					MiniPlayButtonImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/Images/mini_play.png"));
					await StopAudioAsync();
					start = false;
				}
				else
				{
					//Zmieniamy obydwa przyciski, bo użyszkodnik może powiększyć w trakcie pauzy/playu
					PlayButtonImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/Images/pause_round.png"));
					MiniPlayButtonImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/Images/mini_pause.png"));
					await Task.Run(
					() =>
					{
						//waveOut?.Dispose();//Przy dodaniu startowania z innych, umieścić to tam
						//reader?.Dispose();//To też. Na razie przy jednym nie potrzeba, ale przy więcej dodać

						if (test || waveOut == null || reader == null)//To trzeba dać do przycisku zmieniającego radia
						{
							reader = new MediaFoundationReader(url);
							waveOut = new WaveOutEvent();
							waveOut.Init(reader);
							test = false;
						}

						waveOut?.Play();
						start = true;
					});

				}

			}
			catch (Exception ex)
			{
				Console.WriteLine($"Błąd odtwarzania: {ex.Message}");
			}
			finally
			{
				playButton.IsEnabled = true;
			}
		}
	}

	private void HeartButtonTappedEvent(object sender, TappedRoutedEventArgs e)
     {
        e.Handled=true;
     }

    private void OnSoundLevelSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (this.waveOut is not null)
        {
            this.waveOut.Volume = (float)(e.NewValue / 100);
			MuteUnmuteButtonImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/Images/speaker_round.png"));
			this.muted = false;
		}
            
	}

    private void OnMuteUnmuteButtonClick(object sender, TappedRoutedEventArgs e)
    { 
        if (this.waveOut is not null)
        {
            if (this.muted == false)
            {
                this.waveOut.Volume = 0;
			    MuteUnmuteButtonImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/Images/speaker_muted_round.png"));
                this.muted = true;
            }
            else
            {
                this.waveOut.Volume = (float)(SoundLevelSlider.Value / 100);
				MuteUnmuteButtonImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/Images/speaker_round.png"));
				this.muted = false;
			}
	    }
    }
}