using NAudio.Wave;
using System.Net;
using System.Text.Json;

namespace PtasieRadio.Services.RadioService;

public class RadioPlayerService : IRadioPlayerService
{
    private WaveOutEvent? waveOut;
    private readonly SemaphoreSlim mediaLock = new SemaphoreSlim(1, 1);
    private MediaFoundationReader? reader;
    private bool isPlaying = false;
    private bool isInitialized = false;
    private float currentVolume = 0.5f;
    private bool isMuted = false;
    private bool isBusy = false;
    private string? url;
    public string? StationName { set; get; }
    public string StationImagePath { set; get; }
    public string StationCountry { get; set; }
    public bool IsPlaying => isPlaying;
    public bool IsMuted => isMuted;

    public bool wasDownloaded = false;
    public float Volume => currentVolume * 100f;
    public event Action? Buffering;
    public event Action? Playing;
    public event Action? NotPlaying;


    public RadioPlayerService()
    {
        LoadListFromRadioBrowser();
    }

    public async Task PlayOrPauseAsync()
    {
        if (isBusy) return;
        isBusy = true;
        await WithMediaLock(async () =>
        {
            try
            {
                if (!isInitialized)
                {
                    try
                    {
                        Buffering.Invoke();
                        reader = new MediaFoundationReader(url);
                        waveOut = new WaveOutEvent();
                        waveOut.Init(reader);
                        waveOut.Volume = isMuted ? 0 : currentVolume;
                        isInitialized = true;
                        Playing.Invoke();
                    }
                    catch (FileNotFoundException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"{ex}");
                    }
                    catch (System.Runtime.InteropServices.COMException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error:{ex}");
                    }
                }
                if (isPlaying)
                {
                    try { waveOut?.Stop(); } catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error:{ex}"); }
                    isPlaying = false;
                    NotPlaying.Invoke();

                }
                else
                {
                    waveOut?.Play();
                    isPlaying = true;
					Playing.Invoke();
				}
            }
            finally
            {
                isBusy = false;
            }
        });
    }

    public async Task StopAsync()
    {
        await Task.Run(() =>
        {
            waveOut?.Stop();
            isPlaying = false;
        });
    }

    public async Task Reset()
    {
        await WithMediaLock(async () =>
        {

            if (isPlaying)
            {
                isPlaying = false;
                try { waveOut?.Stop(); } catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error:{ex}"); }
            }
            waveOut?.Dispose();
            waveOut = null;
            reader?.Dispose();
            reader = null;
            isInitialized = false;

        });
    }

    private async Task WithMediaLock(Func<Task> function)
    {
        await Task.Run(async () =>
        {
            await mediaLock.WaitAsync();
            try
            {
                await function();
            }
            finally
            {
                mediaLock.Release();
            }
        });
    }

    public void SetVolume(double volume)
    {
        currentVolume = (float)(volume / 100);
        if (waveOut != null)
        {
            waveOut.Volume = (float)(volume / 100);
        }
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        if (waveOut != null)
        {
            waveOut.Volume = isMuted ? 0 : currentVolume;
        }
    }

    private async void LoadListFromRadioBrowser()
    {
        try
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", "PtasieRadio/0.1");
                using (var response = await httpClient.GetAsync($"https://all.api.radio-browser.info/json/stations/search?limit=30&countrycode=pl&hidebroken=true&order=clickcount&reverse=true"))
                {

                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsStringAsync();
                    var stations = JsonSerializer.Deserialize<List<StationInfo>>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

                    StorageFolder folder = await MainPage.OpenFolder();
                    var entries = await MainPage.LoadFromJson(folder);
                    int imageIndex = 1;

                    foreach (var station in stations)
                    {
                        var fileFormat = station.Favicon?.Map(f => Path.GetExtension(f.AbsolutePath));
                        string fileName = "stationIcon" + imageIndex + (string.IsNullOrEmpty(fileFormat) ? "" : ("." + fileFormat));
                        imageIndex++;

                        HttpResponseMessage? image = null;
                        Stream stream;
                        try
                        {
                            image = await httpClient.GetAsync(station.Favicon);
                            image.EnsureSuccessStatusCode();
                            stream = await image.Content.ReadAsStreamAsync();
                        }
                        catch(Exception)
                        {
                            var uri = new Uri("ms-appx:///Assets/Images/radio_placeholder_square.png");
                            StorageFile f = await StorageFile.GetFileFromApplicationUriAsync(uri);
                            stream = (await f.OpenReadAsync()).AsStream();
                        }
                        using (image)
                        using (stream)
                        using (await AddRadioService.AddRadioService.jsonSemaphore.Lock())
                        {
                            var saveFile = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                            File.WriteAllBytes(saveFile.Path, stream.ReadBytes());
                            SaveEntryData s = new SaveEntryData
                            {
                                Name = station.Name ?? "Brak Nazwy",
                                StreamUrl = station.Url?.ToString() ?? "Brak",
                                ImagePath = saveFile.Path,
                                Country = station.CountryCode ?? "Unknown",
                                Category = "POP",
                                Description = String.Join(", ", station.Tags ?? new List<string>()),
                                NumberOfTimesPlayed = 0,
                            };
                            foreach (var item in entries.Where(kvp => kvp.Value.StreamUrl == s.StreamUrl && kvp.Value.Category == "POP").ToList())//Dodane kvp.Value.Category, aby tylko z POP brało i usuwało
                            {
                                try
                                {
                                    s.NumberOfTimesPlayed = item.Value.NumberOfTimesPlayed;
                                }
                                catch (Exception)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Exception: Nie udało się pozyskać NumberOfTimesPlayed");
                                }
                                entries.Remove(item.Key);
                            }
                            entries.Add(AddRadioService.AddRadioService.NextFreeIndex(entries).ToString(), s);
                        }
                    }
                    await AddRadioService.AddRadioService.SaveToJson(folder, entries);
                    
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public void SetIsMuted(bool muted) { isMuted = muted; }
    public bool GetIsMuted() { return IsMuted; }
    public bool GetIsPlaying() { return IsPlaying; }
    public void SetIsPlaying(bool isPlaying) { this.isPlaying = isPlaying; }
    public float GetVolume() { return Volume; }
    public string? GetUrl() { return url; }
    public bool GetIsInitialized() { return isInitialized; }
    public void SetUrl(string url) { this.url = url; }
}