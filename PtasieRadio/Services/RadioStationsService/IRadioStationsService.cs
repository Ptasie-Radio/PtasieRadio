namespace PtasieRadio.Services.RadioStationsService;
public interface IRadioStationsService
{
    Task<List<RadioStation>> LoadStationsAsync(string filePath);
}