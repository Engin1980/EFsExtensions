using ChecklistModule.Types.Autostarts;

namespace ChecklistModule.Types
{
  public class CheckListMetaInfo
  {
    public IAutostart? Autostart { get; set; }
    public CheckDefinition CustomEntrySpeech { get; set; }
    public CheckDefinition CustomExitSpeech { get; set; }
  }
}