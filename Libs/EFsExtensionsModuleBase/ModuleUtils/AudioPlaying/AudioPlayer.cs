using ESystem.Exceptions;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.AudioPlaying
{
  public class AudioPlayer
  {
    public enum AudioFormat
    {
      Unknown,
      Wav,
      Mp3
    }

    public static AudioFormat DetectAudioFormat(byte[] data)
    {
      if (data == null || data.Length < 12)
        return AudioFormat.Unknown;

      // Check for WAV signature (RIFF header and WAVE format)
      if (data.Length >= 12 &&
          data[0] == 'R' && data[1] == 'I' && data[2] == 'F' && data[3] == 'F' &&
          data[8] == 'W' && data[9] == 'A' && data[10] == 'V' && data[11] == 'E')
      {
        return AudioFormat.Wav;
      }

      // Check for MP3 signature (MPEG Audio Frame Sync or ID3 header)
      if (data.Length >= 3 &&
          ((data[0] == 0xFF && (data[1] & 0xE0) == 0xE0) || // Frame sync (MPEG Audio)
           (data[0] == 'I' && data[1] == 'D' && data[2] == '3'))) // ID3 tag
      {
        return AudioFormat.Mp3;
      }

      return AudioFormat.Unknown;
    }

    private static MemoryStream ConvertMp3ToWav(byte[] mp3Bytes)
    {
      MemoryStream mp3Stream = new(mp3Bytes);
      MemoryStream wavStream = new();

      using (var mp3Reader = new Mp3FileReader(mp3Stream))
      using (var pcmStream = WaveFormatConversionStream.CreatePcmStream(mp3Reader))
      {
        var waveFile = new WaveFileWriter(wavStream, pcmStream.WaveFormat);
        pcmStream.CopyTo(waveFile);
        waveFile.Flush();
      }

      // Reset stream position for reading
      wavStream.Position = 0;

      return wavStream;
    }

    private const int SLEEP_INTERVAL_IN_PLAY_TEST_WHILE = 300;
    private static int nextId = 1;
    public int Id { get; private set; } = nextId++;
    private readonly byte[] audioData;
    private readonly AudioFormat audioFormat;

    public delegate void AudioPlayerHandler(AudioPlayer sender);
    public event AudioPlayerHandler? PlayCompleted;
    public event AudioPlayerHandler? PlayRequested;
    public event AudioPlayerHandler? PlayStarting;
    public event AudioPlayerHandler? PlayStarted;

    public AudioPlayer(byte[] audioData)
    {
      this.audioData = audioData ?? throw new ArgumentNullException(nameof(audioData));
      this.audioFormat = DetectAudioFormat(this.audioData);
      if (this.audioFormat == AudioFormat.Unknown)
        throw new ArgumentException("Invalid/Unrecognized audio format. Expected Wav/Mp3. - argument " + nameof(audioData));
    }

    public void Play()
    {
      this.PlayRequested?.Invoke(this);
      if (this.audioFormat == AudioFormat.Mp3)
        PlayMp3(audioData);
      else if (this.audioFormat == AudioFormat.Wav)
        PlayWav(audioData);
      else
        throw new UnexpectedEnumValueException(this.audioFormat);
    }

    public Task PlayAsync()
    {
      Task t = new(() => Play());
      t.Start();
      return t;
    }

    private void PlayWavStream(Stream stream)
    {
      using var waveOut = new WaveOutEvent();
      using var provider = new WaveFileReader(stream);

      this.PlayStarting?.Invoke(this);
      waveOut.Init(provider);
      waveOut.PlaybackStopped += WaveOut_PlaybackStopped;
      waveOut.Play();
      this.PlayStarted?.Invoke(this);

      // this must be here to keep "using" active, otherwise waveOut+provider is disposed and sound is cut
      while (waveOut.PlaybackState == PlaybackState.Playing)
        Thread.Sleep(SLEEP_INTERVAL_IN_PLAY_TEST_WHILE);
    }

    private void PlayWav(byte[] audioData)
    {
      using var ms = new MemoryStream(audioData);
      ms.Position = 0;
      PlayWavStream(ms);
    }

    private void WaveOut_PlaybackStopped(object? sender, StoppedEventArgs e)
    {
      PlayCompleted?.Invoke(this);
    }

    private void PlayMp3(byte[] audioData)
    {
      Stream s = ConvertMp3ToWav(audioData);
      PlayWavStream(s);
    }
  }
}
