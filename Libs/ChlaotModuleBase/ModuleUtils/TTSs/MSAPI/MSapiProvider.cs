using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs.MSAPI
{
  public class MSapiProvider : ITtsProvider
  {
    private readonly MSapiSettings settings;
    private readonly SpeechSynthesizer synthetizer;

    public MSapiProvider(MSapiSettings settings)
    {
      this.settings = settings;
      this.synthetizer = new SpeechSynthesizer()
        .WithInjectedOneCoreVoices();
      this.synthetizer.SelectVoice(settings.Voice);
      this.synthetizer.Rate = settings.Rate;
    }

    public async Task<byte[]> ConvertAsync(string text)
    {
      MemoryStream tmp = new();
      //Task a = new(() =>
      //{
      //  this.synthetizer.SetOutputToWaveStream(tmp);
      //  this.synthetizer.Speak(text);
      //});
      //a.Start();
      //await a;

      this.synthetizer.SetOutputToWaveStream(tmp);
      this.synthetizer.Speak(text);

      MemoryStream ret = new();
      if (settings.StartTrimMilisecondsTimeSpan.TotalMilliseconds > 0 || settings.EndTrimMilisecondsTimeSpan.TotalMilliseconds > 0)
        WavFileTrimmer.Trim(tmp, ret, settings.StartTrimMilisecondsTimeSpan, settings.EndTrimMilisecondsTimeSpan);
      else
        ret = tmp;

      return ret.ToArray();
    }
  }
}
