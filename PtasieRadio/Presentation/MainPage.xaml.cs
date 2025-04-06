using System;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Controls;
using Uno;

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
    private WaveOutEvent? waveOut;//Znak zapytania, aby warning nie dawało
    private MediaFoundationReader? reader;
    
    public MainPage()
    {  
        start = false;
        test = true;//Zmienna test na czas pierwszego sprinta. Zmienić później na przycisk zmieniający radia
        this.InitializeComponent();
    }
    
    public async Task StopAudioAsync() // Dodane słowo kluczowe async
    {
        await Task.Run(() =>
        {
            waveOut?.Stop();
        });
    }

    private void MiniPlayerTapped(object sender, TappedRoutedEventArgs e)
    {
        MiniPlayer.Visibility = Visibility.Collapsed;
    }


     private async void PlayPauseButtonTappedEvent(object sender, TappedRoutedEventArgs e)
     {
        
        e.Handled=true;
        playButton.IsEnabled = false;
        //string url = "https://playerservices.streamtheworld.com/api/livestream-redirect/WUAL_HD3.mp3";
        string url = "http://chi.cdn.eurozet.pl/chi-net.mp3";
        try
        {
            //Tutaj bierze zatrzymuje jeśli coś już nam gra, inaczej się psuło
          
            if(start == true) 
            {
                playButtonImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/Images/mini_play.png"));
                await StopAudioAsync();
                start = false;
            }
            else
            {
                playButtonImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/Images/mini_pause.png"));
                await Task.Run(
                () =>
                {
                    //waveOut?.Dispose();//Przy dodaniu startowania z innych, umieścić to tam
                    //reader?.Dispose();//To też. Na razie przy jednym nie potrzeba, ale przy więcej dodać
                   
                    if(test || waveOut == null || reader == null)//To trzeba dać do przycisku zmieniającego radia
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

