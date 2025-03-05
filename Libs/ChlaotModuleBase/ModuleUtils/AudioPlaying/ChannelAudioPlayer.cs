using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.AudioPlaying.ChannelAudioPlayer;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.AudioPlaying
{
  public class ChannelAudioPlayer
  {
    private record PlayRecord(PlayId Id, byte[] data);

    private class ChannelPlayerInfo : IDisposable
    {
      private static int nextPlayId = 1;
      private const int INT_TRUE = 1;
      private const int INT_FALSE = 0;
      private readonly string channel;
      private readonly ChannelAudioPlayer parent;
      private readonly ConcurrentQueue<PlayRecord> audioDatas = new();
      private int isPlayingFlag = INT_FALSE;

      private static PlayId GetNextPlayId()
      {
        int id = Interlocked.Increment(ref ChannelPlayerInfo.nextPlayId);
        PlayId ret = new(id);
        return ret;
      }

      internal ChannelPlayerInfo(ChannelAudioPlayer parent, string channel)
      {
        this.channel = channel;
        this.parent = parent;
      }

      internal PlayId Add(byte[] audioData)
      {
        PlayId playId = GetNextPlayId();
        PlayRecord playRecord = new(playId, audioData);
        audioDatas.Enqueue(playRecord);
        return playId;
      }

      internal PlayId AddAndPlay(byte[] audioData)
      {
        var ret = this.Add(audioData);
        this.Play();
        return ret;
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

        if (audioDatas.TryDequeue(out PlayRecord? next))
        {
          AudioPlayer player = new(next.data);
          player.PlayStarting += s => Player_PlayStarting(s, next.Id);
          player.PlayStarted += s => Player_PlayStarted(s, next.Id);
          player.PlayCompleted += s => Player_PlayCompleted(s, next.Id);
          player.PlayRequested += s => Player_PlayRequested(s, next.Id);
          player.PlayAsync();
        }
        else
        {
          Interlocked.Exchange(ref this.isPlayingFlag, INT_FALSE);
          this.parent.PlayChannelCompleted?.Invoke(this.parent, this.channel);
        }
      }

      private void Player_PlayRequested(AudioPlayer sender, PlayId playId)
      {
        this.parent.PlayRequested?.Invoke(this.parent, this.channel, playId);
      }

      private void Player_PlayCompleted(AudioPlayer sender, PlayId playId)
      {
        Interlocked.Exchange(ref this.isPlayingFlag, INT_FALSE);
        this.parent.PlayCompleted?.Invoke(this.parent, this.channel, playId);
        StartNextPlay();
      }

      private void Player_PlayStarted(AudioPlayer sender, PlayId playId)
      {
        this.parent.PlayStarted?.Invoke(this.parent, this.channel, playId);
      }

      private void Player_PlayStarting(AudioPlayer sender, PlayId playId)
      {
        this.parent.PlayInitializing?.Invoke(this.parent, this.channel, playId);
      }

      public void Dispose()
      {
        throw new NotImplementedException();
      }
    }

    private readonly ConcurrentDictionary<string, ChannelPlayerInfo> channels = new();

    public delegate void ChannelPlayIdHandler(ChannelAudioPlayer player, string channel, PlayId playId);
    public delegate void ChannelHandler(ChannelAudioPlayer player, string channel);
    public event ChannelPlayIdHandler? PlayRequested;
    public event ChannelPlayIdHandler? PlayInitializing;
    public event ChannelPlayIdHandler? PlayStarted;
    public event ChannelPlayIdHandler? PlayCompleted;
    public event ChannelHandler? PlayChannelCompleted;
    public event ChannelHandler? ClearChannelCompleted;

    public PlayId Play(byte[] audioData, string channelName)
    {
      ChannelPlayerInfo ci = channels.GetOrAdd(channelName, _ => new ChannelPlayerInfo(this, channelName));
      var ret = ci.AddAndPlay(audioData);
      return ret;
    }

    public Task<PlayId> PlayAsync(byte[] audioData, string channelName)
    {
      Task<PlayId> t = new(() => Play(audioData, channelName));
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
