using System;
using System.Net;
using System.IO;
using NAudio.Wave;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno;

namespace PtasieRadio.Presentation;

public sealed partial class MainPage : Page
{

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

    public async Task StopAudioAsync()
{
    await Task.Run(() =>
    {
        waveOut?.Stop();
    });
}

    private async void RadioOnClick(object sender, RoutedEventArgs e)
    {
        playButton.IsEnabled = false;
        string url = "https://playerservices.streamtheworld.com/api/livestream-redirect/WUAL_HD3.mp3";

        try
        {
            //Tutaj bierze zatrzymuje jeśli coś już nam gra, inaczej się psuło

           
            if(start == true) 
            {
                playButtonImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/play.png"));
                await StopAudioAsync();
                start = false;
            }
            else
            {
                playButtonImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/pause.png"));
                await Task.Run(//Tutaj się robi osobny wątek, dzięki czemu nie wiesza całej aplikacji
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
