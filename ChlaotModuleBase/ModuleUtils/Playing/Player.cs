using System;
using System.Linq;
using System.Text;

namespace ChlaotModuleBase.ModuleUtils.Playing
{
  public partial class Player
  {
    private readonly PlayQueue queue = new();
    private bool isPlaying = false;

    public Player()
    {
      queue.NewItemInserted += Queue_NewItemInserted;
    }

    public void ClearQueue()
    {
      this.queue.Clear();
    }

    public void PlayAsync(byte[] bytes)
    {
      InternalPlayer ip = new(bytes);
      this.queue.Enqueue(ip);
    }

    private void Ip_PlaybackFinished(InternalPlayer sender)
    {
      lock (queue)
      {
        isPlaying = false;
        PlayNext();
      }
    }

    private void PlayNext()
    {
      lock (queue)
      {
        if (isPlaying)
        {
          return;
        }

        InternalPlayer? ip = queue.TryDequeue();
        if (ip != null)
        {
          ip.PlaybackFinished += Ip_PlaybackFinished;
          isPlaying = true;
          ip.PlayAsync();
        }
        else
        {
        }
      }
    }
    private void Queue_NewItemInserted()
    {
      PlayNext();
    }
  }
}
