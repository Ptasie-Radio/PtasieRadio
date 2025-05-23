using NAudio.Wave;
using Newtonsoft.Json;
using Windows.Storage;
using WinRT.Interop;
using PtasieRadio.Models;
using System.Collections.Generic;
using Uno.Extensions.Navigation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace PtasieRadio.Services.AddRadioService;

public class AddRadioService : IAddRadioService
{
    private string url = "";
    private string name = "";
    private string description = "";

    private string fileName = "";
    private static string folderName = "PtasieRadio";

    private StorageFile? selectedFile;

    public AddRadioService()
    {

    }

    public void setUrl(string url) { this.url = url; }
    public void setName(string name) { this.name = name; }
    public void setDescription(string description) { this.description = description; }

    public void setSelectedFile(StorageFile selectedFile) { this.selectedFile = selectedFile; }
    public async Task SaveImageToFile(INavigator _navigator)
    {
        if (selectedFile != null)
        {
            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(
                folderName, CreationCollisionOption.OpenIfExists);
            var path = folder.Path;
            System.Diagnostics.Process.Start("explorer.exe", path);
            ApplicationData.Current.LocalSettings.Values["SavedImageFolder"] = folderName;

            var files = await folder.GetFilesAsync();
            int i = 1;
            foreach (var file in files.Where(f => f.FileType == ".png")) { i++; }//Zwiększamy i dla kolejnych plików

            fileName = i + ".png";
            var savedFile = await folder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            await selectedFile.CopyAndReplaceAsync(savedFile);//Nadpisujemy plik naszymi danymi

            _ = SaveToJson(_navigator);
        }
    }

    public async Task SaveToJson(INavigator _navigator)
    {
        var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(
        folderName, CreationCollisionOption.OpenIfExists);
        var localFileName = "radio.json";
        Dictionary<string, SaveEntryData> entries;
        StorageFile file;
        try
        {
            file = await folder.GetFileAsync(localFileName);
            string existingJson = await FileIO.ReadTextAsync(file);
            entries = JsonConvert.DeserializeObject<Dictionary<string, SaveEntryData>>(existingJson) ?? new Dictionary<string, SaveEntryData>();
        }
        catch (FileNotFoundException)
        {
            entries = new Dictionary<string, SaveEntryData>();
        }
        if (selectedFile == null) return;
        var entry = new SaveEntryData
        {
            Url = url,
            Name = name,
            Description = description,
            SelectedFile = selectedFile
        };

        string index = fileName != null && fileName.Contains(".")
        ? fileName.Substring(0, fileName.IndexOf('.'))
        : fileName ?? "default_index";
        System.Diagnostics.Debug.WriteLine($"Zapisano do jsona {fileName}");
        entries[index] = entry;
        string json = JsonConvert.SerializeObject(entries, Formatting.Indented);
        file = await folder.CreateFileAsync(localFileName, CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, json);
        _ = GoToMain(_navigator);
    }

    public async Task GoToMain(INavigator _navigator)
    {
        await _navigator.NavigateRouteAsync(this, "/Main");
    }

}