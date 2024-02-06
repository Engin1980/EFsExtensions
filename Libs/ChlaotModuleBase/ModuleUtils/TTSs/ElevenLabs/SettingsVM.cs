using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs.ElevenLabs
{
  internal class SettingsVM : NotifyPropertyChangedBase
  {
    public class VoiceSettingsVM : NotifyPropertyChangedBase
    {
      public double SimilarityBoost
      {
        get => base.GetProperty<double>(nameof(SimilarityBoost))!;
        set => base.UpdateProperty(nameof(SimilarityBoost), value);
      }

      public double Stability
      {
        get => base.GetProperty<double>(nameof(Stability))!;
        set => base.UpdateProperty(nameof(Stability), value);
      }

      public VoiceSettingsVM()
      {
        this.SimilarityBoost = 0.5;
        this.Stability = 0.75;
      }
    }

    public string ApiKey
    {
      get => base.GetProperty<string>(nameof(ApiKey))!;
      set => base.UpdateProperty(nameof(ApiKey), value);
    }

    public List<ElevenLabsVoice> Voices
    {
      get => base.GetProperty<List<ElevenLabsVoice>>(nameof(Voices))!;
      set => base.UpdateProperty(nameof(Voices), value);
    }

    public ElevenLabsVoice Voice
    {
      get => base.GetProperty<ElevenLabsVoice>(nameof(Voice))!;
      set => base.UpdateProperty(nameof(Voice), value);
    }

    public VoiceSettingsVM VoiceSettings
    {
      get => base.GetProperty<VoiceSettingsVM>(nameof(VoiceSettings))!;
      set => base.UpdateProperty(nameof(VoiceSettings), value);
    }
  }
}
