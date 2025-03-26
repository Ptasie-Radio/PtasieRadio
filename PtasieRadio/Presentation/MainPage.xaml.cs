using System;
using System.Net;
using System.IO;
using NAudio.Wave;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PtasieRadio.Presentation;

public sealed partial class MainPage : Page
{
    private WaveOutEvent waveOut;
    private MediaFoundationReader reader;
    public MainPage()
    {
        this.InitializeComponent();
    }

    public async Task StopAudioAsync()
{
    await Task.Run(() =>
    {
        if (waveOut?.PlaybackState != PlaybackState.Stopped)
        {
            waveOut?.Stop();
            waveOut?.Dispose();
            reader?.Dispose();
        }
    });
}

    private async void radioOnClick(object sender, RoutedEventArgs e)
    {
        string url = "https://playerservices.streamtheworld.com/api/livestream-redirect/WUAL_HD3.mp3";

        try
        {
            //Tutaj bierze zatrzymuje jeśli coś już nam gra, inaczej się psuło

            await StopAudioAsync();//
            
            await Task.Run(//Tutaj się robi osobny wątek, dzięki czemu nie wiesza całej aplikacji
            () =>
            {
                reader = new MediaFoundationReader(url);
                waveOut = new WaveOutEvent();
                waveOut.Init(reader);
                waveOut.Play();
            });
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Błąd odtwarzania: {ex.Message}");
        }
    }
}
