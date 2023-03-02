using ChlaotModuleBase.ModuleUtils.StateChecking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopilotModule.Types
{
  public class SpeechDefinition
  {
#pragma warning disable CS8618
    public string Title { get; set; }
    public int? ReactivateIn { get; set; } = null;
    public Speech Speech { get; set; }
    public IStateCheckItem When { get; set; }
    public List<Variable> Variables { get; set; } = new();
#pragma warning restore CS8618
  }
}
