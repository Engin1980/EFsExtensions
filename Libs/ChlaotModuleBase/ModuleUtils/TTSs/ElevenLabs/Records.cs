using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs.ElevenLabs
{
  // see : https://elevenlabs.io/docs/api-reference/voices/get-default-settings
  public record struct VoiceSettings(double SimilarityBoost = 0.75, double Stability = 0.5, double Style = 0, bool UseSpeakerBoost = true);
  internal record struct HttpGetModel(
    string Text, 
    string ModelId = "eleven_multilingual_v2", 
    VoiceSettings VoiceSettings = new VoiceSettings());
}
