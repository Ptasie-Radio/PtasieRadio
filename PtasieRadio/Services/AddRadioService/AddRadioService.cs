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
    private static string folderName = "PtasieRadio";

    private StorageFile? selectedFile;

    public AddRadioService()
    {

    }

    public void setUrl(string url) { this.url = url; }
    public void setName(string name) { this.name = name; }
    public void setDescription(string description) { this.description = description; }

    public void setSelectedFile(StorageFile selectedFile) { this.selectedFile = selectedFile; }

    public async Task<int> GetIndexFromJson()
    {
        var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(
            folderName, CreationCollisionOption.OpenIfExists);
        var localFileName = "radio.json";
        Dictionary<string, SaveEntryData> entries;
        int i = 1;
        try
        {
            var file = await folder.GetFileAsync(localFileName);
            string json = await FileIO.ReadTextAsync(file);
            entries = JsonConvert.DeserializeObject<Dictionary<string, SaveEntryData>>(json)
                      ?? new Dictionary<string, SaveEntryData>();
        }
        catch (FileNotFoundException)
        {
            entries = new Dictionary<string, SaveEntryData>();
        }
        while (true)
        {
            if (entries.ContainsKey(i.ToString()))
            {
                i++;
                continue;
            }
            else break;
        }
        return i;   
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
            
            System.Diagnostics.Process.Start("explorer.exe", file.Path);
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
            SelectedFile = selectedFile,
            PictureLocalization = selectedFile.Path
        };

        int index = await GetIndexFromJson();
       
        entries[index.ToString()] = entry;
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