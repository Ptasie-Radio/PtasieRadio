namespace PtasieRadio.Services.UserProfileService;
public interface IUserProfileService
{
    IReadOnlyDictionary<string, User> Profiles { get; }
    string? SelectedKey { get; }
    Task InitializeAsync();
    Task AddProfileAsync(User profile);
    Task EditProfileAsync(string key, string newName, string newImagePath);
    Task RemoveProfileAsync(string key);
    Task SelectProfileAsync(string key);
}