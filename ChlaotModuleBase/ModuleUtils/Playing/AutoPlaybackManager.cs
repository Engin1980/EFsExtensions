using ELogging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ChlaotModuleBase.ModuleUtils.Playing
{
  public class AutoPlaybackManager : LogIdAble, IDisposable
  {
    private readonly Queue<byte[]> queue = new();
    private bool isPlaying = false;
    private readonly NewLogHandler logHandler;

    public AutoPlaybackManager()
    {
      this.logHandler = Logger.RegisterSender(this);
    }

    public void ClearQueue()
    {
      this.logHandler.Invoke(LogLevel.INFO, "ClearQueue() requested.");
      this.queue.Clear();
    }

    public void Dispose()
    {
      Logger.UnregisterSender(this);
      GC.SuppressFinalize(this);
    }

    public void Enqueue(byte[] bytes)
    {
      this.queue.Enqueue(bytes ?? throw new ArgumentNullException(nameof(bytes)));
      this.logHandler.Invoke(LogLevel.INFO, $"Enqueueing {bytes.Length} bytes.");
      TryPlayNext();
    }

    private void Ip_PlaybackFinished(Player sender)
    {
      lock (queue)
      {
        isPlaying = false;
        TryPlayNext();
      }
    }

    private void TryPlayNext()
    {
      this.logHandler.Invoke(LogLevel.INFO, $"TryPlayNext invoked.");
      lock (queue)
      {
        if (isPlaying || queue.Count == 0)
        {
          this.logHandler.Invoke(LogLevel.INFO, $"TryPlayNext - nothing to play, return.");
          return;
        }

        isPlaying = true;
        byte[] bytes = queue.Dequeue();
        this.logHandler.Invoke(LogLevel.INFO, $"TryPlayNext - starting play of {bytes.Length}.");
        Player player = new(bytes);
        player.PlaybackFinished += Ip_PlaybackFinished;
        player.PlayAsync();
      }
    }
  }
}
