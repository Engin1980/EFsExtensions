using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Media;
using System.Numerics;
using System.Speech.Synthesis.TtsEngine;
using System.Text;
using System.Threading.Tasks;

namespace ChecklistModule.Support
{
  public class InternalPlayer
  {
    public delegate void PlayerDelegate(InternalPlayer sender);

    public event PlayerDelegate? PlaybackFinished;

    public event PlayerDelegate? PlaybackStarted;

    public readonly int length;
    private readonly SoundPlayer soundPlayer;
    public InternalPlayer(byte[] bytes)
    {
      MemoryStream stream = new(bytes);
      this.soundPlayer = new SoundPlayer(stream);
      this.length = bytes.Length;
    }
    public void Play()
    {
      PlaybackStarted?.Invoke(this);
      this.soundPlayer.PlaySync();
      PlaybackFinished?.Invoke(this);
    }

    internal void PlayAsync()
    {
      Task t = new(this.Play);
      t.Start();
    }
  }

  public class Player
  {
    private class PlayQueue
    {
      public delegate void ChangedDelegate();
      public event ChangedDelegate? NewItemInserted;
      private readonly List<InternalPlayer> inner = new();

      internal void Clear()
      {
        lock (inner)
        {
          this.inner.Clear();
        }
      }

      internal void Enqueue(InternalPlayer ip)
      {
        lock (inner)
        {
          inner.Add(ip);
          this.NewItemInserted?.Invoke();
        }
      }

      internal InternalPlayer? TryDequeue()
      {
        InternalPlayer? ret = null;
        lock (inner)
        {
          if (inner.Count > 0)
          {
            ret = inner[0];
            inner.RemoveAt(0);
          }
        }
        return ret;
      }
    }

    private readonly PlayQueue queue = new();
    private bool isPlaying = false;

    public Player()
    {
      queue.NewItemInserted += Queue_NewItemInserted;
    }

    internal void ClearQueue()
    {
      this.queue.Clear();
    }

    internal void PlayAsync(byte[] bytes)
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
