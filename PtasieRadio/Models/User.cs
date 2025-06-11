namespace PtasieRadio;

public class User
{
    public required string Name { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty; // tak naprawde to nie Path tylko base64 zdjecia zamieniany na BitmapImage
    // w przypadku gdy ImagePath bedzie null najlepiej bedzie uzyc takiego rozwiazania:
    // ProfileImage = new BitmapImage(new Uri("ms-appx:///Assets/Images/user.png"));
    public List<string> UserRadioStationKeys { get; set; } = new();
    public string? Misc { get; set; } // pole na przyszłość

}