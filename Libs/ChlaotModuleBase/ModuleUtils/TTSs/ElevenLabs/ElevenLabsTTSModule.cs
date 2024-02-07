using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs.ElevenLabs
{
  public class ElevenLabsTtsModule : ITtsModule
  {
    private readonly CtrSettings ctr;
    public ITts Tts => this.ctr.Tts;
    public string Name => "ElevenLabs TTS";

    public ElevenLabsTtsModule()
    {
      this.ctr = new CtrSettings();
    }

    public UserControl SettingsControl => ctr;
  }
}
