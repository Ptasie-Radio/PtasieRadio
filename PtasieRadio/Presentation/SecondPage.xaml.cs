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
using Windows.Devices.Radios;
using Microsoft.UI.Xaml.Controls.Primitives;

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
        Debug.WriteLine("DataContext is NOT SecondModel");

        if (viewModel == null) return;
        viewModel.NavigateToAddRadioCommand.Execute(null);

    }
}

