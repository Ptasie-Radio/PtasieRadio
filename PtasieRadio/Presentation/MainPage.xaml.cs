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

    private void radioOnClick(object sender, RoutedEventArgs e)
    {
        string url = "https://playerservices.streamtheworld.com/api/livestream-redirect/WUAL_HD3.mp3";

        try
        {
            //Tutaj bierze zatrzymuje jeśli coś już nam gra, inaczej się psuło

            waveOut?.Stop();// ?. To operator warunkowego dostępu, nie wywala błędu jak NULL
            waveOut?.Dispose();
            reader?.Dispose();

            reader = new MediaFoundationReader(url);//Czyta z tego url'a
            waveOut = new WaveOutEvent();
            waveOut.Init(reader);
            waveOut.Play();
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Błąd odtwarzania: {ex.Message}");
        }
    }
}
