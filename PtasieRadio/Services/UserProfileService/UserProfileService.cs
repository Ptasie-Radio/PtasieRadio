using Newtonsoft.Json;

namespace PtasieRadio.Services.UserProfileService;
public class UserProfileService : IUserProfileService
{
    private const string FileName = "users.json";
    private const string SelectedKeySetting = "SelectedKeyUser";
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
            var defaultProfile = new User
            {
                Name = "Nowy profil",
                ImagePath = @"Assets/Images/user.png",
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

    public async Task EditProfileAsync(string key, string newName, string newImagePath)
    {
        if (_profiles.TryGetValue(key, out var profile))
        {
            profile.Name = newName;
            profile.ImagePath = newImagePath;
            await SaveProfilesAsync();
        }
    }

    public async Task RemoveProfileAsync(string key)
    {
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
                        ImagePath = @"Assets/Images/user.png",
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
}