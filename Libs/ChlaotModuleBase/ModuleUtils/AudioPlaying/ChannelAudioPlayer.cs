using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.AudioPlaying
{
  public class ChannelAudioPlayer
  {
    private class ChannelPlayerInfo : IDisposable
    {
      private const int INT_TRUE = 1;
      private const int INT_FALSE = 0;
      private readonly string channel;
      private readonly ChannelAudioPlayer parent;
      private readonly ConcurrentQueue<byte[]> audioDatas = new();
      private int isPlayingFlag = INT_FALSE;

      internal ChannelPlayerInfo(ChannelAudioPlayer parent, string channel)
      {
        this.channel = channel;
        this.parent = parent;        
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

      internal void Clear()
      {
        this.audioDatas.Clear();
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
          AudioPlayer player = new(next);
          player.PlayStarting += Player_PlayStarting;
          player.PlayStarted += Player_PlayStarted;
          player.PlayCompleted += Player_PlayCompleted;
          player.PlayRequested += Player_PlayRequested;
          player.PlayAsync();
        }
        else
        {
          Interlocked.Exchange(ref this.isPlayingFlag, INT_FALSE);
          this.parent.PlayChannelCompleted?.Invoke(this.parent, this.channel);
        }
      }

      private void Player_PlayRequested(AudioPlayer sender)
      {
        this.parent.PlayRequested?.Invoke(this.parent, this.channel);
      }

      private void Player_PlayCompleted(AudioPlayer sender)
      {
        Interlocked.Exchange(ref this.isPlayingFlag, INT_FALSE);
        this.parent.PlayCompleted?.Invoke(this.parent, this.channel);
        StartNextPlay();
      }

      private void Player_PlayStarted(AudioPlayer sender)
      {
        this.parent.PlayStarted?.Invoke(this.parent, this.channel);
      }

      private void Player_PlayStarting(AudioPlayer sender)
      {
        this.parent.PlayInitializing?.Invoke(this.parent, this.channel);
      }

      public void Dispose()
      {
        throw new NotImplementedException();
      }
    }

    private readonly ConcurrentDictionary<string, ChannelPlayerInfo> channels = new();

    public delegate void ChannelHandler(ChannelAudioPlayer player, string channel);
    public event ChannelHandler? PlayRequested;
    public event ChannelHandler? PlayInitializing;
    public event ChannelHandler? PlayStarted;
    public event ChannelHandler? PlayCompleted;
    public event ChannelHandler? PlayChannelCompleted;
    public event ChannelHandler? ClearChannelCompleted;

    public void Play(byte[] audioData, string channelName)
    {
      ChannelPlayerInfo ci = channels.GetOrAdd(channelName, _ => new ChannelPlayerInfo(this, channelName));
      ci.AddAndPlay(audioData);
    }

    public Task PlayAsync(byte[] audioData, string channelName)
    {
      Task t = new(() => Play(audioData, channelName));
      t.Start();
      return t;
    }

    public void Clear(string channelName)
    {
      ChannelPlayerInfo ci = channels.GetOrAdd(channelName, _ => new ChannelPlayerInfo(this, channelName));
      ci.Clear();
      this.ClearChannelCompleted?.Invoke(this, channelName);
    }
  }
}
