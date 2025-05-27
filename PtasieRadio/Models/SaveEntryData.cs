namespace PtasieRadio.Models;

using Windows.Storage;
using Newtonsoft.Json;
public class SaveEntryData
{
    public required string Name { get; set; }  = string.Empty;
    public required string StreamUrl { get; set; }  = string.Empty;
    public required string ImagePath { get; set; }  = string.Empty;
    public required string Country { get; set; }  = string.Empty;
    public required string Category { get; set; }  = string.Empty;
    public required string Description { get; set; }  = string.Empty;
    
    //[JsonIgnore]
    //public required StorageFile SelectedFile { get; set; }
}