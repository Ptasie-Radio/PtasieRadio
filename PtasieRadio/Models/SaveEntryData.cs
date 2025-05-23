namespace PtasieRadio.Models;

using Windows.Storage;
using Newtonsoft.Json;
public class SaveEntryData
{
    public required string Url { get; set; }  = string.Empty;
    public required string Name { get; set; }  = string.Empty;
    public required string Description { get; set; }  = string.Empty;
    public required string PictureLocalization { get; set; }  = string.Empty;
    
    [JsonIgnore]
    public required StorageFile SelectedFile { get; set; }
}