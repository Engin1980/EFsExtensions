using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs.ElevenLabs
{
  public class ElevenLabsTtsSettings : NotifyPropertyChanged, ITtsSettings
  {
    private const double STYLE_MIN = 0.0;
    private const double STYLE_MAX = 0.5;
    private const double STABILITY_MIN = 0.3;
    private const double STABILITY_MAX = 1;
    private const double SIMILARITY_MIN = 0;
    private const double SIMILARITY_MAX = 1;

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


    public string? ModelId
    {
      get { return base.GetProperty<string?>(nameof(ModelId))!; }
      set { base.UpdateProperty(nameof(ModelId), value); }
    }




    public double Style
    {
      get { return base.GetProperty<double>(nameof(Style))!; }
      set { base.UpdateProperty(nameof(Style), EnsureIn(STYLE_MIN, value, STYLE_MAX)); }
    }


    public double Stability
    {
      get { return base.GetProperty<double>(nameof(Stability))!; }
      set { base.UpdateProperty(nameof(Stability), EnsureIn(STABILITY_MIN, value, STABILITY_MAX)); }
    }


    public double Similarity
    {
      get { return base.GetProperty<double>(nameof(Similarity))!; }
      set { base.UpdateProperty(nameof(Similarity), EnsureIn(SIMILARITY_MIN, value, SIMILARITY_MAX)); }
    }

    private double EnsureIn(double min, double val, double max) =>
      Math.Max(min, Math.Min(max, val));


    public bool IsValid
    {
      get { return base.GetProperty<bool>(nameof(IsValid))!; }
      set { base.UpdateProperty(nameof(IsValid), value); }
    }

    public ElevenLabsTtsSettings()
    {
      this.PropertyChanged += ElevenLabsTtsSettings_PropertyChanged;
      this.Style = 1;
      this.Stability = 0.5;
      this.Similarity = 0.5;
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

    public void LoadFromSettingsString(string str)
    {
      string[] pts = str.Split(";");
      ApiKey = pts[0];
      VoiceId = pts[1];
      Style = double.Parse(pts[2]);
      Stability = double.Parse(pts[3]);
      Similarity = double.Parse(pts[4]);
    }

    public string CreateSettingsString() => $"{ApiKey};{VoiceId};{Style};{Stability};{Similarity}";

    // following things are optional
    //public string ModelId { get; set; } = "eleven_monolingual_v1";
    //public List<PronunciationDirectoryLocator> PronunociationDictionaryLocators { get; private set; } = new();
    //public VoiceSettings VoiceSettings { get; internal set; }
    //public int OptimizeStreamingLatency { get; set; } = 0;
    //public string OutputFormat { get; set; } = "mp3_44100_128";
  }
}
