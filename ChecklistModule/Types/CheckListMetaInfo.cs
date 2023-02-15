using ChecklistModule.Types.Autostarts;

namespace ChecklistModule.Types
{
  public class CheckListMetaInfo
  {
    public IAutostart? Autostart { get; set; }
    public CheckDefinition CustomEntryCallout { get; set; }
    public CheckDefinition CustomExitCallout { get; set; }
  }
}