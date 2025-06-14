public interface IRadioPlayerService
{
    bool IsPlaying { get; }
    bool IsMuted { get; }
    float Volume { get; }
    string? StationName { set; get; }
    string StationImagePath { get; set; }
    string StationCountry { get; set; }
    Task PlayOrPauseAsync();
    Task StopAsync();
    Task Reset();
    void SetVolume(double value);
    void ToggleMute();
    void SetIsMuted(bool muted);
    void SetUrl(string url);
    string? GetUrl();
    bool GetIsInitialized();
    bool GetIsMuted();
    bool GetIsPlaying();
    void SetIsPlaying(bool isPlaying);
    float GetVolume();
}