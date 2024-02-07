using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs.ElevenLabs
{
  public record struct VoiceSettings(double SimilarityBoost = 0.5, double Stability = 0.75, int Style = 0, bool UseSpeakerBoost = true);
  internal record struct HttpGetModel(string Text);

  /*
   * '{
        "model_id": "<string>",
        "pronunciation_dictionary_locators": [
          {
            "pronunciation_dictionary_id": "<string>",
            "version_id": "<string>"
          }
        ],
        "text": "<string>",
        "voice_settings": {
          "similarity_boost": 123,
          "stability": 123,
          "style": 123,
          "use_speaker_boost": true
        }
      }'
   * */
}
