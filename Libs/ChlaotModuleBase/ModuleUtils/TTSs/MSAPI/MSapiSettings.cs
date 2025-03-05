using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs.MSAPI
{
  public class MSapiSettings : NotifyPropertyChanged, ITtsSettings
  {
    private readonly static string[] StaticAvailableVoices = new SpeechSynthesizer()
      .WithInjectedOneCoreVoices()
      .GetInstalledVoices()
      .Select(q => q.VoiceInfo.Name)
      .ToArray();

    public MSapiSettings()
    {
      this.PropertyChanged += (s, e) =>
      {
        if (e.PropertyName == nameof(IsValid)) return;
        this.IsValid = this.Voice != null;
      };

      this.AvailableVoices = StaticAvailableVoices;
      this.Voice = this.AvailableVoices.FirstOrDefault() ?? "";
      this.Rate = 0;
      this.StartTrimMiliseconds = 0;
      this.EndTrimMiliseconds = 750;

    }

    [XmlIgnore]
    public string[] AvailableVoices { get; set; }
    public int EndTrimMiliseconds
    {
      get => base.GetProperty<int>(nameof(EndTrimMiliseconds))!;
      set => base.UpdateProperty(nameof(EndTrimMiliseconds), Math.Max(value, 0));
    }

    public TimeSpan EndTrimMilisecondsTimeSpan => TimeSpan.FromMilliseconds(EndTrimMiliseconds);

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

    public TimeSpan StartTrimMilisecondsTimeSpan => TimeSpan.FromMilliseconds(StartTrimMiliseconds);

    public string Voice
    {
      get => base.GetProperty<string>(nameof(Voice))!;
      set => base.UpdateProperty(nameof(Voice), value);
    }
    public bool IsValid
    {
      get { return base.GetProperty<bool>(nameof(IsValid))!; }
      set { base.UpdateProperty(nameof(IsValid), value); }
    }

    public string CreateSettingsString() => $"{Voice};{StartTrimMiliseconds};{EndTrimMiliseconds}";

    public void LoadFromSettingsString(string str)
    {
      string[] pts = str.Split(";");
      Voice = pts[0];
      StartTrimMiliseconds = int.Parse(pts[1]);
      EndTrimMiliseconds = int.Parse(pts[2]);
    }
  }
}
