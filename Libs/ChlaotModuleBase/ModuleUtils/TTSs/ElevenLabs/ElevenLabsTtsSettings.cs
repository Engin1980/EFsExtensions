using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs.ElevenLabs
{
  public class ElevenLabsTtsSettings : NotifyPropertyChanged, ITtsSettings
  {
    public string ApiKey
    {
      get { return base.GetProperty<string>(nameof(ApiKey))!; }
      set { base.UpdateProperty(nameof(ApiKey), value); }
    }

    public string VoiceId
    {
      get { return base.GetProperty<string>(nameof(VoiceId))!; }
      set { base.UpdateProperty(nameof(VoiceId), value); }
    }

    public bool IsValid
    {
      get { return base.GetProperty<bool>(nameof(IsValid))!; }
      set { base.UpdateProperty(nameof(IsValid), value); }
    }

    public ElevenLabsTtsSettings()
    {
      this.PropertyChanged += ElevenLabsTtsSettings_PropertyChanged;
    }

    private void ElevenLabsTtsSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(IsValid)) return;
      UpdateIsValid();
    }

    private void UpdateIsValid()
    {
      this.IsValid = !string.IsNullOrWhiteSpace(this.ApiKey) && !string.IsNullOrWhiteSpace(this.VoiceId);
    }

    // following things are optional
    //public string ModelId { get; set; } = "eleven_monolingual_v1";
    //public List<PronunciationDirectoryLocator> PronunociationDictionaryLocators { get; private set; } = new();
    //public VoiceSettings VoiceSettings { get; internal set; }
    //public int OptimizeStreamingLatency { get; set; } = 0;
    //public string OutputFormat { get; set; } = "mp3_44100_128";
  }
}
