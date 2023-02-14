using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace ChecklistModule.Support
{
  internal class Synthetizer
  {
    private SpeechSynthesizer synthetizer;

    public Synthetizer(string voice, int rate)
    {
      this.synthetizer = new SpeechSynthesizer();
      this.synthetizer.SelectVoice(voice);
      this.synthetizer.Rate = rate;
    }

    private Synthetizer()
    {
      this.synthetizer = new();
    }

    internal static Synthetizer CreateDefault()
    {
      return new Synthetizer();
    }

    internal byte[] Generate(string value)
    {
      MemoryStream stream = new();
      this.synthetizer.SetOutputToWaveStream(stream);
      this.synthetizer.Speak(value);
      return stream.ToArray();
    }
  }
}
