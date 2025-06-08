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
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using PtasieRadio.Models;

namespace PtasieRadio.Presentation;


public sealed partial class AddRadioPage : Page
{
    private BitmapImage? bitmap;
    private string? selectedFile;

    public AddRadioPage()
    {
        this.InitializeComponent();
    }

	private async void OnSelectImageTapped(object sender, TappedRoutedEventArgs e)
	{
		e.Handled = true;
		var picker = new FileOpenPicker();
		var hwnd = WindowNative.GetWindowHandle(App.GetMainWindow());
		InitializeWithWindow.Initialize(picker, hwnd);

		picker.FileTypeFilter.Add(".png");
		picker.FileTypeFilter.Add(".jpg");
		picker.FileTypeFilter.Add(".jpeg");
		picker.FileTypeFilter.Add(".webp");
		var file = await picker.PickSingleFileAsync();
		if (file != null)
		{
			selectedFile = file.Path;
			try
			{
				using var stream = await file.OpenAsync(FileAccessMode.Read);
				bitmap = new BitmapImage();
				await bitmap.SetSourceAsync(stream);
			}
			catch (FileNotFoundException)
			{
				bitmap = new BitmapImage(new Uri("ms-appx:///Assets/Images/placeholder.png"));
			}
			catch (System.NullReferenceException)
			{
				bitmap = new BitmapImage(new Uri("ms-appx:///Assets/Images/placeholder.png"));
			}

			var image = SelectedImageButton.GetTemplateChild("SelectedImage") as Image;
			if (image != null)
			{
				image.Source = bitmap;
			}
		}
	}

	private void OnSaveButtonTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        string url = UrlTextBox.Text;
        string name = NameTextBox.Text;
        string description = DescriptionTextBox.Text;
        if (!string.IsNullOrWhiteSpace(url) && !string.IsNullOrWhiteSpace(url) && selectedFile != null)
        {

            var viewModel = DataContext as AddRadioModel;
            if (viewModel == null) return;
            var data = new SaveEntryData
            {
                StreamUrl = url,
                Name = name,
                Description = description,
                ImagePath = selectedFile,
                Country = "Polska",
                Category="Własne"
            };
            viewModel.OnSaveToFileCommand.Execute(data);
        }

    }

}



