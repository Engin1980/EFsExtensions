using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.AudioPlaying.ChannelAudioPlayer;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.AudioPlaying
{
  public class ChannelAudioPlayer
  {
    private record PlayRecord(byte[] AudioData, Action? OnCompleted);

    private class ChannelPlayerInfo : IDisposable
    {
      private const int INT_TRUE = 1;
      private const int INT_FALSE = 0;
      private readonly string channel;
      private readonly ConcurrentQueue<PlayRecord> audioDatas = new();
      private int isPlayingFlag = INT_FALSE;
      public delegate void ChannelCompletedHandler(string channel);
      public event ChannelCompletedHandler? ChannelCompleted;

      internal ChannelPlayerInfo(string channel)
      {
        this.channel = channel;
      }

      internal void Enqueue(byte[] audioData, Action? onCompleted)
      {
        PlayRecord playRecord = new(audioData, onCompleted);
        audioDatas.Enqueue(playRecord);
        this.StartNextPlay();
      }

      internal void Clear()
      {
        this.audioDatas.Clear();
      }

      private void StartNextPlay()
      {
        int isPlayingNow = Interlocked.Exchange(ref this.isPlayingFlag, INT_TRUE);
        if (isPlayingNow == INT_TRUE) return;

        if (audioDatas.TryDequeue(out PlayRecord? next))
        {
          AudioPlayer player = new(next.AudioData);
          player.PlayCompleted += e =>
          {
            Interlocked.Exchange(ref this.isPlayingFlag, INT_FALSE);
            next.OnCompleted?.Invoke();
            StartNextPlay();
          };
          player.PlayAsync();
        }
        else
        {
          Interlocked.Exchange(ref this.isPlayingFlag, INT_FALSE);
          this.ChannelCompleted?.Invoke(this.channel);
        }
      }

      public void Dispose()
      {
        throw new NotImplementedException();
      }
    }

    private readonly ConcurrentDictionary<string, ChannelPlayerInfo> channels = new();

    public delegate void ChannelHandler(ChannelAudioPlayer player, string channel);
    public event ChannelHandler? PlayChannelCompleted;
    public event ChannelHandler? ClearChannelCompleted;

    public void PlayAndWait(byte[] audioData, string channelName)
    {
      PlayAsync(audioData, channelName).GetAwaiter().GetResult();
    }

    public void PlayAndForget(byte[] audioData, string channelName, Action? onCompleted = null)
    {
      ChannelPlayerInfo ci = GetOrAddChannel(channelName);
      ci.Enqueue(audioData, onCompleted);
    }

    public Task PlayAsync(byte[] audioData, string channelName)
    {
      Task t = new(() =>
      {
        bool isDone = false;

        this.PlayAndForget(audioData, channelName, () => isDone = true);

        while (!isDone)
        {
          Thread.Sleep(100);
        }
      });
      t.Start();
      return t;
    }

    private ChannelPlayerInfo GetOrAddChannel(string channelName)
    {
      lock (channels)
      {
        if (channels.ContainsKey(channelName) == false)
        {
          var cpi = new ChannelPlayerInfo(channelName);
          cpi.ChannelCompleted += channelName => this.PlayChannelCompleted?.Invoke(this, channelName);
          channels[channelName] = cpi;
        }
      }
      ChannelPlayerInfo ret = channels[channelName];
      return ret;
    }

    public void Clear(string channelName)
    {
      ChannelPlayerInfo ci = GetOrAddChannel(channelName);
      ci.Clear();
      this.ClearChannelCompleted?.Invoke(this, channelName);
    }
  }
}
