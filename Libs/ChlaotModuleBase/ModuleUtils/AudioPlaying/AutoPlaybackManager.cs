using ELogging;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.AudioPlaying;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.AudioPlaying
{
  public class AutoPlaybackManager : IDisposable
  {
    private readonly Queue<byte[]> queue = new();
    private bool isPlaying = false;
    private readonly Logger logger;

    public AutoPlaybackManager()
    {
      this.logger = Logger.Create(this);
    }

    public void ClearQueue()
    {
      this.logger.Log(LogLevel.INFO, "ClearQueue() requested.");
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
      this.logger.Log(LogLevel.INFO, $"Enqueueing {bytes.Length} bytes.");
      TryPlayNext();
    }

    private void Player_PlayCompleted(XPlayer sender)
    {
      lock (queue)
      {
        isPlaying = false;
        TryPlayNext();
      }
    }

    private void TryPlayNext()
    {
      this.logger.Log(LogLevel.INFO, $"TryPlayNext invoked.");
      lock (queue)
      {
        if (isPlaying || queue.Count == 0)
        {
          this.logger.Log(LogLevel.INFO, $"TryPlayNext - nothing to play, return.");
          return;
        }

        isPlaying = true;
        byte[] bytes = queue.Dequeue();
        this.logger.Log(LogLevel.INFO, $"TryPlayNext - starting play of {bytes.Length}.");
        XPlayer player = new(bytes);
        player.PlayCompleted += Player_PlayCompleted;
        player.PlayAsync();
      }
    }
  }
}
