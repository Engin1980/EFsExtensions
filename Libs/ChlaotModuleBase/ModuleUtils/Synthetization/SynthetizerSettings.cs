using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Synthetization
{
  public class SynthetizerSettings : NotifyPropertyChanged
  {
    [XmlIgnore]
    public string[] AvailableVoices { get; set; }
    public int EndTrimMiliseconds
    {
      get => base.GetProperty<int>(nameof(EndTrimMiliseconds))!;
      set => base.UpdateProperty(nameof(EndTrimMiliseconds), Math.Max(value, 0));
    }

    public TimeSpan EndTrimMilisecondsTimeSpan { get => TimeSpan.FromMilliseconds(EndTrimMiliseconds); }

    public int Rate
    {
      get => base.GetProperty<int>(nameof(Rate))!;
      set => base.UpdateProperty(nameof(Rate), Math.Max(Math.Min(value, 10), -10));
    }

    public int StartTrimMiliseconds
    {
      get => base.GetProperty<int>(nameof(StartTrimMiliseconds))!;
      set => base.UpdateProperty(nameof(StartTrimMiliseconds), Math.Max(value, 0));
    }

    public TimeSpan StartTrimMilisecondsTimeSpan { get => TimeSpan.FromMilliseconds(StartTrimMiliseconds); }

    public string Voice
    {
      get => base.GetProperty<string>(nameof(Voice))!;
      set => base.UpdateProperty(nameof(Voice), value);
    }
    public SynthetizerSettings()
    {
      this.AvailableVoices = new SpeechSynthesizer().GetInstalledVoices().Select(q => q.VoiceInfo.Name).ToArray();
      this.Voice = this.AvailableVoices.FirstOrDefault() ?? "";
      this.Rate = 0;
      this.StartTrimMiliseconds = 0;
      this.EndTrimMiliseconds = 750;
    }
  }
}
