using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs.ElevenLabs
{
  internal class VoicesResponse
  {
    public ElevenLabsVoice[] Voices { get; set; } = null!;
  }

  public class ElevenLabsVoice
  {
    public string VoiceId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public object Samples { get; set; } = null!;
    public string Category { get; set; } = null!;
    public FineTuning FineTuning { get; set; } = null!;
    public Labels Labels { get; set; } = null!;
    public object Description { get; set; } = null!;
    public string PreviewUrl { get; set; } = null!;
    public object[] AvailableForTiers { get; set; } = null!;
    public object Settings { get; set; } = null!;
    public object Sharing { get; set; } = null!;
    public string[] HighQualityBaseModelIds { get; set; } = null!;
  }

  public class FineTuning
  {
    public bool IsAllowedToFineTune { get; set; }
    public string FinetuningState { get; set; } = null!;
    public object[] VerificationFailures { get; set; } = null!;
    public int VerificationAttemptsCount { get; set; }
    public bool ManualVerificationRequested { get; set; }
    public object Language { get; set; } = null!;
    public object FinetuningProgress { get; set; } = null!;
    public object Message { get; set; } = null!;
    public object DatasetDurationSeconds { get; set; } = null!;
    public object VerificationAttempts { get; set; } = null!;
    public object SliceIds { get; set; } = null!;
    public object ManualVerification { get; set; } = null!;
  }

  public class Labels
  {
    public string Accent { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Age { get; set; } = null!;
    public string Gender { get; set; } = null!;
    public string Usecase { get; set; } = null!;
  }

}
