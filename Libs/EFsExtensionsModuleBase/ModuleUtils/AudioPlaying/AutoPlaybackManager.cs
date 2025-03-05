using ELogging;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.AudioPlaying;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.AudioPlaying
{
  public class AudioPlayManager : IDisposable
  {
    public const string CHANNEL_COPILOT = "COPILOT";
    public const string CHANNEL_AIRPLANE = "AIRPLANE";
    public const string CHANNEL_STEWARD = "STEWARD";
    private static int nextChannelId = 0;
    private readonly Logger logger;
    private readonly ChannelAudioPlayer channelAudioPlayer = new();

    public AudioPlayManager()
    {
      this.logger = Logger.Create(this);
    }

    public void ClearQueue(string channel)
    {
      if (channel == null) throw new ArgumentNullException(nameof(channel));
      this.logger.Log(LogLevel.INFO, $"ClearQueue() over channel '{channel}' requested.");
      this.channelAudioPlayer.Clear(channel);
      this.logger.Log(LogLevel.INFO, $"ClearQueue() over channel '{channel}' completed.");
    }

    public void Dispose()
    {
      Logger.UnregisterSender(this);
      GC.SuppressFinalize(this);
    }

    public void Enqueue(byte[] bytes, string? channel = null, Action? onCompletedCallback = null)
    {
      channel ??= $"Generated_{nextChannelId++}";
      this.logger.Log(LogLevel.INFO, $"Enqueueing {bytes.Length} bytes in channel '{channel}'.");
      Task<PlayId> t = this.channelAudioPlayer.PlayAsync(bytes, channel);
      if (onCompletedCallback != null)
      {
        Task tt = t.ContinueWith(q => onCompletedCallback());
      }
    }
  }
}
