using System.Text.Json;

namespace PtasieRadio.Services.RadioStationsService;

public class RadioStationsService : IRadioStationsService
{
    public async Task<List<RadioStation>> LoadStationsAsync(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<List<RadioStation>>(stream);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd wczytywania stacji: {ex.Message}");
            return new List<RadioStation>();
        }
    }
}