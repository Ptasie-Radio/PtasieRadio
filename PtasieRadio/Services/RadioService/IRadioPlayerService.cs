public interface IRadioPlayerService
{
    bool IsPlaying { get; }
    bool IsMuted { get; }
    float Volume { get; }

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