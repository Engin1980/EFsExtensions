using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs.ElevenLabs
{
  public class SettingsVM : NotifyPropertyChangedBase
  {
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

    public ElevenLabsVoice? Voice
    {
      get => base.GetProperty<ElevenLabsVoice?>(nameof(Voice))!;
      set => base.UpdateProperty(nameof(Voice), value);
    }

    public SettingsVM()
    {
      this.Voices = new();
      this.ApiKey = string.Empty;
      this.Voice = null;
    }
  }
}
