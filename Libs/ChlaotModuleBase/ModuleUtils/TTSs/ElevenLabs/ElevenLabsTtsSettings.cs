using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs.ElevenLabs
{
  public class ElevenLabsTtsSettings
  {
    public string API { get; set; } = string.Empty;
    public string VoiceId { get; set; } = string.Empty;

    // following things are optional
    //public string ModelId { get; set; } = "eleven_monolingual_v1";
    //public List<PronunciationDirectoryLocator> PronunociationDictionaryLocators { get; private set; } = new();
    //public VoiceSettings VoiceSettings { get; internal set; }
    //public int OptimizeStreamingLatency { get; set; } = 0;
    //public string OutputFormat { get; set; } = "mp3_44100_128";
  }
}
