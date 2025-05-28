using NAudio.Wave;
using Newtonsoft.Json;
using Windows.Storage;
using WinRT.Interop;
using PtasieRadio.Models;
using System.Collections.Generic;
using Uno.Extensions.Navigation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Uno.Disposables;

namespace PtasieRadio.Services.AddRadioService;

public class AddRadioService : IAddRadioService
{
	public const string localFileName = "radio.json";
	public static SemaphoreSlim jsonSemaphore = new SemaphoreSlim(1);

	private static string folderName = "PtasieRadio";

    public AddRadioService()
    {

    }

	public static int NextFreeIndex<T>(Dictionary<string, T> dictionary)
    {
        int i = 1;
        for (; dictionary.ContainsKey(i.ToString()); i++) ;
        return i;
	}


	public async Task AddOneRadioToJson(string url, string name, string description, string imagePath)
	{
		using (await jsonSemaphore.Lock())
		{
			var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(
			folderName, CreationCollisionOption.OpenIfExists);
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
			var entry = new SaveEntryData
			{
				StreamUrl = url,
				Name = name,
				Description = description,
				ImagePath = imagePath,
				Country = "Polska",
				Category = "Własne"
			};

			int index = NextFreeIndex(entries);

			entries[index.ToString()] = entry;
			await SaveToJson(folder, entries);
		}
	}

	public static async Task SaveToJson(StorageFolder folder, Dictionary<string, SaveEntryData> entries)
	{
		string json = JsonConvert.SerializeObject(entries, Formatting.Indented);
		var saveFile = await folder.CreateFileAsync(localFileName, CreationCollisionOption.ReplaceExisting);
		await FileIO.WriteTextAsync(saveFile, json);
	}
}