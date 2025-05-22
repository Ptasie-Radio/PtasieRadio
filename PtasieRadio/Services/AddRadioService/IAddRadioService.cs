public interface IAddRadioService
{
    
    void setUrl(string url);
    void setName(string name);
    void setDescription(string description);
    void setSelectedFile(StorageFile selectedFile);

    Task SaveImageToFile(INavigator _navigator);
}