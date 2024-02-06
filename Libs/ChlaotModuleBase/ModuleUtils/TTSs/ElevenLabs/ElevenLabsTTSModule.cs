using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs.ElevenLabs
{
  public class ElevenLabsTTSModule : ITTSModule
  {
    private ElevenLabsTts tts = new();
    private readonly 
    public ITTS TTS => tts;

    public UserControl SettingsControl => throw new NotImplementedException();
  }
}
