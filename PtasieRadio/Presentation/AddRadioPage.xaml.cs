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

namespace PtasieRadio.Presentation;


public sealed partial class AddRadioPage : Page
{
    public AddRadioPage()
    {
        this.InitializeComponent();
    }
    private StorageFile? _selectedFile;
    /*
    private async void OnSelectImageClick(object sender, RoutedEventArgs e)
    {
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
            _selectedFile = file;

            using var stream = await file.OpenAsync(FileAccessMode.Read);
            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);
            SelectedImage.Source = bitmap;
            SelectedImage.Visibility = Visibility.Visible;
        }
    }
    
    private async Task SaveImageAsync()
    {
        if (_selectedFile != null)
        {
            string folderName = "PtasieRadio";
            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(
                folderName, CreationCollisionOption.OpenIfExists);

            ApplicationData.Current.LocalSettings.Values["SavedImageFolder"] = folderName;

            string fileName = "image.png";

            var savedFile = await folder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            await _selectedFile.CopyAndReplaceAsync(savedFile);
        }
    }

    private async Task LoadAllSavedImagesAsync()
    {
        var settings = ApplicationData.Current.LocalSettings;

        if (settings.Values.TryGetValue("SavedImageFolder", out var folderNameObj) && folderNameObj is string folderName)
        {
            try
            {
                var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync(folderName);

                var files = await folder.GetFilesAsync();

                foreach (var file in files.Where(f => f.FileType == ".png" || f.FileType == ".jpg" || f.FileType == ".webp" || f.FileType == ".jpeg"))
                {
                    using var stream = await file.OpenAsync(FileAccessMode.Read);

                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(stream);

                    LoadedImage.Source = bitmap;
                }
            }
            catch (Exception ex)
            {

                System.Diagnostics.Debug.WriteLine($"Błąd ładowania folderu: {ex.Message}");
            }
        }
    }
    
    private async Task SaveRadio(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        await SaveImageAsync();
    }
    */
    
    //To tutaj przerobić tak, by po wybraniu zdjęcia podmieniło odpowiednie na naszej stronie
    /*private async Task LoadSavedImageAsync()
    {
        try
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var customFolder = await localFolder.GetFolderAsync("PtasieRadio");
            var savedFile = await customFolder.GetFileAsync("myimage.png");

            using var stream = await savedFile.OpenAsync(FileAccessMode.Read);
            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);

            LoadedImage.Source = bitmap;
            LoadedImage.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Błąd wczytywania obrazu: {ex.Message}");
        }
    }*/
}