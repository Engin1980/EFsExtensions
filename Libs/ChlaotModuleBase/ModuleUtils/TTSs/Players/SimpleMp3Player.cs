using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs.Players
{
  public class SimpleMp3Player
  {
    private MemoryStream? ms;
    private WaveStream? mp3stream;
    private WaveOutEvent? player;
    private bool isUsed = false;

    public void PlayAsync(byte[] mp3)
    {
      Debug.Assert(isUsed == false, "The same simple player cannot be used twice.");
      isUsed = true;

      ms = new MemoryStream(mp3);
      mp3stream = new Mp3FileReader(ms);
      player = new WaveOutEvent();
      player.PlaybackStopped += Player_PlaybackStopped;
      player.Init(mp3stream);
      player.Play();
    }

    private void Player_PlaybackStopped(object? sender, EventArgs e)
    {
      player!.Stop();
      mp3stream!.Dispose();
      ms!.Dispose();
      player!.Dispose();
    }
  }
}
