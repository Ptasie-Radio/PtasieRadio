public interface IRadioPlayerService
{
    bool IsPlaying { get; }
    bool IsMuted { get; }
    float Volume { get; }

    Task PlayOrPauseAsync(string url);
    Task StopAsync();
    void Reset();
    void SetVolume(double value);
    void ToggleMute();
    void setIsMuted(bool muted);
    bool GetIsMuted();
    bool GetIsPlaying();
    float GetVolume();
}