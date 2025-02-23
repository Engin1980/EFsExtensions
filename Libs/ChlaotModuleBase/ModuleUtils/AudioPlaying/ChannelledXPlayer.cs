using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.AudioPlaying
{
  public class ChannelledXPlayer
  {
    private class ChannelInfo
    {
      private const int INT_TRUE = 1;
      private const int INT_FALSE = 0;
      private readonly string channel;
      private readonly ChannelledXPlayer parent;
      private readonly ConcurrentQueue<byte[]> audioDatas = new();
      private readonly XPlayer player;
      private readonly object lck = new();
      private int isPlayingFlag = INT_FALSE;

      public ChannelInfo(ChannelledXPlayer parent, string channel)
      {
        this.channel = channel;
        this.parent = parent;
        this.player = new();
        player.PlayStarting += Player_PlayStarting;
        player.PlayStarted += Player_PlayStarted;
        player.PlayCompleted += Player_PlayCompleted;
        player.PlayRequested += Player_PlayRequested;
      }

      internal void Add(byte[] audioData)
      {
        audioDatas.Enqueue(audioData);
      }

      internal void AddAndPlay(byte[] audioData)
      {
        this.Add(audioData);
        this.Play();
      }

      internal void Play()
      {
        StartNextPlay();
      }

      private void StartNextPlay()
      {
        int isPlayingNow = Interlocked.Exchange(ref this.isPlayingFlag, INT_TRUE);
        if (isPlayingNow == INT_TRUE) return;

        if (audioDatas.TryDequeue(out byte[]? next))
        {
          player.PlayAsync(next);
        }
        else
        {
          Interlocked.Exchange(ref this.isPlayingFlag, INT_FALSE);
          this.parent.PlayAllCompleted?.Invoke(this.parent, this.channel);
        }
      }

      private void Player_PlayRequested(XPlayer sender)
      {
        this.parent.PlayRequested?.Invoke(this.parent, this.channel);
      }

      private void Player_PlayCompleted(XPlayer sender)
      {
        Interlocked.Exchange(ref this.isPlayingFlag, INT_FALSE);
        this.parent.PlayCompleted?.Invoke(this.parent, this.channel);
        StartNextPlay();
      }

      private void Player_PlayStarted(XPlayer sender)
      {
        this.parent.PlayStarted?.Invoke(this.parent, this.channel);
      }

      private void Player_PlayStarting(XPlayer sender)
      {
        this.parent.PlayInitializing?.Invoke(this.parent, this.channel);
      }
    }

    private readonly ConcurrentDictionary<string, ChannelInfo> channels = new();

    public delegate void ChannelHandler(ChannelledXPlayer player, string channel);
    public event ChannelHandler? PlayRequested;
    public event ChannelHandler? PlayInitializing;
    public event ChannelHandler? PlayStarted;
    public event ChannelHandler? PlayCompleted;
    public event ChannelHandler? PlayAllCompleted;

    public void Play(byte[] audioData, string channelName)
    {
      ChannelInfo ci = channels.GetOrAdd(channelName, _ => new ChannelInfo(this, channelName));
      ci.AddAndPlay(audioData);
    }

    public Task PlayAsync(byte[] audioData, string channelName)
    {
      Task t = new(() => Play(audioData, channelName));
      t.Start();
      return t;
    }
  }
}
