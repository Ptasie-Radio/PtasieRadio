namespace PtasieRadio;

public class User
{
    public required string Name { get; set; } = string.Empty;
    public required string ImagePath { get; set; } = string.Empty;
    public string? Misc { get; set; } // pole na przyszłość

}