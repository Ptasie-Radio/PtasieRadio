using NAudio.Wave;
using System.Net;
using System.Text.Json;
//using static Android.Renderscripts.Sampler;

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
    public float Volume => currentVolume * 100f;


    public RadioPlayerService()
    {
        LoadListFromRadioBrowser();
    }

    public async Task PlayOrPauseAsync()
    {
        if (isBusy) return;//Jeśli mamy blokadę, to nie pozwalaj na zmianę
        isBusy = true;
        await WithMediaLock(async () =>
        {
            try
            {
                if (!isInitialized)
                {
                    try
                    {
                        reader = new MediaFoundationReader(url);
                        waveOut = new WaveOutEvent();
                        waveOut.Init(reader);
                        waveOut.Volume = isMuted ? 0 : currentVolume;
                        isInitialized = true;
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
                }
                else
                {
                    waveOut?.Play();
                    isPlaying = true;
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
                    foreach (var station in stations)
                        Console.WriteLine(station.Name);

                    using (await AddRadioService.AddRadioService.jsonSemaphore.Lock())
                    {
                        StorageFolder folder = await MainPage.OpenFolder();
                        var entries = await MainPage.LoadFromJson(folder);
                        int imageIndex = 1;
                        foreach (var station in stations)
                        {
                            var fileFormat = station.Favicon?.Map(f => Path.GetExtension(f.AbsolutePath));
                            string fileName = "stationIcon" + imageIndex + (string.IsNullOrEmpty(fileFormat) ? "" : ("." + fileFormat));
                            int index = fileName.IndexOf('?');
                            if (index >= 0) fileName = fileName.Substring(0, index);

                            imageIndex++;

                            if (index >= 0) fileName = fileName.Substring(0, index);
                            HttpResponseMessage? image = null;
                            Stream stream;
                            try
                            {
                                image = await httpClient.GetAsync(station.Favicon);
                                image.EnsureSuccessStatusCode();
                                stream = await image.Content.ReadAsStreamAsync();

                            }
                            catch
                            {
                                var uri = new Uri("ms-appx:///Assets/Images/placeholder.png");
                                StorageFile f = await StorageFile.GetFileFromApplicationUriAsync(uri);
                                stream = (await f.OpenReadAsync()).AsStream();

                            }

                            using (image)
                            using (stream)
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
                                };

                                foreach (var item in entries.Where(kvp => kvp.Value.StreamUrl == s.StreamUrl && kvp.Value.Category == "POP").ToList())//Dodane kvp.Value.Category, aby tylko z POP brało i usuwało
                                {
                                    entries.Remove(item.Key);
                                }

                                entries.Add(AddRadioService.AddRadioService.NextFreeIndex(entries).ToString(), s);
                            }
                        }
                        await AddRadioService.AddRadioService.SaveToJson(folder, entries);
                    }
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