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
    public const string CHANNEL_COPILOT = "COPILOT";
    public const string CHANNEL_AIRPLANE = "AIRPLANE";
    private static int nextChannelId = 0;
    private readonly Logger logger;
    private readonly ChannelAudioPlayer cap = new ChannelAudioPlayer();

    public AutoPlaybackManager()
    {
      this.logger = Logger.Create(this);
    }

    public void ClearQueue(string channel)
    {
      if (channel == null) throw new ArgumentNullException(nameof(channel));
      this.logger.Log(LogLevel.INFO, $"ClearQueue() over channel '{channel}' requested.");
      this.cap.Clear(channel);
      this.logger.Log(LogLevel.INFO, $"ClearQueue() over channel '{channel}' completed.");
    }

    public void Dispose()
    {
      Logger.UnregisterSender(this);
      GC.SuppressFinalize(this);
    }

    public void Enqueue(byte[] bytes, string? channel)
    {
      channel ??= $"Generated_{nextChannelId++}";
      this.logger.Log(LogLevel.INFO, $"Enqueueing {bytes.Length} bytes in channel '{channel}'.");
      this.cap.PlayAsync(bytes, channel);
    }
  }
}
