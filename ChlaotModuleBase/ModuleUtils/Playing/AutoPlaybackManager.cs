using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChlaotModuleBase.ModuleUtils.Playing
{
  public class AutoPlaybackManager
  {
    private readonly Queue<byte[]> queue = new();
    private bool isPlaying = false;

    public void ClearQueue()
    {
      this.queue.Clear();
    }

    public void Enqueue(byte[] bytes)
    {
      this.queue.Enqueue(bytes ?? throw new ArgumentNullException(nameof(bytes)));
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
      lock (queue)
      {
        if (isPlaying || queue.Count == 0)
        {
          return;
        }

        isPlaying = true;
        byte[] bytes = queue.Dequeue();
        Player player = new(bytes);
        player.PlaybackFinished += Ip_PlaybackFinished;
        player.PlayAsync();
      }
    }
  }
}
