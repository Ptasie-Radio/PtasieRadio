using Microsoft.UI.Xaml.Media.Imaging;

namespace PtasieRadio.Services.UserProfileService;
public interface IUserProfileService
{
    IReadOnlyDictionary<string, User> Profiles { get; }
    string? SelectedKey { get; }
    Task InitializeAsync();
    Task AddProfileAsync(User profile);
    Task EditProfileAsync(string? key, string newName, StorageFile newImagePath);
    Task RemoveProfileAsync(string? key);
    Task SelectProfileAsync(string key);
    Task AddRadioStationKeyToCurrentProfile(string stationKey);
    Task FavouriteStationKeyToCurrentProfile(string stationKey);
    Task<string> ImageToBase64Async(string imagePath);
    Task<string> ImageFileToBase64(StorageFile file);
    Task<BitmapImage> Base64ToBitmapImage(string base64);
}