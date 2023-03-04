using ELogging;
using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;

namespace ChlaotModuleBase.ModuleUtils.Playing
{
  public class Player : LogIdAble, IDisposable
  {
    public delegate void PlayerDelegate(Player sender);

    public event PlayerDelegate? PlaybackFinished;

    public event PlayerDelegate? PlaybackStarted;
    private readonly NewLogHandler logHandler;

    public readonly int length;
    private readonly SoundPlayer soundPlayer;
    public Player(byte[] bytes)
    {
      this.logHandler= Logger.RegisterSender(this);
      MemoryStream stream = new(bytes);
      this.soundPlayer = new SoundPlayer(stream);
      this.length = bytes.Length;
      this.logHandler.Invoke(LogLevel.INFO, $"Created sound-player for {bytes.Length} bytes.");
    }
    public void Play()
    {
      this.logHandler.Invoke(LogLevel.INFO, $"Play requested.");
      PlaybackStarted?.Invoke(this);
      this.soundPlayer.PlaySync();
      PlaybackFinished?.Invoke(this);
    }

    public void PlayAsync()
    {
      this.logHandler.Invoke(LogLevel.INFO, $"Play-async requested.");
      Task t = new(this.Play);
      t.Start();
    }

    public void Dispose()
    {
      Logger.UnregisterSender(this);
    }
  }
}
