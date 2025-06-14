using Microsoft.UI.Xaml.Media.Imaging;
using Newtonsoft.Json;

namespace PtasieRadio.Services.UserProfileService;

public class UserProfileService : IUserProfileService
{
    private const string FileName = "users.json";
    private const string SelectedKeySetting = "SelectedKeyUser";
    private const string folderName = "PtasieRadio";
    StorageFolder folder = ApplicationData.Current.LocalFolder;
    private Dictionary<string, User> _profiles = new();
    public IReadOnlyDictionary<string, User> Profiles => _profiles;
    public string? SelectedKey { get; private set; }

    public async Task InitializeAsync()
    {
        await LoadProfilesAsync();
        LoadSelectedKey();

        // Jeśli nie ma profili, utwórz domyślny
        if (_profiles.Count == 0)
        {
            var defaultImageFile = await StorageFile.GetFileFromApplicationUriAsync(
                       new Uri("ms-appx:///Assets/Images/user.png")
                   );
            string defaultImageBase64 = await ImageFileToBase64(defaultImageFile);

            var defaultProfile = new User
            {
                Name = "Nowy profil",
                ImagePath = defaultImageBase64, // Base64 zamiast ścieżki
                Misc = ""
            };
            _profiles["1"] = defaultProfile;
            SelectedKey = "1";
            SaveSelectedKey();
            await SaveProfilesAsync();
        }
        // Jeśli nie ma wybranego profilu, wybierz pierwszy
        else if (string.IsNullOrEmpty(SelectedKey) || !_profiles.ContainsKey(SelectedKey))
        {
            SelectedKey = _profiles.Keys.First();
            SaveSelectedKey();
        }
    }

    public async Task AddProfileAsync(User profile)
    {
        // Znajdź wolny klucz
        int idx = 1;
        while (_profiles.ContainsKey(idx.ToString())) idx++;
        _profiles[idx.ToString()] = profile;
        await SaveProfilesAsync();
    }

    public async Task EditProfileAsync(string key, string newName, StorageFile newImageFile)
    {
        if (_profiles.TryGetValue(key, out var profile))
        {
            profile.Name = newName;
            profile.ImagePath = await ImageFileToBase64(newImageFile);
            await SaveProfilesAsync();
        }
    }
    private async Task<StorageFolder> OpenFolder() =>
        await ApplicationData.Current.LocalFolder.CreateFolderAsync(
            folderName, CreationCollisionOption.OpenIfExists);

    public static async Task RemoveEntryById(StorageFolder folder, string id)
    {
        var localFileName = "radio.json";
        try
        {
            var file = await folder.GetFileAsync(localFileName);
            string json = await FileIO.ReadTextAsync(file);

            var entries = JsonConvert.DeserializeObject<Dictionary<string, SaveEntryData>>(json)
                          ?? new Dictionary<string, SaveEntryData>();

            if (entries.ContainsKey(id))
            {
                entries.Remove(id);
                string newJson = JsonConvert.SerializeObject(entries, Formatting.Indented);
                await FileIO.WriteTextAsync(file, newJson);
            }
        }
        catch (FileNotFoundException)
        {
            System.Diagnostics.Debug.WriteLine($"Exception: Nie udało się znaleźć pliku");
        }
    }
    public async Task RemoveProfileAsync(string key)
    {
        if (_profiles.TryGetValue(key, out var profile))
        {
            using (await AddRadioService.AddRadioService.jsonSemaphore.Lock())
            {
                foreach (var stationId in profile.UserRadioStationKeys)
                {
                    await RemoveEntryById(folder, stationId);
                }
            }
        }
        if (_profiles.Remove(key))
        {
            // Jeśli usuwasz wybrany profil, wybierz inny lub utwórz domyślny
            if (SelectedKey == key)
            {
                if (_profiles.Count > 0)
                {
                    SelectedKey = _profiles.Keys.First();
                }
                else
                {
                    var defaultProfile = new User
                    {
                        Name = "Nowy profil usuniety",
                        ImagePath = "",
                        Misc = ""
                    };
                    _profiles["1"] = defaultProfile;
                    SelectedKey = "1";
                }
                SaveSelectedKey();
            }
            await SaveProfilesAsync();
        }
    }

    public Task SelectProfileAsync(string key)
    {
        if (_profiles.ContainsKey(key))
        {
            SelectedKey = key;
            SaveSelectedKey();
        }
        return Task.CompletedTask;
    }

    // --- Prywatne metody pomocnicze ---

    private async Task LoadProfilesAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        try
        {
            var file = await folder.GetFileAsync(FileName);
            var json = await FileIO.ReadTextAsync(file);
            _profiles = JsonConvert.DeserializeObject<Dictionary<string, User>>(json) ?? new();
        }
        catch
        {
            _profiles = new Dictionary<string, User>();
        }
    }

    private async Task SaveProfilesAsync()
    {
        var folder = ApplicationData.Current.LocalFolder;
        var file = await folder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
        var json = JsonConvert.SerializeObject(_profiles, Formatting.Indented);
        await FileIO.WriteTextAsync(file, json);
    }

    private void LoadSelectedKey()
    {
        SelectedKey = ApplicationData.Current.LocalSettings.Values[SelectedKeySetting] as string;
    }

    private void SaveSelectedKey()
    {
        ApplicationData.Current.LocalSettings.Values[SelectedKeySetting] = SelectedKey;
    }

    public async Task AddRadioStationKeyToCurrentProfile(string stationKey)
    {
        if (!string.IsNullOrEmpty(SelectedKey) && _profiles.TryGetValue(SelectedKey, out var profile))
        {
            if (!profile.UserRadioStationKeys.Contains(stationKey))
            {
                profile.UserRadioStationKeys.Add(stationKey);
                await SaveProfilesAsync();
            }
        }
    }
    public async Task FavouriteStationKeyToCurrentProfile(string stationKey)
    {
        if (!string.IsNullOrEmpty(SelectedKey) && _profiles.TryGetValue(SelectedKey, out var profile))
        {
            if (!profile.FavoriteStationIds.Contains(stationKey))
            {
                profile.FavoriteStationIds.Add(stationKey);
                await SaveProfilesAsync();
            }
            else
            {
                profile.FavoriteStationIds.Remove(stationKey);
                await SaveProfilesAsync();
            }
        }
    }

    public async Task<BitmapImage> Base64ToBitmapImage(string base64)
    {
        byte[] bytes = Convert.FromBase64String(base64);
        using (var stream = new MemoryStream(bytes))
        {
            var image = new BitmapImage();
            await image.SetSourceAsync(stream.AsRandomAccessStream());
            return image;
        }
    }
    public async Task<string> ImageFileToBase64(StorageFile file)
    {
        using (var stream = await file.OpenStreamForReadAsync())
        using (var memoryStream = new MemoryStream())
        {
            await stream.CopyToAsync(memoryStream);
            byte[] imageBytes = memoryStream.ToArray();
            return Convert.ToBase64String(imageBytes);
        }
    }


    public async Task<string> ImageToBase64Async(string imagePath)
    {
        var file = await StorageFile.GetFileFromApplicationUriAsync(
            new Uri($"ms-appx:///{imagePath}")
        );
        return await ImageFileToBase64(file);
    }
}