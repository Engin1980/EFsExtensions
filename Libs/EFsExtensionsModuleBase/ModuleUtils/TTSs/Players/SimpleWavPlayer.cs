using ESystem.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs.Players
{
  public class SimpleWavPlayer : IDisposable
  {
    public delegate void PlayerDelegate(SimpleWavPlayer sender);

    public event PlayerDelegate? PlaybackFinished;

    public event PlayerDelegate? PlaybackStarted;
    private readonly Logger logHandler;

    public readonly int length;
    private readonly SoundPlayer soundPlayer;
    public SimpleWavPlayer(byte[] bytes)
    {
      this.logHandler = Logger.Create(this);
      MemoryStream stream = new(bytes);
      this.soundPlayer = new SoundPlayer(stream);
      this.length = bytes.Length;
      this.logHandler.Log(LogLevel.INFO, $"Created sound-player for {bytes.Length} bytes.");
    }
    public void Play()
    {
      this.logHandler.Log(LogLevel.INFO, $"Play requested.");
      PlaybackStarted?.Invoke(this);
      this.soundPlayer.PlaySync();
      PlaybackFinished?.Invoke(this);
    }

    public void PlayAsync()
    {
      this.logHandler.Log(LogLevel.INFO, $"Play-async requested.");
      Task t = new(this.Play);
      t.Start();
    }

    public void Dispose()
    {
      Logger.UnregisterSender(this);
    }
  }
}
