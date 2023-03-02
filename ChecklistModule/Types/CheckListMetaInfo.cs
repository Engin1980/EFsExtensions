using ChlaotModuleBase.ModuleUtils.StateChecking;

namespace ChecklistModule.Types
{
  public class CheckListMetaInfo
  {
#pragma warning disable CS8618
    public IStateCheckItem Autostart { get; set; }
    public CheckDefinition CustomEntrySpeech { get; set; }
    public CheckDefinition CustomExitSpeech { get; set; }
#pragma warning disable CS8618
  }
}