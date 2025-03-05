using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs.Players
{
  //TODO rewrite once working
  static class PlayHandler
  {
    public static void Play(byte[] data)
    {
      var format = DetectAudioFormat(data);
      if (format == "MP3")
      {
        SimpleMp3Player player = new SimpleMp3Player();
        player.PlayAsync(data);
      }
      else if (format == "WAV")
      {
        SimpleWavPlayer player = new SimpleWavPlayer(data);
        player.PlayAsync();
      }
    }

    private static string DetectAudioFormat(byte[] data)
    {
      if (data == null || data.Length < 12)
        return "Unknown";

      // Check for WAV signature (RIFF header and WAVE format)
      if (data.Length >= 12 &&
          data[0] == 'R' && data[1] == 'I' && data[2] == 'F' && data[3] == 'F' &&
          data[8] == 'W' && data[9] == 'A' && data[10] == 'V' && data[11] == 'E')
      {
        return "WAV";
      }

      // Check for MP3 signature (MPEG Audio Frame Sync or ID3 header)
      if (data.Length >= 3 &&
          ((data[0] == 0xFF && (data[1] & 0xE0) == 0xE0) || // Frame sync (MPEG Audio)
           (data[0] == 'I' && data[1] == 'D' && data[2] == '3'))) // ID3 tag
      {
        return "MP3";
      }

      return "Unknown";
    }
  }
}
