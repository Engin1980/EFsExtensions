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

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs.MsSapi
{
  public class MsSapiSettings : NotifyPropertyChanged, ITtsSettings
  {
    private readonly static string[] StaticAvailableVoices = new SpeechSynthesizer()
      .WithInjectedOneCoreVoices()
      .GetInstalledVoices()
      .Select(q => q.VoiceInfo.Name)
      .ToArray();

    public MsSapiSettings()
    {
      this.PropertyChanged += (s, e) =>
      {
        if (e.PropertyName == nameof(IsValid)) return;
        this.IsValid = this.Voice != null;
      };

      this.AvailableVoices = StaticAvailableVoices;
      this.Voice = this.AvailableVoices.FirstOrDefault() ?? "";
      this.Rate = 0;
    }

    [XmlIgnore]
    public string[] AvailableVoices { get; set; }

    public int Rate
    {
      get => base.GetProperty<int>(nameof(Rate))!;
      set => base.UpdateProperty(nameof(Rate), Math.Max(Math.Min(value, 10), -10));
    }
    public string Voice
    {
      get => base.GetProperty<string>(nameof(Voice))!;
      set => base.UpdateProperty(nameof(Voice), value);
    }

    [XmlIgnore]
    public bool IsValid
    {
      get { return base.GetProperty<bool>(nameof(IsValid))!; }
      set { base.UpdateProperty(nameof(IsValid), value); }
    }

    public string CreateSettingsString() => $"{Voice}";

    public void LoadFromSettingsString(string str)
    {
      Voice = str;
    }
  }
}
