using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs.ElevenLabs
{
  public class ElevenLabsTtsModule : TtsModule<ElevenLabsTtsProvider, ElevenLabsTtsSettings>
  {
    //private readonly CtrSettings ctr;
    //public ITtsProvider Tts => this.ctr.Tts;
    //public string Name => 
    //public bool IsReady => Tts.IsReady;

    public ElevenLabsTtsModule()
    {
    }

    public override string Name => "ElevenLabs TTS";

    protected override ElevenLabsTtsProvider GetTypedProvider(ElevenLabsTtsSettings settings) => new ElevenLabsTtsProvider(settings);

    protected override UserControl GetTypedSettingsControl(ElevenLabsTtsSettings settings) => new CtrSettings(settings);
  }
}
