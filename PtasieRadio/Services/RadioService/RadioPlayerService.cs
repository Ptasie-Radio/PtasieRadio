using NAudio.Wave;

namespace PtasieRadio.Services.RadioService;

public class RadioPlayerService : IRadioPlayerService
{
    private WaveOutEvent? waveOut;
    private MediaFoundationReader? reader;
    private bool isPlaying = false;
    private bool isInitialized = false;
    private float currentVolume = 1.0f;
    private bool isMuted = false;

    public bool IsPlaying => isPlaying;
    public bool IsMuted => isMuted;
    public float Volume => currentVolume * 100f;

    public async Task PlayOrPauseAsync(string url)
    {
        await Task.Run(() =>
        {
            if (isPlaying)
            {
                waveOut?.Stop();
                isPlaying = false;
            }
            else
            {
                if (!isInitialized)
                {
                    reader = new MediaFoundationReader(url);
                    waveOut = new WaveOutEvent();
                    waveOut.Init(reader);
                    waveOut.Volume = isMuted ? 0 : currentVolume;
                    isInitialized = true;
                }

                waveOut?.Play();
                isPlaying = true;
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

    public void Reset()
    {
        waveOut?.Stop();
        waveOut?.Dispose();
        waveOut = null;

        reader?.Dispose();
        reader = null;

        isPlaying = false;
        isInitialized = false;
        isMuted = false;
        currentVolume = 1.0f;
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

    public void setIsMuted(bool muted){isMuted = muted;}
    public bool GetIsMuted(){return IsMuted;}
    public bool GetIsPlaying(){return IsPlaying;}
    public float GetVolume(){return Volume;}
}