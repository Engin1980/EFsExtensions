using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;

namespace Eng.Chlaot.Modules.ChecklistModule.Types
{
  public class CheckListMetaInfo
  {
#pragma warning disable CS8618
    public IStateCheckItem When { get; set; }
    public CheckDefinition CustomEntrySpeech { get; set; }
    public CheckDefinition CustomExitSpeech { get; set; }
#pragma warning disable CS8618
  }
}