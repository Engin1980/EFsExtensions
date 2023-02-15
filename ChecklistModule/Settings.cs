using ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace ChecklistModule
{
  public class Settings
  {
    public class SynthetizerSettings : NotifyPropertyChangedBase
    {
      public string[] AvailableVoices { get; set; }
      public string Voice
      {
        get => base.GetProperty<string>(nameof(Voice))!;
        set => base.UpdateProperty(nameof(Voice), value);
      }

      public int Rate
      {
        get => base.GetProperty<int>(nameof(Rate))!;
        set => base.UpdateProperty(nameof(Rate), Math.Max(Math.Min(value, 10), -10));
      }

      public SynthetizerSettings()
      {
        this.AvailableVoices = new SpeechSynthesizer().GetInstalledVoices().Select(q => q.VoiceInfo.Name).ToArray();
        this.Voice = this.AvailableVoices.FirstOrDefault() ?? "";
        this.Rate = 0;
      }
    }

    public SynthetizerSettings Synthetizer { get; set; } = new();
  }
}
