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
    private double pageAnimationTime;

    public MainPage()
    {  

        this.pageAnimationTime = 0.4;
        this.InitializeComponent();
        this.SizeChanged += MainPage_SizeChanged;
        
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

        if(Microsoft.UI.Xaml.Window.Current == null)return;//Musi tak być, inaczej warning
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

        if(Microsoft.UI.Xaml.Window.Current == null)return;//Musi tak być, inaczej warning
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

    private void ShuffleButtonTappedEvent(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
    }

    private void PlayPauseButtonTappedEvent(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
    }

    private void HeartButtonTappedEvent(object sender, TappedRoutedEventArgs e)
    {
        e.Handled=true;
    }

    private void OnSoundLevelSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {

    var viewModel = DataContext as MainModel;

    if (viewModel == null)  return;

    viewModel.Volume = e.NewValue;
	}
}