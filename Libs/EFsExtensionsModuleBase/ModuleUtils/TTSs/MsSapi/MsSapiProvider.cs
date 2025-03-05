using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.AudioPlaying;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs.MsSapi
{
  public class MsSapiProvider : ITtsProvider
  {
    private readonly MsSapiSettings settings;
    private readonly SpeechSynthesizer synthetizer;

    public MsSapiProvider(MsSapiSettings settings)
    {
      this.settings = settings;
      this.synthetizer = new SpeechSynthesizer()
        .WithInjectedOneCoreVoices();
      this.synthetizer.SelectVoice(settings.Voice);
      this.synthetizer.Rate = settings.Rate;
    }

    public async Task<byte[]> ConvertAsync(string text)
    {
      Task<byte[]> t = new(() => Convert(text));
      byte[] ret = await t;
      return ret;
    }

    public byte[] Convert(string text)
    {
      MemoryStream ret = new();

      this.synthetizer.SetOutputToWaveStream(ret);
      this.synthetizer.Speak(text);

      ret = AudioUtils.TrimSilence(ret);

      return ret.ToArray();
    }
  }
}
