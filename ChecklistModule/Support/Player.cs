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
  internal class Player
  {
    private class InternalPlayer
    {
      private readonly SoundPlayer soundPlayer;
      public readonly int length;

      public InternalPlayer(byte[] bytes)
      {
        MemoryStream stream = new(bytes);
        this.soundPlayer = new SoundPlayer(stream);
        this.length = bytes.Length;
      }

      public delegate void PlayerDelegate(InternalPlayer sender);
      public event PlayerDelegate? PlaybackStarted;
      public event PlayerDelegate? PlaybackFinished;

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

    private class PlayQueue
    {
      public delegate void ChangedDelegate();
      public event ChangedDelegate? NewItemInserted;
      private readonly List<InternalPlayer> inner = new();

      internal void Enqueue(InternalPlayer ip)
      {
        Trace.WriteLine("Q.Enqueue - before lock " + ip.length);
        lock (inner)
        {
          Trace.WriteLine("Q.Enqueue - in lock " + ip.length);
          inner.Add(ip);
          Trace.WriteLine("Q.Enqueue - in lock before NewItemInserted " + ip.length);
          this.NewItemInserted?.Invoke();
          Trace.WriteLine("Q.Enqueue - in lock after NewItemInserted " + ip.length);
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

      internal void Clear()
      {
        lock (inner)
        {
          this.inner.Clear();
        }
      }
    }

    private readonly PlayQueue queue = new();
    private bool isPlaying = false;

    public Player()
    {
      queue.NewItemInserted += Queue_NewItemInserted;
    }

    private void Queue_NewItemInserted()
    {
      Trace.WriteLine("Player.Queue_NewItemInserted");
      PlayNext();
    }

    private void PlayNext()
    {
      Trace.WriteLine("Player.PlayNext - before lock");
      lock (queue)
      {
        Trace.WriteLine("Player.PlayNext - isPlaying check");
        if (isPlaying)
        {
          Trace.WriteLine("Player.PlayNext - isPlaying=true, returning");
          return;
        }

        Trace.WriteLine("Player.PlayNext - isPlaying=false, trying to get next IP");
        InternalPlayer? ip = queue.TryDequeue();
        if (ip != null)
        {
          Trace.WriteLine("Player.PlayNext - there is IP " + ip.length);
          ip.PlaybackFinished += Ip_PlaybackFinished;
          Trace.WriteLine("Player.PlayNext - switch isPlaying to true " + ip.length);
          isPlaying = true;
          Trace.WriteLine("Player.PlayNext - IP.PlayAsync " + ip.length);
          ip.PlayAsync();
        }
        else
        {
          Trace.WriteLine("Player.PlayNext - no next IP, returning");
        }
      }
    }

    private void Ip_PlaybackFinished(InternalPlayer sender)
    {
      Trace.WriteLine("Player.Ip_PlaybackFinished - before lock " + sender.length);
      lock (queue)
      {
        Trace.WriteLine("Player.Ip_PlaybackFinished - in lock, setting isPlaying to false " + sender.length);
        isPlaying = false;
        Trace.WriteLine("Player.Ip_PlaybackFinished - invoking PlayNext() " + sender.length);
        PlayNext();
      }
    }

    internal void PlayAsync(byte[] bytes)
    {
      Trace.WriteLine("PlayAsync start " + bytes.Length);
      InternalPlayer ip = new(bytes);
      Trace.WriteLine("PlayAsync enqueing " + bytes.Length);
      this.queue.Enqueue(ip);
      Trace.WriteLine("PlayAsync start completed " + bytes.Length);
    }

    internal void ClearQueue()
    {
      this.queue.Clear();
    }
  }
}
