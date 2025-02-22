using ELogging;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.AudioPlaying;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs.Players;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.AudioPlaying
{
  public class EPlayer
  {
    public enum Format
    {
      Unknown,
      MP3,
      WAV
    }

    private readonly byte[] bytes;
    private readonly Format format;

    public delegate void PlaybackFinishedHandler(EPlayer player);
    public event PlaybackFinishedHandler? PlaybackFinished;

    public EPlayer(byte[] audioBytes)
    {
      this.bytes = audioBytes;
      this.format = DetectAudioFormat(audioBytes);
    }

    private static Format DetectAudioFormat(byte[] data)
    {
      if (data == null || data.Length < 12)
        return Format.Unknown;

      // Check for WAV signature (RIFF header and WAVE format)
      if (data.Length >= 12 &&
          data[0] == 'R' && data[1] == 'I' && data[2] == 'F' && data[3] == 'F' &&
          data[8] == 'W' && data[9] == 'A' && data[10] == 'V' && data[11] == 'E')
      {
        return Format.WAV;
      }

      // Check for MP3 signature (MPEG Audio Frame Sync or ID3 header)
      if (data.Length >= 3 &&
          ((data[0] == 0xFF && (data[1] & 0xE0) == 0xE0) || // Frame sync (MPEG Audio)
           (data[0] == 'I' && data[1] == 'D' && data[2] == '3'))) // ID3 tag
      {
        return Format.MP3;
      }

      return Format.Unknown;
    }

    private static void PlayMp3Asynchornously(byte[] bytes, Action callback)
    {
      var ms = new MemoryStream(bytes);
      var mp3stream = new Mp3FileReader(ms);
      var player = new WaveOutEvent();
      player.PlaybackStopped += (s, e) =>
      {
        player.Stop();
        mp3stream.Dispose();
        ms.Dispose();
        player.Dispose();

        callback();
      };
      player.Init(mp3stream);
      player.Play();
    }

    private static void PlayWavAsynchronously(byte[] bytes, Action callback)
    {
      void playAsync()
      {
        MemoryStream stream = new(bytes);
        var player = new SoundPlayer(stream);
        player.PlaySync();
        callback();
      }

      Task t = new(playAsync);
      t.Start();
    }

    public void PlayAsynchronously()
    {
      if (format == Format.MP3)
      {
        PlayMp3Asynchornously(this.bytes, () => PlaybackFinished?.Invoke(this));

      }
      else if (format == Format.WAV)
      {
        PlayWavAsynchronously(this.bytes, () => PlaybackFinished?.Invoke(this));
      }
    }
  }
}
